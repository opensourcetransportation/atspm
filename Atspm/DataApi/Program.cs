#region license
// Copyright 2026 Utah Departement of Transportation
// for DataApi - %Namespace%/Program.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.DataApi.CustomOperations;
using Utah.Udot.Atspm.Infrastructure.Common;

//git 2

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ApplyVolumeConfiguration()
    .ConfigureLogging((h, l) => l.AddGoogle(h))
    .ConfigureServices((h, s) =>
    {
        s.AddControllers(o =>
        {
            o.ReturnHttpNotAcceptable = true;
            o.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
            //o.Filters.Add(new ProducesAttribute("application/json", "application/xml"));
            //o.OutputFormatters.Add(new EventLogCsvFormatter());
            o.OutputFormatters.RemoveType<StringOutputFormatter>();
        })
        .AddNewtonsoftJson();
        s.AddProblemDetails();
        s.AddConfiguredCompression(new[] { "application/json", "application/xml", "text/csv", "application/x-ndjson" });
        s.AddConfiguredSwagger(builder.Configuration, o =>
        {
            o.IncludeXmlComments(typeof(Program).Assembly);
            o.CustomOperationIds((controller, verb, action) => $"{verb}{controller}{action}");
            o.CustomSchemaIds(type => type.Name);
            o.EnableAnnotations();
            o.AddAtspmSecurityDefinitions();
            //o.OperationFilter<TimestampFormatHeader>();
            o.OperationFilter<DataTypeEnumOperationFilter>();
            o.DocumentFilter<GenerateAggregationSchemas>();
            o.DocumentFilter<GenerateEventSchemas>();
        });
        s.AddConfiguredCors(builder.Configuration);
        s.AddAtspmHttpLogging();
        s.AddAtspmDbContext(h);
        s.AddAtspmEFConfigRepositories();
        s.AddAtspmEFEventLogRepositories();
        s.AddAtspmEFAggregationRepositories();
        s.AddPathBaseFilter(h);
        s.AddAtspmIdentity(h);
        s.AddHealthChecks();
    });

var app = builder.Build();

await app.ApplyMigrations<EventLogContext>();
await app.ApplyMigrations<AggregationContext>();

#region Middleware Pipeline

//Error handling
if (!app.Environment.IsProduction())
{
    app.Services.PrintHostInformation();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

//Security
app.UseHttpsRedirection();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

//Cross-cutting
app.UseResponseCompression();
app.UseHttpLogging();
app.UseMiddleware<UsageLoggingMiddleware>(
    (HttpContext ctx) => Assembly.GetEntryAssembly()?.GetName().Name,
    (HttpContext ctx, ControllerActionDescriptor? ad) => ad?.ActionName == "StreamData",
    (HttpContext ctx, ControllerActionDescriptor? ad) => ad?.ActionName == "GetData",
    (HttpContext ctx) => ctx.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ctx.User?.Identity?.Name);

//Swagger
app.UseConfiguredSwaggerUI();

//Endpoints
app.MapControllers();
app.MapJsonHealthChecks();

#endregion

app.Run();




//builder.Services.Configure<RateLimitingOptions>(
//builder.Configuration.GetSection("RateLimiting"));

//builder.Services.AddSingleton<RateLimitingPolicyService>();

//builder.Services.AddRateLimiter(options =>
//{
//    var sp = builder.Services.BuildServiceProvider();
//    var policyService = sp.GetRequiredService<RateLimitingPolicyService>();
//    options.GlobalLimiter = policyService.CreateGlobalLimiter();
//});

//builder.Services.AddRateLimiter(options =>
//{
//    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
//    {
//        // Partition key: combine IP + user
//        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
//        var user = httpContext.User.Identity?.Name ?? "anonymous";

//        // Example deny list
//        var blockedIps = new[] { "203.0.113.42", "198.51.100.77" };
//        var blockedUsers = new[] { "baduser@example.com", "spammer", "anonymous" };

//        // Block specific IPs outright
//        if (blockedIps.Contains(ip))
//        {
//            return RateLimitPartition.GetNoLimiter($"ip:{ip}");
//        }

//        // Block specific users outright
//        if (blockedUsers.Contains(user))
//        {
//            return RateLimitPartition.GetNoLimiter($"user:{user}");
//        }

//        // Otherwise apply a sliding window limiter per user+IP combo
//        var key = $"{user}:{ip}";
//        return RateLimitPartition.GetSlidingWindowLimiter(key, _ => new SlidingWindowRateLimiterOptions
//        {
//            PermitLimit = 10,                  // max 10 requests
//            Window = TimeSpan.FromMinutes(1),  // per 1 minute
//            SegmentsPerWindow = 2,             // smoother distribution
//            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
//            QueueLimit = 0
//        });
//    });
//});

//app.UseRateLimiter();


//"RateLimiting": {
//    "PermitLimit": 10,
//  "QueueLimit": 2,
//  "WindowSeconds": 30,
//  "Algorithm": "Sliding",
//  "BlockedIps": ["203.0.113.42", "198.51.100.77"],
//  "BlockedUsers": ["baduser@example.com", "spammer"]
//}

public class RateLimitingOptions
{
    public int PermitLimit { get; set; }
    public int QueueLimit { get; set; }
    public int WindowSeconds { get; set; }
    public string Algorithm { get; set; } = "Fixed"; // Fixed, Sliding, TokenBucket, Concurrency
    public List<string> BlockedIps { get; set; } = new();
    public List<string> BlockedUsers { get; set; } = new();
}


public class RateLimitingPolicyService
{
    private readonly RateLimitingOptions _options;
    private readonly ILogger<RateLimitingPolicyService> _logger;

    public RateLimitingPolicyService(IOptions<RateLimitingOptions> options,
                                     ILogger<RateLimitingPolicyService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public PartitionedRateLimiter<HttpContext> CreateGlobalLimiter()
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var user = httpContext.User.Identity?.Name ?? "anonymous";

            if (_options.BlockedIps.Contains(ip))
            {
                _logger.LogWarning("Blocked IP attempted access: {Ip}", ip);
                return RateLimitPartition.GetNoLimiter($"ip:{ip}");
            }

            if (_options.BlockedUsers.Contains(user))
            {
                _logger.LogWarning("Blocked user attempted access: {User}", user);
                return RateLimitPartition.GetNoLimiter($"user:{user}");
            }

            var key = $"{user}:{ip}";

            return _options.Algorithm switch
            {
                "Fixed" => RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = _options.PermitLimit,
                    Window = TimeSpan.FromSeconds(_options.WindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = _options.QueueLimit
                }),
                "Sliding" => RateLimitPartition.GetSlidingWindowLimiter(key, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = _options.PermitLimit,
                    Window = TimeSpan.FromSeconds(_options.WindowSeconds),
                    SegmentsPerWindow = 2,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = _options.QueueLimit
                }),
                "TokenBucket" => RateLimitPartition.GetTokenBucketLimiter(key, _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = _options.PermitLimit,
                    TokensPerPeriod = _options.PermitLimit / 2,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(_options.WindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = _options.QueueLimit
                }),
                "Concurrency" => RateLimitPartition.GetConcurrencyLimiter(key, _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = _options.PermitLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = _options.QueueLimit
                }),
                _ => RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = _options.PermitLimit,
                    Window = TimeSpan.FromSeconds(_options.WindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = _options.QueueLimit
                })
            };
        });
    }
}
