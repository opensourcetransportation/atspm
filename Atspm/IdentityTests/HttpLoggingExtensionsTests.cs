// SPDX-License-Identifier: Apache-2.0

#nullable enable

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Xunit;

namespace Utah.Udot.Atspm.IdentityTests;

public class HttpLoggingExtensionsTests
{
    [Fact]
    public void AddAtspmHttpLogging_ExcludesRequestAndResponseSecrets()
    {
        using var services = new ServiceCollection()
            .AddAtspmHttpLogging()
            .BuildServiceProvider();

        var options = services.GetRequiredService<IOptions<HttpLoggingOptions>>().Value;
        var sensitiveFields = HttpLoggingFields.RequestPath
            | HttpLoggingFields.RequestQuery
            | HttpLoggingFields.RequestHeaders
            | HttpLoggingFields.RequestBody
            | HttpLoggingFields.ResponseHeaders
            | HttpLoggingFields.ResponseBody;

        Assert.Equal(HttpLoggingFields.None, options.LoggingFields & sensitiveFields);
        Assert.Equal(
            HttpLoggingFields.RequestMethod | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration,
            options.LoggingFields);
        Assert.True(options.CombineLogs);
    }

    [Fact]
    public async Task HttpLoggingMiddleware_DoesNotRecordCanaryCredentials()
    {
        var entries = new ConcurrentQueue<string>();
        using var services = new ServiceCollection()
            .AddLogging(builder => builder.AddProvider(new CaptureLoggerProvider(entries)))
            .AddAtspmHttpLogging()
            .BuildServiceProvider();

        var app = new ApplicationBuilder(services);
        app.UseHttpLogging();
        app.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"token\":\"response-canary-token\"}");
        });

        var context = new DefaultHttpContext { RequestServices = services };
        context.Request.Method = "POST";
        context.Request.Path = "/login/path-canary";
        context.Request.QueryString = new QueryString("?token=query-canary-token");
        context.Request.Headers.Authorization = "Bearer header-canary-token";
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{\"password\":\"body-canary-password\"}"));
        context.Response.Body = new MemoryStream();

        await app.Build()(context);

        var output = string.Join("\n", entries);
        Assert.Contains("POST", output);
        Assert.Contains("200", output);
        Assert.DoesNotContain("path-canary", output);
        Assert.DoesNotContain("query-canary", output);
        Assert.DoesNotContain("header-canary", output);
        Assert.DoesNotContain("body-canary", output);
        Assert.DoesNotContain("response-canary", output);
    }

    private sealed class CaptureLoggerProvider(ConcurrentQueue<string> entries) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new CaptureLogger(entries);
        public void Dispose() { }
    }

    private sealed class CaptureLogger(ConcurrentQueue<string> entries) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoopScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter) => entries.Enqueue(formatter(state, exception));
    }

    private sealed class NoopScope : IDisposable
    {
        public static readonly NoopScope Instance = new();
        public void Dispose() { }
    }
}
