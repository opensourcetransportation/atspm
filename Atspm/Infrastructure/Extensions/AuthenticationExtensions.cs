#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Extensions/AuthenticationExtensions.cs
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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models.IdentityModels;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    //public class PluginRegistrationService
    //{
    //    private readonly AtspmDbContext _context;

    //    public PluginRegistrationService(AtspmDbContext context)
    //    {
    //        _context = context;
    //    }

    //    public async Task RegisterPluginAsync(IAtspmPlugin plugin)
    //    {
    //        var pluginName = plugin.GetType().Name;

    //        // 1. Generate a new API Key for this plugin instance
    //        string rawKey = ApiKeyGenerator.GenerateKey(); // Your method to make a random string
    //        string hashedKey = ApiKeyGenerator.HashKey(rawKey);

    //        var apiKey = new ApiKey
    //        {
    //            Id = Guid.NewGuid().ToString(),
    //            OwnerId = $"Plugin:{pluginName}",
    //            KeyHash = hashedKey,
    //            CreatedAt = DateTime.UtcNow,
    //            IsRevoked = false,
    //            Claims = new List<ApiKeyClaim>()
    //        };

    //        // 2. THE ASSOCIATION LOGIC:
    //        // Look at every interface we have mapped in AtspmSecurity
    //        foreach (var mapping in AtspmSecurity.PluginInterfaceMap)
    //        {
    //            var requiredInterface = mapping.Key;
    //            var grantedPermissions = mapping.Value;

    //            // Does this specific plugin implement the mapped interface?
    //            if (requiredInterface.IsAssignableFrom(plugin.GetType()))
    //            {
    //                // Give the plugin the capability claim (The "Feature" sticker)
    //                apiKey.Claims.Add(new ApiKeyClaim
    //                {
    //                    Type = AtspmSecurity.PluginFeatureClaimType,
    //                    Value = requiredInterface.Name // e.g., "IHasMigration"
    //                });

    //                // Give the plugin the standard permission claims (The "Role" stickers)
    //                foreach (var permission in grantedPermissions)
    //                {
    //                    // THIS IS THE CRITICAL LINE:
    //                    // It associates the plugin with the string your attribute checks!
    //                    apiKey.Claims.Add(new ApiKeyClaim
    //                    {
    //                        Type = AtspmSecurity.RoleClaimType,
    //                        Value = permission // e.g., "Data:Edit"
    //                    });
    //                }
    //            }
    //        }

    //        // 3. Save to Database
    //        _context.ApiKeys.Add(apiKey);
    //        await _context.SaveChangesAsync();

    //        // 4. Hand the raw key to the plugin so it can use it in headers
    //        plugin.Initialize(rawKey);
    //    }
    //}


    /// <summary>
    /// Custom authentication handler that validates requests using an API key provided in the "X-API-KEY" header.
    /// </summary>
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /// <summary>
        /// The database context used to access and validate API keys.
        /// </summary>
        private readonly IdentityContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyAuthenticationHandler"/> class.
        /// </summary>
        /// <param name="options">The monitor for the authentication scheme options.</param>
        /// <param name="logger">The logger factory for capturing diagnostic information.</param>
        /// <param name="encoder">The URL encoder for handling authentication-related strings.</param>
        /// <param name="context">The <see cref="IdentityContext"/> used for database operations.</param>
        public ApiKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IdentityContext context) : base(options, logger, encoder)
        {
            _context = context;
        }

        /// <summary>
        /// Performs the core authentication logic by extracting the API key from the request header and verifying it against the database.
        /// </summary>
        /// <returns>
        /// An <see cref="AuthenticateResult"/> indicating success if the key is valid, active, and not expired; 
        /// otherwise, an appropriate failure result.
        /// </returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-API-KEY", out var extractedKey))
            {
                return AuthenticateResult.NoResult();
            }

            string providedKey = extractedKey.ToString();
            string hashedKey = ApiKeyGenerator.HashKey(providedKey);

            var apiKey = await _context.Set<ApiKey>()
                .Include(k => k.Claims)
                .FirstOrDefaultAsync(k => k.KeyHash == hashedKey
                                       && !k.IsRevoked
                                       && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow));

            if (apiKey == null)
            {
                return AuthenticateResult.Fail("Invalid, revoked, or expired API Key.");
            }
            var claimsList = new List<Claim>();

            if (apiKey.Claims != null)
            {
                foreach (var c in apiKey.Claims)
                {
                    claimsList.Add(new Claim(c.Type ?? ClaimTypes.Role, c.Value ?? ""));
                }
            }

            if (!string.IsNullOrEmpty(apiKey.OwnerId))
            {
                claimsList.Add(new Claim(ClaimTypes.NameIdentifier, apiKey.OwnerId));
                claimsList.Add(new Claim(ClaimTypes.GivenName, apiKey.OwnerId));
            }

            if (!string.IsNullOrEmpty(apiKey.Name))
            {
                claimsList.Add(new Claim(ClaimTypes.Surname, apiKey.Name));
            }

            claimsList.Add(new Claim("AuthenticationMethod", "ApiKey"));

            var identity = new ClaimsIdentity(claimsList, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }

    /// <summary>
    /// Helper extensions for <see cref="Microsoft.Extensions.Hosting"/> using <see cref="Microsoft.AspNetCore.Authentication"/> 
    /// </summary>
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Adds Atspm identity services if <see cref="Host"/> is not in <see cref="Environments.Development"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IServiceCollection AddAtspmIdentity(this IServiceCollection services, HostBuilderContext host)
        {
            //if (!host.HostingEnvironment.IsDevelopment())
            //{
            services.AddAtspmAuthentication(host);
            services.AddAtspmAuthorization();
            //}

            return services;
        }

        /// <summary>
        /// Configures Swagger to include security definitions and requirements for both 
        /// JWT Bearer tokens and X-API-KEY headers.
        /// </summary>
        /// <param name="swaggerGenOptions">The Swagger generation options to configure.</param>
        /// <returns>The configured <see cref="SwaggerGenOptions"/> instance for method chaining.</returns>
        /// <remarks>
        /// This method registers two security schemes:
        /// <list type="bullet">
        /// <item>
        /// <description><b>JWT Bearer:</b> Uses the standard Authorization header with the Bearer scheme.</description>
        /// </item>
        /// <item>
        /// <description><b>API Key:</b> Uses a custom <c>X-API-KEY</c> header.</description>
        /// </item>
        /// </list>
        /// By default, this adds alternative global security requirements so a JWT or an
        /// API key can authorize an operation. Operations with different access rules must
        /// override the global requirement.
        /// </remarks>
        public static SwaggerGenOptions AddAtspmSecurityDefinitions(this SwaggerGenOptions swaggerGenOptions)
        {
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            var apiKeySecurityScheme = new OpenApiSecurityScheme
            {
                Name = "X-API-KEY",
                Description = "Enter your API Key directly (no 'Bearer' prefix needed)",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKey",
                Reference = new OpenApiReference
                {
                    Id = "ApiKey",
                    Type = ReferenceType.SecurityScheme
                }
            };

            swaggerGenOptions.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
            swaggerGenOptions.AddSecurityDefinition(apiKeySecurityScheme.Reference.Id, apiKeySecurityScheme);

            swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtSecurityScheme, Array.Empty<string>() }
            });
            swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { apiKeySecurityScheme, Array.Empty<string>() }
            });

            return swaggerGenOptions;
        }

        /// <summary>
        /// Add atspm authentication
        /// </summary>
        /// <param name="services"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IServiceCollection AddAtspmAuthentication(this IServiceCollection services, HostBuilderContext host)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = host.Configuration["Jwt:Issuer"],
                    ValidAudience = host.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(host.Configuration["Jwt:Key"]))
                };
            })

            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

            var oidc = host.Configuration.GetSection("Oidc");
            if (oidc.Exists() && !string.IsNullOrEmpty(oidc["Authority"]) &&
                !string.IsNullOrEmpty(oidc["ClientId"]) &&
                !string.IsNullOrEmpty(oidc["ClientSecret"]) &&
                !string.IsNullOrEmpty(oidc["CallbackPath"]))
            {
                services.AddAuthentication()
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = oidc["Authority"];
                options.ClientId = oidc["ClientId"];
                options.ClientSecret = oidc["ClientSecret"];
                options.ResponseType = OpenIdConnectResponseType.IdToken;
                options.SaveTokens = true;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");
                options.Scope.Add("app:Atspm");

                options.CallbackPath = oidc["CallbackPath"];

                options.GetClaimsFromUserInfoEndpoint = true;
                options.UseTokenLifetime = true;
                options.SkipUnrecognizedRequests = true;

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        var b = new UriBuilder(context.ProtocolMessage.RedirectUri);
                        b.Scheme = "https";
                        b.Port = -1;
                        context.ProtocolMessage.RedirectUri = b.ToString();

                        Console.WriteLine($"callback: {b.ToString()}");

                        return Task.CompletedTask;
                    },
                };
            });
            }

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            return services;
        }

        /// <summary>
        /// Configures the ATSPM authorization system by dynamically generating policies based on defined permissions.
        /// </summary>
        /// <remarks>
        /// This method performs the following actions:
        /// <list type="bullet">
        /// <item>Uses reflection to iterate through all constant fields in <see cref="AtspmAuthorization.Permissions"/>.</item>
        /// <item>Generates policy names using the pattern "Can{Action}{Category}" (e.g., "Users:View" becomes "CanViewUsers").</item>
        /// <item>Includes a global bypass for the <see cref="AtspmAuthorization.Roles.Admin"/> role in every functional policy.</item>
        /// <item>Registers an "AdminOnly" policy for high-security system tasks.</item>
        /// <item>Supports both JWT Bearer and ApiKey authentication schemes.</item>
        /// </list>
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddAtspmAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                var schemes = new[] { JwtBearerDefaults.AuthenticationScheme, "ApiKey" };

                var permissions = typeof(AtspmAuthorization.Permissions)
                    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(f => f.IsLiteral && !f.IsInitOnly)
                    .Select(f => f.GetRawConstantValue()?.ToString())
                    .Where(v => v != null)
                    .ToList();

                foreach (var permission in permissions!)
                {
                    if (permission == AtspmAuthorization.Permissions.Admin) continue;

                    var policyName = AtspmAuthorization.GetPolicyName(permission);

                    options.AddPolicy(policyName, policy =>
                    {
                        policy.AddAuthenticationSchemes(schemes);

                        policy.RequireAssertion(context =>
                        {
                            var hasPermission = context.User.HasClaim(c => c.Type == AtspmAuthorization.RoleClaimType && c.Value == permission);
                            var isAdmin = context.User.IsInRole(AtspmAuthorization.Roles.Admin);

                            return hasPermission || isAdmin;
                        });
                    });
                }

                options.AddPolicy(AtspmAuthorization.Roles.Admin, policy =>
                {
                    policy.AddAuthenticationSchemes(schemes);
                    policy.RequireClaim(AtspmAuthorization.RoleClaimType, AtspmAuthorization.Roles.Admin);
                });
            });

            return services;
        }
    }
}
