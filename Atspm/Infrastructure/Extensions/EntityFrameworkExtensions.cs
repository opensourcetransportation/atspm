#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Extensions/EntityFrameworkExtensions.cs
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

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models.IdentityModels;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    /// <summary>
    /// Contains configuration settings for database connectivity and provides logic to generate provider-specific connection strings.
    /// </summary>
    public static class MigrationExtensions
    {
        /// <summary>
        /// Applies pending migrations for the specified <typeparamref name="TContext"/>.
        /// Ensures the physical database is created before migrations are applied.
        /// </summary>
        /// <typeparam name="TContext">The type of <see cref="DbContext"/> to migrate.</typeparam>
        /// <param name="host">The application host providing access to services.</param>
        /// <param name="seedAction">An optional asynchronous action to perform database seeding.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ApplyMigrations<TContext>(this IHost host, Func<IServiceProvider, Task>? seedAction = null) where TContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var options = services.GetRequiredService<IOptionsSnapshot<DatabaseConfiguration>>();
            var settings = options.Get(typeof(TContext).Name);
            var contextName = typeof(TContext).Name;

            var logMessage = new MigrationLogMessages(services.GetRequiredService<ILogger<TContext>>(), contextName);

            logMessage.RunMigrationsFlag(contextName, settings.RunMigrations);

            if (!settings.RunMigrations)
            {
                return;
            }

            try
            {
                var context = services.GetRequiredService<TContext>();
                var databaseCreator = context.GetService<IRelationalDatabaseCreator>();
                var databaseExists = await databaseCreator.ExistsAsync();

                logMessage.DatabaseExists(contextName, databaseExists);

                if (!databaseExists)
                {
                    logMessage.CreateDatabase(contextName);

                    await databaseCreator.CreateAsync();
                }

                logMessage.ApplyngMigrations(contextName);

                await context.Database.MigrateAsync();

                if (seedAction != null)
                {
                    logMessage.ApplySeeding(contextName);

                    await seedAction(services);
                }
            }
            catch (Exception e)
            {
                logMessage.ApplyMigrationsException(contextName, e);
                throw;
            }
        }

        /// <summary>
        /// Seeds a default administrative user and role based on environment variables.
        /// </summary>
        /// <param name="services">The service provider used to resolve Identity services.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SeedAdminUser(this IServiceProvider services)
        {
            var logMessages = new UserSeedLogMessages(services.GetRequiredService<ILogger<IdentityContext>>());
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            var email = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                logMessages.MissingCredentials();
                return;
            }

            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                logMessages.AdminNotFound(email);

                var user = new ApplicationUser { UserName = email, Email = email };
                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    var adminRole = AtspmAuthorization.Roles.Admin;

                    if (!await roleManager.RoleExistsAsync(adminRole))
                    {
                        logMessages.CreatingRole(adminRole);
                        var roleResult = await roleManager.CreateAsync(new IdentityRole(adminRole));
                        if (!roleResult.Succeeded)
                        {
                            throw new InvalidOperationException("Failed to create admin role: " +
                                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        }
                    }

                    var addToRoleResult = await userManager.AddToRoleAsync(user, adminRole);

                    if (addToRoleResult.Succeeded)
                    {
                        logMessages.SeedingSuccess(email, adminRole);
                    }
                    else
                    {
                        var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                        logMessages.RoleAssignmentError(adminRole, errors);
                        throw new InvalidOperationException("Failed to assign admin role: " + errors);
                    }
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logMessages.UserCreationError(errors);
                    throw new InvalidOperationException("Failed to create admin user: " + errors);
                }
            }
            else
            {
                logMessages.UserAlreadyExists(email);
            }
        }

        /// <summary>
        /// Seeds the roles and granular claims required for ATSPM authorization.
        /// </summary>
        public static async Task SeedIdentityData(this IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var logMessages = new IdentitySeedLogMessages(services.GetRequiredService<ILogger<IdentityContext>>());

            //if (await roleManager.RoleExistsAsync(AtspmAuthorization.Roles.Admin))
            //{
            //    logMessages.RolesAlreadyExist();
            //    return;
            //}

            foreach (var entry in AtspmAuthorization.RoleClaimsMap)
            {
                var role = await roleManager.FindByNameAsync(entry.Key);

                if (role == null)
                {
                    role = new IdentityRole(entry.Key);
                    var result = await roleManager.CreateAsync(role);

                    if (result.Succeeded)
                    {
                        logMessages.RoleCreated(entry.Key);
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        logMessages.RoleCreationError(entry.Key, errors);
                        throw new InvalidOperationException("Failed to create role " + entry.Key + ": " + errors);
                    }
                }

                var existingClaims = await roleManager.GetClaimsAsync(role);
                foreach (var permission in entry.Value)
                {
                    if (!existingClaims.Any(c => c.Value == permission))
                    {
                        var claimResult = await roleManager.AddClaimAsync(role, new Claim(AtspmAuthorization.RoleClaimType, permission));
                        if (!claimResult.Succeeded)
                        {
                            throw new InvalidOperationException("Failed to add permission to role " + entry.Key + ": " +
                                string.Join(", ", claimResult.Errors.Select(e => e.Description)));
                        }
                        logMessages.PermissionAdded(permission, entry.Key);
                    }
                }
            }
        }
    }
}
