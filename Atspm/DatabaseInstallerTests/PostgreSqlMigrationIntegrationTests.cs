// SPDX-License-Identifier: Apache-2.0

using Microsoft.EntityFrameworkCore;
using Npgsql;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.PostgreSQLDatabaseProvider;
using Xunit;

namespace DatabaseInstallerTests;

public class PostgreSqlMigrationIntegrationTests
{
    [Fact]
    public async Task Fresh_databases_migrate_all_contexts_and_repeat_safely_when_configured()
    {
        var adminConnection = Environment.GetEnvironmentVariable("ATSPM_MIGRATION_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(adminConnection))
        {
            return; // Explicit opt-in: this test creates and drops temporary databases.
        }

        var databaseNames = new List<string>();
        try
        {
            foreach (var contextName in new[] { "Config", "Aggregation", "EventLog", "Identity" })
            {
                var databaseName = $"atspm_migration_{contextName.ToLowerInvariant()}_{Guid.NewGuid():N}";
                databaseNames.Add(databaseName);
                var databaseConnection = new NpgsqlConnectionStringBuilder(adminConnection)
                {
                    Database = databaseName
                }.ConnectionString;

                await using var context = CreateContext(contextName, databaseConnection);
                await context.Database.MigrateAsync();
                Assert.Empty(await context.Database.GetPendingMigrationsAsync());
                var applied = (await context.Database.GetAppliedMigrationsAsync()).ToArray();
                Assert.NotEmpty(applied);

                await context.Database.MigrateAsync();
                Assert.Equal(applied, await context.Database.GetAppliedMigrationsAsync());
            }
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await using var admin = new NpgsqlConnection(adminConnection);
            await admin.OpenAsync();
            foreach (var databaseName in databaseNames)
            {
                await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)", admin);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    private static DbContext CreateContext(string name, string connectionString)
    {
        return name switch
        {
            "Config" => new ConfigContext(Options<ConfigContext>()),
            "Aggregation" => new AggregationContext(Options<AggregationContext>()),
            "EventLog" => new EventLogContext(Options<EventLogContext>()),
            "Identity" => new IdentityContext(Options<IdentityContext>()),
            _ => throw new ArgumentOutOfRangeException(nameof(name))
        };

        DbContextOptions<T> Options<T>() where T : DbContext => new DbContextOptionsBuilder<T>()
            .UseNpgsql(connectionString, options => options.MigrationsAssembly(PostgreSqlProvider.Migration))
            .Options;
    }
}
