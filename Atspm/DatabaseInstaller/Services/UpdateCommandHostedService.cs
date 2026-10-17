#region license
// Copyright 2026 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Services/UpdateCommandHostedService.cs
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

using global::DatabaseInstaller.Commands;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Infrastructure.Extensions;


namespace DatabaseInstaller.Services
{
    public class UpdateCommandHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UpdateCommandConfiguration _config;
        private readonly ILogger<UpdateCommandHostedService> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public UpdateCommandHostedService(
            IServiceProvider serviceProvider,
            IOptions<UpdateCommandConfiguration> config,
            ILogger<UpdateCommandHostedService> logger,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _serviceProvider = serviceProvider;
            _config = config.Value;
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Apply migrations
                await ApplyMigrationsForAllContexts(cancellationToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    await scope.ServiceProvider.SeedIdentityData();
                }

                // Optionally seed admin
                if (_config.SeedAdmin)
                {
                    await SeedAdminUserAndAssignRole();
                }

                _logger.LogInformation("Database migration and admin seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database migration or seeding failed.");
                throw;
            }
            finally
            {
                _logger.LogInformation("Shutting down the application after completion.");
                _hostApplicationLifetime.StopApplication(); // Stop the host after all tasks are complete
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


        // Helper method to ensure the PostgreSQL database exists.
        //private async Task EnsureDatabaseExists(string connectionString)
        //{
        //    var builder = new NpgsqlConnectionStringBuilder(connectionString);
        //    var databaseName = builder.Database;
        //    // Connect to the default database instead.
        //    builder.Database = "postgres";

        //    using var connection = new NpgsqlConnection(builder.ConnectionString);
        //    await connection.OpenAsync();

        //    using (var command = connection.CreateCommand())
        //    {
        //        // Check if the database exists.
        //        command.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
        //        var exists = await command.ExecuteScalarAsync();

        //        if (exists == null)
        //        {
        //            // Create the database if it does not exist.
        //            command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
        //            await command.ExecuteNonQueryAsync();
        //            _logger.LogInformation("Created database {DatabaseName}", databaseName);
        //        }
        //        else
        //        {
        //            _logger.LogInformation("Database {DatabaseName} already exists", databaseName);
        //        }
        //    }
        //}

        //    private async Task MigrateContextAsync<TContext>(
        //string connectionString,
        //CancellationToken cancellationToken,
        //string contextName)
        //where TContext : DbContext
        //    {
        //        // Build new options for the context using the correct connection string.
        //        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        //        optionsBuilder.UseNpgsql(connectionString);

        //        // Create a new instance of the context with these options.
        //        using var context = (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options);

        //        // Optionally, ensure the database exists.
        //        //await EnsureDatabaseExists(connectionString);

        //        // Check for pending migrations.
        //        //var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        //        //if (pendingMigrations.Any())
        //        //{
        //            //_logger.LogInformation("{ContextName} has pending migrations: {Pending}",
        //            //    contextName, string.Join(", ", pendingMigrations));
        //            await context.Database.MigrateAsync(cancellationToken);
        //            _logger.LogInformation("Migrations applied for {ContextName}.", contextName);
        //        //}
        //        //else
        //        //{
        //        //    _logger.LogInformation("No pending migrations for {ContextName}.", contextName);
        //        //}
        //    }

        private async Task ApplyMigrationsForAllContexts(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            // ConfigContext
            var configContext = serviceProvider.GetRequiredService<ConfigContext>();
            if (!string.IsNullOrEmpty(_config.ConfigConnection))
            {
                _logger.LogInformation("Overriding ConfigContext connection string.");
                configContext.Database.SetConnectionString(_config.ConfigConnection);
            }
            _logger.LogInformation("Applying migrations for ConfigContext.");
            await configContext.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Migrations applied for ConfigContext.");

            // AggregationContext
            var aggregationContext = serviceProvider.GetRequiredService<AggregationContext>();
            if (!string.IsNullOrEmpty(_config.AggregationConnection))
            {
                _logger.LogInformation("Overriding AggregationContext connection string.");
                aggregationContext.Database.SetConnectionString(_config.AggregationConnection);
            }
            _logger.LogInformation("Applying migrations for AggregationContext.");
            await aggregationContext.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Migrations applied for AggregationContext.");

            // EventLogContext
            var eventLogContext = serviceProvider.GetRequiredService<EventLogContext>();
            if (!string.IsNullOrEmpty(_config.EventLogConnection))
            {
                _logger.LogInformation("Overriding EventLogContext connection string.");
                eventLogContext.Database.SetConnectionString(_config.EventLogConnection);
            }
            _logger.LogInformation("Applying migrations for EventLogContext.");
            await eventLogContext.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Migrations applied for EventLogContext.");

            // IdentityContext
            var identityContext = serviceProvider.GetRequiredService<IdentityContext>();
            if (!string.IsNullOrEmpty(_config.IdentityConnection))
            {
                _logger.LogInformation("Overriding IdentityContext connection string.");
                identityContext.Database.SetConnectionString(_config.IdentityConnection);
            }
            _logger.LogInformation("Applying migrations for IdentityContext.");
            await identityContext.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Migrations applied for IdentityContext.");

            // Seed roles and claims
            //await RolesAndClaimsDBInitializer.SeedRolesAndClaims(serviceProvider, _config.IdentityConnection);
        }



        //private async Task ApplyMigrationsForAllContexts(CancellationToken cancellationToken)
        //{
        //    await MigrateContextAsync<ConfigContext>(_config.ConfigConnection, cancellationToken, "ConfigContext");
        //    await MigrateContextAsync<AggregationContext>(_config.AggregationConnection, cancellationToken, "AggregationContext");
        //    await MigrateContextAsync<EventLogContext>(_config.EventLogConnection, cancellationToken, "EventLogContext");
        //    await MigrateContextAsync<IdentityContext>(_config.IdentityConnection, cancellationToken, "IdentityContext");

        //    // Seed roles and claims as needed.
        //    using var scope = _serviceProvider.CreateScope();
        //    var serviceProvider = scope.ServiceProvider;
        //    await RolesAndClaimsDBInitializer.SeedRolesAndClaims(serviceProvider, _config.IdentityConnection);
        //}



        //private async Task SeedAdminUserAndAssignRole()
        //{
        //    using var scope = _serviceProvider.CreateScope();
        //    var identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
        //    // Set the connection string dynamically
        //    identityContext.Database.GetDbConnection().ConnectionString = _config.IdentityConnection;
        //    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        //    // Check if the admin user already exists
        //    var adminUser = await userManager.FindByEmailAsync(_config.AdminEmail);
        //    if (adminUser == null)
        //    {
        //        // Create the admin user
        //        adminUser = new ApplicationUser
        //        {
        //            UserName = _config.AdminEmail,
        //            Email = _config.AdminEmail,
        //            EmailConfirmed = true,
        //            FirstName = "Admin",
        //            LastName = "Admin",
        //            Agency = "Transportation Agency",
        //        };
        //        var result = await userManager.CreateAsync(adminUser, _config.AdminPassword);
        //        if (result.Succeeded)
        //        {
        //            _logger.LogInformation("Admin user created.");
        //        }
        //        else
        //        {
        //            _logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        //            return;
        //        }
        //    }

        //    // Assign the Admin role to the admin user (role must already exist)
        //    if (!await userManager.IsInRoleAsync(adminUser, _config.AdminRole))
        //    {
        //        await userManager.AddToRoleAsync(adminUser, _config.AdminRole);
        //        _logger.LogInformation("Admin user assigned to Admin role.");
        //    }
        //}

        private async Task SeedAdminUserAndAssignRole()
        {
            using var scope = _serviceProvider.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            // Manually create IdentityContext with the correct connection string
            var dbContextOptions = serviceProvider.GetRequiredService<DbContextOptions<IdentityContext>>();
            var identityContext = new IdentityContext(dbContextOptions);

            if (!string.IsNullOrEmpty(_config.IdentityConnection))
            {
                _logger.LogInformation("Overriding IdentityContext connection string.");
                identityContext.Database.SetConnectionString(_config.IdentityConnection);
            }

            // Ensure the database is migrated before proceeding
            await identityContext.Database.MigrateAsync();

            // Manually create the dependencies required for UserManager<ApplicationUser>
            var userStore = new UserStore<ApplicationUser>(identityContext);
            var passwordHasher = new PasswordHasher<ApplicationUser>();

            // Explicitly configure IdentityOptions to allow the provided password
            var identityOptions = new IdentityOptions();
            identityOptions.Password.RequireDigit = true;
            identityOptions.Password.RequiredLength = 6;
            identityOptions.Password.RequireLowercase = true;
            identityOptions.Password.RequireUppercase = true;
            identityOptions.Password.RequireNonAlphanumeric = true;
            identityOptions.Password.RequiredUniqueChars = 1;

            var options = Options.Create(identityOptions);

            var passwordValidators = new List<IPasswordValidator<ApplicationUser>> { new PasswordValidator<ApplicationUser>() };
            var userValidators = new List<IUserValidator<ApplicationUser>> { new UserValidator<ApplicationUser>() };
            var keyNormalizer = serviceProvider.GetRequiredService<ILookupNormalizer>();
            var errors = serviceProvider.GetRequiredService<IdentityErrorDescriber>();
            var logger = serviceProvider.GetRequiredService<ILogger<UserManager<ApplicationUser>>>();

            // Manually create UserManager with configured password options
            var userManager = new UserManager<ApplicationUser>(
                userStore, options, passwordHasher, userValidators, passwordValidators,
                keyNormalizer, errors, serviceProvider, logger);

            // Check if the admin user already exists.
            var adminUser = await userManager.FindByEmailAsync(_config.AdminEmail);
            if (adminUser == null)
            {
                // Create the admin user.
                adminUser = new ApplicationUser
                {
                    UserName = _config.AdminEmail,
                    Email = _config.AdminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "Admin",
                    Agency = "Transportation Agency",
                };

                var createResult = await userManager.CreateAsync(adminUser, _config.AdminPassword);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException("Failed to create admin user: " +
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }

                _logger.LogInformation("Admin user created successfully.");
            }
            else
            {
                _logger.LogInformation("Admin user already exists.");
            }

            // Assign the Admin role to the user if not already assigned.
            if (!await userManager.IsInRoleAsync(adminUser, _config.AdminRole))
            {
                var roleResult = await userManager.AddToRoleAsync(adminUser, _config.AdminRole);
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException("Failed to assign admin role: " +
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
                _logger.LogInformation("Admin user assigned to Admin role successfully.");
            }
            else
            {
                _logger.LogInformation("Admin user is already assigned to the Admin role.");
            }
        }






    }


}
