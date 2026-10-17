// SPDX-License-Identifier: Apache-2.0

using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;

namespace Utah.Udot.Atspm.Infrastructure.Extensions;

/// <summary>
/// Configures HTTP diagnostics without recording credentials, tokens, or response data.
/// </summary>
public static class HttpLoggingExtensions
{
    public static IServiceCollection AddAtspmHttpLogging(this IServiceCollection services)
    {
        return services.AddHttpLogging(options =>
        {
            // Paths can contain identifiers, and queries, headers, and bodies can contain secrets.
            options.LoggingFields = HttpLoggingFields.RequestMethod
                | HttpLoggingFields.ResponseStatusCode
                | HttpLoggingFields.Duration;
            options.CombineLogs = true;
        });
    }
}
