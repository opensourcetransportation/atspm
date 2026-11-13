// SPDX-License-Identifier: Apache-2.0

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.SqlDatabaseProvider;
using Xunit;

namespace DatabaseInstallerTests;

public class SqlServerMigrationIntegrationTests
{
    [Fact]
    public async Task Fresh_databases_and_config_upgrade_migrate_safely_when_configured()
    {
        var adminConnection = Environment.GetEnvironmentVariable("ATSPM_MIGRATION_TEST_SQLSERVER");
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
                var databaseConnection = new SqlConnectionStringBuilder(adminConnection)
                {
                    InitialCatalog = databaseName
                }.ConnectionString;

                await using var context = CreateContext(contextName, databaseConnection);
                if (contextName == "Config")
                {
                    await context.GetService<IMigrator>().MigrateAsync("20260401160850_5_2_1");
                    await using var connection = new SqlConnection(databaseConnection);
                    await connection.OpenAsync();
                    await using var insert = new SqlCommand(
                        "INSERT INTO [UsageEntries] ([Timestamp], [StatusCode], [DurationMs], [Success]) VALUES ('2026-01-01T00:00:00', 200, 42, 1)",
                        connection);
                    await insert.ExecuteNonQueryAsync();
                }

                await context.Database.MigrateAsync();
                Assert.Empty(await context.Database.GetPendingMigrationsAsync());
                var applied = (await context.Database.GetAppliedMigrationsAsync()).ToArray();
                Assert.NotEmpty(applied);

                await context.Database.MigrateAsync();
                Assert.Equal(applied, await context.Database.GetAppliedMigrationsAsync());

                if (contextName == "Config")
                {
                    await using var connection = new SqlConnection(databaseConnection);
                    await connection.OpenAsync();
                    await using var check = new SqlCommand(
                        "SELECT TYPE_NAME(c.system_type_id) FROM sys.columns AS c WHERE c.object_id = OBJECT_ID('UsageEntries') AND c.name = 'Timestamp'",
                        connection);
                    Assert.Equal("datetimeoffset", await check.ExecuteScalarAsync());
                    check.CommandText = "SELECT COUNT(*) FROM [UsageEntries] WHERE [StatusCode] = 200 AND [DurationMs] = 42";
                    Assert.Equal(1, await check.ExecuteScalarAsync());
                }
                else if (contextName == "Aggregation")
                {
                    await using var connection = new SqlConnection(databaseConnection);
                    await connection.OpenAsync();
                    await using var check = new SqlCommand(
                        "INSERT INTO [SignalTimingPlans] ([Start], [End], [LocationIdentifier], [PlanNumber]) VALUES ('2026-01-01', '2026-01-02', 'TEST', 1), ('2026-01-03', '2026-01-02', 'TEST', 2); SELECT [Valid] FROM [SignalTimingPlans] WHERE [LocationIdentifier] = 'TEST' ORDER BY [PlanNumber]",
                        connection);
                    await using var reader = await check.ExecuteReaderAsync();
                    Assert.True(await reader.ReadAsync());
                    Assert.True(reader.GetBoolean(0));
                    Assert.True(await reader.ReadAsync());
                    Assert.False(reader.GetBoolean(0));
                    Assert.False(await reader.ReadAsync());
                }
            }
        }
        finally
        {
            SqlConnection.ClearAllPools();
            await using var admin = new SqlConnection(adminConnection);
            await admin.OpenAsync();
            foreach (var databaseName in databaseNames)
            {
                await using var command = new SqlCommand(
                    $"IF DB_ID('{databaseName}') IS NOT NULL BEGIN ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}]; END",
                    admin);
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
            .UseSqlServer(connectionString, options => options.MigrationsAssembly(SqlServerProvider.Migration))
            .Options;
    }
}
