// SPDX-License-Identifier: Apache-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.PostgreSQLDatabaseProvider;
using Utah.Udot.Atspm.SqlDatabaseProvider;
using Xunit;

namespace DatabaseInstallerTests;

public class ProviderMigrationScriptTests
{
    [Theory]
    [InlineData("Config", "SqlServer", 6)]
    [InlineData("Aggregation", "SqlServer", 4)]
    [InlineData("EventLog", "SqlServer", 3)]
    [InlineData("Identity", "SqlServer", 2)]
    [InlineData("Config", "PostgreSql", 6)]
    [InlineData("Aggregation", "PostgreSql", 4)]
    [InlineData("EventLog", "PostgreSql", 3)]
    [InlineData("Identity", "PostgreSql", 2)]
    public void Provider_migrations_are_discoverable_and_generate_a_script(string contextName, string provider, int minimumMigrations)
    {
        using var context = CreateContext(contextName, provider);

        var migrations = context.Database.GetMigrations().ToArray();
        Assert.True(migrations.Length >= minimumMigrations,
            $"{provider} {contextName} should expose at least {minimumMigrations} migrations; found {migrations.Length}.");

        var script = context.GetService<IMigrator>()
            .GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);
        Assert.Contains(migrations[0], script);
        Assert.Contains(migrations[^1], script);

        if (provider == "SqlServer")
        {
            Assert.DoesNotContain("timestamp with time zone", script, StringComparison.OrdinalIgnoreCase);

            if (contextName == "Config")
            {
                var rollbackScript = context.GetService<IMigrator>().GenerateScript(
                    fromMigration: "20260521170321_5_3",
                    toMigration: "20260401160850_5_2_1");
                Assert.DoesNotContain("timestamp with time zone", rollbackScript, StringComparison.OrdinalIgnoreCase);
            }
            else if (contextName == "Aggregation")
            {
                Assert.Contains("CONVERT(bit, CASE WHEN [End] > [Start] THEN 1 ELSE 0 END)", script);
            }
        }
    }

    private static DbContext CreateContext(string name, string provider)
    {
        const string connectionString = "Server=localhost;Database=ATSPM_Migration_Test;Integrated Security=True;TrustServerCertificate=True";
        const string postgresConnection = "Host=localhost;Database=atspm_migration_test;Username=atspm";

        return name switch
        {
            "Config" => new ConfigContext(Options<ConfigContext>()),
            "Aggregation" => new AggregationContext(Options<AggregationContext>()),
            "EventLog" => new EventLogContext(Options<EventLogContext>()),
            "Identity" => new IdentityContext(Options<IdentityContext>()),
            _ => throw new ArgumentOutOfRangeException(nameof(name))
        };

        DbContextOptions<T> Options<T>() where T : DbContext
        {
            var builder = new DbContextOptionsBuilder<T>();
            if (provider == "SqlServer")
            {
                builder.UseSqlServer(connectionString, options => options.MigrationsAssembly(SqlServerProvider.Migration));
            }
            else if (provider == "PostgreSql")
            {
                builder.UseNpgsql(postgresConnection, options => options.MigrationsAssembly(PostgreSqlProvider.Migration));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(provider));
            }

            return builder.Options;
        }
    }
}
