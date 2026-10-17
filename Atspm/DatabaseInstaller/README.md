# DatabaseInstaller

`DatabaseInstaller` is a command-line utility for applying database migrations, moving event log data, copying configuration data, and seeding test records.

## Verifying database migrations

Run the provider script checks without a database:

```bash
dotnet test ../DatabaseInstallerTests/DatabaseInstallerTests.csproj --filter FullyQualifiedName~ProviderMigrationScriptTests
```

These checks discover the migrations for all four contexts on PostgreSQL and SQL Server, generate forward scripts, and check SQL Server-specific type and computed-column expressions. They do not execute SQL against a server.

To test PostgreSQL migrations on a disposable database server, set `ATSPM_MIGRATION_TEST_POSTGRES` to an administrator connection string for that server, then run:

```bash
dotnet test ../DatabaseInstallerTests/DatabaseInstallerTests.csproj --filter FullyQualifiedName~PostgreSqlMigrationIntegrationTests
```

The integration test creates four uniquely named databases, migrates each context twice, checks that no migrations remain pending, and drops those databases. The connection must allow database creation and deletion. Without the environment variable, this opt-in test does no database work. Use an isolated test server; do not point it at production.

For SQL Server, set `ATSPM_MIGRATION_TEST_SQLSERVER` to an administrator connection string for an isolated SQL Server instance and run:

```bash
dotnet test ../DatabaseInstallerTests/DatabaseInstallerTests.csproj --filter FullyQualifiedName~SqlServerMigrationIntegrationTests
```

This test also creates and drops four uniquely named databases. It migrates Config first to `5_2_1`, inserts a synthetic usage row, then upgrades to the current schema and checks that the row survives with a `datetimeoffset` timestamp. It verifies the computed timing-plan validity column for both true and false cases. The test does no database work when the environment variable is unset.

The SQL Server Config migration `20260318134420_5_1_4` now declares `UsageEntries.Timestamp` as `datetime2` until the later `20260521170321_5_3` migration changes it to `datetimeoffset`. The SQL Server Aggregation migration uses a `bit`-valued `CASE` expression for `SignalTimingPlans.Valid`. These repairs do not add a migration or delete history records. An existing database that has already recorded those migrations is not replayed by EF Core. If a database was manually repaired and its intermediate column type differs, inspect its actual schema and back it up before applying further upgrades. The live test covers a fresh installation and one synthetic Config upgrade; upgrades from actual prior-release snapshots and rollback with existing agency rows still need rehearsal against a disposable copy before production use.

## Running The Tool

Run commands from the `DatabaseInstaller` project root:

```bash
dotnet run --project DatabaseInstaller -- <command> [options]
```

Or run the built executable directly:

```bash
DatabaseInstaller.exe <command> [options]
```

Command-line arguments override values from `appsettings.json` and user secrets.

## Available Commands

| Command | Purpose |
| --- | --- |
| `update` | Apply migrations for all contexts and optionally seed the admin user |
| `transfer-config` | Copy configuration data from the ATSPM Config API into the target database |
| `transfer` | Transfer event logs from SQL Server into the current database format |
| `transfer-speed` | Transfer speed events from SQL Server into compressed event logs |
| `translate` | Translate logs from the initial compression format |
| `transfer-daily-hourly` | Convert daily compressed logs to hourly compressed logs |
| `setup-test` | Seed test data using `devices.json` |
| `copy-sql` | Copy event logs from SQL Server into PostgreSQL |

## `update`

Applies migrations for `ConfigContext`, `AggregationContext`, `EventLogContext`, and `IdentityContext`.
If `--seed-admin` is supplied, the command also seeds an admin user.

### Options

- `--config-connection`
- `--aggregation-connection`
- `--eventlog-connection`
- `--identity-connection`
- `--provider`
- `--admin-email`
- `--admin-password`
- `--admin-role`
- `--seed-admin`

### Example

```bash
dotnet run --project DatabaseInstaller -- update \
  --provider PostgreSQL \
  --config-connection "Host=localhost;Database=ATSPM-Config;Username=postgres;Password=postgres" \
  --aggregation-connection "Host=localhost;Database=ATSPM-Aggregation;Username=postgres;Password=postgres" \
  --eventlog-connection "Host=localhost;Database=ATSPM-EventLogs;Username=postgres;Password=postgres" \
  --identity-connection "Host=localhost;Database=ATSPM-Identity;Username=postgres;Password=postgres" \
  --seed-admin true \
  --admin-email admin@example.com \
  --admin-password "ChangeMe123!" \
  --admin-role Admin
```

## `transfer-config`

Imports configuration records from the ATSPM Config API into the target database.

### Options

- `--api-base-url`
- `--api-key`
- `--delete`
- `--update-locations`
- `--update-speed`

### Notes

- `--api-key` is required when importing configuration. The key is sent to the Config API in the `X-API-KEY` header.
- With only `--update-speed`, `--delete` removes only existing speed devices before importing them again. Speed products, device configurations, locations, and other device types are preserved.
- With `--update-locations`, `--delete` clears the existing target configuration before inserting new data.
- `--update-locations` imports locations and related configuration data.
- `--update-speed` imports speed devices.
- Without `--delete`, `--update-speed` preserves existing device records and IDs. Each new device is attached to the current active target version of its location. A speed device is skipped when its ID, location, or device identifier already exists; only new speed devices are inserted.
- Before inserting speed devices, the command verifies that their shared speed product and device configuration exist in the target database. Either dependency is inserted from the source when its ID is missing.

### Example

```bash
dotnet run --project DatabaseInstaller -- transfer-config \
  --api-base-url "https://atspm.udot.utah.gov/config/" \
  --api-key "<key>" \
  --update-locations true
```

## `transfer`

Transfers historical event logs from SQL Server into the event log store.

### Options

- `--source`
- `--start`
- `--end`
- `--device`
- `--batch`
- `--locations`

### Notes

- `--source` is the SQL Server connection string.
- `--start` and `--end` define the transfer range.
- `--device` limits the transfer to a device type id.
- `--batch` controls insert batch size.
- `--locations` accepts a comma-separated list of location identifiers.
- If `--batch` is omitted, the service uses a default batch size of `500`.

### Example

```bash
dotnet run --project DatabaseInstaller -- transfer \
  --source "Server=localhost;Database=MOE;Trusted_Connection=True;TrustServerCertificate=True" \
  --start "2024-01-01" \
  --end "2024-01-31" \
  --locations "LOC1,LOC2" \
  --batch 250
```

## `transfer-speed`

Transfers speed events from SQL Server into compressed event logs.

The command reads all speed events for one hour in a single timestamp-range query and groups them by configured location in memory. Source timestamps are used as stored; the command does not apply a UTC or local-time conversion. A failed hourly query is retried once after five seconds, then the skipped hour is logged clearly.

### Options

- `--source`
- `--start`
- `--end`

### Example

```bash
dotnet run --project DatabaseInstaller -- transfer-speed \
  --source "Server=localhost;Database=MOE;Trusted_Connection=True;TrustServerCertificate=True" \
  --start "2024-01-01" \
  --end "2024-01-31"
```

## `translate`

Translates logs from the initial compression format into the current hourly format.

### Options

- `--source`
- `--start`
- `--end`
- `--batch`
- `--device`
- `--locations`

### Example

```bash
dotnet run --project DatabaseInstaller -- translate \
  --source "Server=localhost;Database=MOE;Trusted_Connection=True;TrustServerCertificate=True" \
  --start "2024-01-01" \
  --end "2024-01-31" \
  --batch 500
```

## `transfer-daily-hourly`

Converts daily compressed logs into hourly compressed logs.

### Options

- `--source`
- `--source-table`
- `--start`
- `--end`
- `--data-type`
- `--device`
- `--batch`

### Notes

- `--source-table` defaults to `public."CompressedEvents"`.
- `--data-type` defaults to `IndianaEvent`.

### Example

```bash
dotnet run --project DatabaseInstaller -- transfer-daily-hourly \
  --source "Host=localhost;Database=ATSPM;Username=postgres;Password=postgres" \
  --source-table "public.\"CompressedEvents\"" \
  --start "2024-01-01" \
  --end "2024-01-31" \
  --data-type IndianaEvent
```

## `setup-test`

Seeds test configuration and device data using `devices.json`.

### Options

- `--location-count`
- `--device-configuration-id`
- `--protocol`

### Notes

- `--protocol` filters seeded devices by protocol.
- Supported protocol values are `ftp`, `ftps`, and `http`.

### Example

```bash
dotnet run --project DatabaseInstaller -- setup-test \
  --location-count 10 \
  --device-configuration-id 2 \
  --protocol ftp
```

## `copy-sql`

Copies rows from the source SQL Server `CompressedEvents` table into the PostgreSQL `CompressedEvents` table.

### Options

- `--source`
- `--start`
- `--end`
- `--locations`
- `--device`
- `--batch-size`
- `--resume`
- `MaxConcurrency` from `TransferCommandConfiguration`

### Notes

- This command copies the stored compressed `Data` payload directly instead of decompressing and recompressing it.
- That makes it much faster than rebuilding each hourly record from expanded events.
- `MaxConcurrency` controls how many locations are processed in parallel. The default is `2`. Start there and increase carefully.
- `--batch-size` controls how many rows are sent in each insert batch. The default is `250`.
- `--resume` builds a batched resume plan for the requested locations and date range, then skips location-days where every SQL Server source key already exists in PostgreSQL. Partial days are replayed safely because inserts use `ON CONFLICT DO NOTHING`.
- Each location worker keeps one SQL Server connection and one PostgreSQL connection open for all of its batches.
- The command reads `TransferCommandConfiguration:MaxConcurrency` and `TransferCommandConfiguration:CopyBatchSize` from `appsettings.json` or user secrets, so you can tune both without changing the command line.
- This assumes the source SQL Server database is already using the same `CompressedEvents` schema and payload format as the target PostgreSQL database.
- If you need to migrate older raw event tables instead, use `transfer` instead of `copy-sql`.

### Example

```bash
dotnet run --project DatabaseInstaller -- copy-sql \
  --source "Server=localhost;Database=ATSPM-EventLogs;Trusted_Connection=True;TrustServerCertificate=True" \
  --start "2024-01-01" \
  --end "2024-01-31" \
  --batch-size 500 \
  --resume \
  --locations "LOC1,LOC2"
```

## Configuration

`DatabaseInstaller` reads from `appsettings.json`, environment variables, and user secrets.

For `copy-sql`, the most relevant settings are:

- `TransferCommandConfiguration:Source` for the source SQL Server connection string
- `TransferCommandConfiguration:MaxConcurrency` for the number of location workers
- `TransferCommandConfiguration:CopyBatchSize` for the insert batch size
- `DatabaseConfiguration:EventLogContext` for the PostgreSQL destination connection

Useful sections:

- `ConnectionStrings` for the database contexts
- `CommandLineOptions` for `update`
- `TransferDailyToHourlyConfiguration` for `transfer-daily-hourly`
- `TransferCommandConfiguration` for `transfer`, `transfer-speed`, and `translate`
- `TransferCommandConfiguration:CopyBatchSize` for `copy-sql`
- `TransferCommandConfiguration:MaxConcurrency` for `copy-sql`
- `TransferConfigCommandConfiguration` for `transfer-config`

## Important Note

`copy-sql` is intended for moving already-compressed event log rows between compatible ATSPM event log databases. It is not the right tool for upgrading legacy raw event log tables into the compressed schema.
