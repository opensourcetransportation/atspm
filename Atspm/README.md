# ATSPM local Docker Compose setup

This guide starts the ATSPM web UI, four APIs, and PostgreSQL on one development machine. It uses a self-signed certificate and binds published ports to `127.0.0.1` for local troubleshooting. Treat this Compose file as a **local development and evaluation setup**, not a production deployment template. No connection to a traffic signal controller is required for the initial startup; the database will have no signal data until you import configuration and events.

## Prerequisites

- Docker Engine or Docker Desktop with Linux containers and the Compose v2 plugin (`docker compose version`).
- OpenSSL available on your command line (`openssl version`).
- Free local ports 3000, 3443, 5432, and 44400–44403. Docker must be able to pull the images and build the .NET and WebUI projects.

Run all commands from the `Atspm` directory containing `docker-compose.yml`. On Windows PowerShell, use `cd Atspm` from the repository root; on Linux or macOS, use the equivalent shell command. Compose resolves relative bind mounts from this directory.

## 1. Create your private configuration

Copy the tracked template to an ignored `.env` file:

```powershell
Copy-Item .env.example .env
```

On Linux or macOS, use `cp .env.example .env` and restrict access with `chmod 600 .env`. Edit these blank values in `.env`:

| Variable | Purpose |
|---|---|
| `POSTGRES_PASSWORD` | Password for the local PostgreSQL server and, by default, all four ATSPM database contexts. |
| `ADMIN_EMAIL` and `ADMIN_PASSWORD` | First administrator account. The password must contain an uppercase letter, lowercase letter, number, and symbol. |
| `JWT_KEY` | Random signing key of at least 32 bytes. `openssl rand -hex 32` produces a suitable value to paste here. |
| `CERT_PASSWORD` | Password for the development `aspnetapp.pfx` certificate generated below. |

Set `POSTGRES_USER` if you do not want `atspm`. Compose supplies matching PostgreSQL defaults for Config, Aggregation, EventLog, and Identity contexts. Existing per-context `DatabaseConfiguration__<Context>__...` variables still override those defaults, so a deployment with a separate database server can keep its explicit settings. The installer now reads the same context configuration as the APIs; its standalone CLI still supports connection-string options.

Do not reuse the formerly tracked `.env` as an installation template: it had obsolete provider keys, sample credentials, and an incorrectly cased certificate variable. If you already have a working installation, keep a private copy of its settings and compare them with `.env.example` before switching Compose versions. Docker does not migrate existing anonymous PostgreSQL data into the new `postgres_data` volume automatically; back up and restore any data you need to keep before changing that mount.

The environment file is ignored by Git. Keep it private and do not commit generated certificates or passwords. The checked-in `frontend-env.txt` holds public UI endpoints and map settings, not the passwords above.

## 2. Generate local HTTPS certificates

The same certificate is used by Nginx (`aspnetapp.crt` and `aspnetapp.key`) and the ASP.NET APIs (`aspnetapp.pfx`). Generate it for `localhost`. The password used for the PFX **must match** `CERT_PASSWORD` in `.env`.

PowerShell:

```powershell
New-Item -ItemType Directory -Force nginx/certs | Out-Null
$env:CERT_PASSWORD = '<the CERT_PASSWORD value from your .env>'
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout nginx/certs/aspnetapp.key -out nginx/certs/aspnetapp.crt -subj '/CN=localhost' -addext 'subjectAltName=DNS:localhost,IP:127.0.0.1'
openssl pkcs12 -export -out nginx/certs/aspnetapp.pfx -inkey nginx/certs/aspnetapp.key -in nginx/certs/aspnetapp.crt -passout env:CERT_PASSWORD
Remove-Item Env:CERT_PASSWORD
```

Linux or macOS shell: replace `New-Item` with `mkdir -p nginx/certs`, set `export CERT_PASSWORD='<same value>'`, run the two `openssl` commands, then `unset CERT_PASSWORD`. On Linux, the Config API container runs as the .NET `app` user; it must be able to read the bind-mounted PFX. For this **local-only** certificate, `chmod 644 nginx/certs/aspnetapp.pfx` makes it readable to that user. Keep the containing development directory private; use an agency-approved certificate and access model for any production deployment.

The default certificate mount is `./nginx/certs:/app/certs:ro`. `CERT_LOCATION` can override it, but a custom mount must keep `aspnetapp.pfx` at `/app/certs/aspnetapp.pfx` inside each API container. Nginx mounts the same host directory at `/etc/nginx/certs` and expects the two PEM files above. Existing mounts targeting `/root/.aspnet/https` need updating.

## 3. Validate configuration and start

```powershell
docker compose config --quiet
docker compose up --build -d
docker compose ps -a
```

`config --quiet` checks Compose syntax and required variables without starting containers. Missing secrets now stop at this step with a message naming the variable. `up` first waits for PostgreSQL health and the database-installer job; the APIs then start, followed by the UI and Nginx. Initial image builds and migrations may take time. Check the installer status before treating the installation as complete:

```powershell
docker compose ps -a database-installer
docker compose logs --tail=80 database-installer
docker compose ps
```

Open `https://localhost:3443` and accept the browser warning for the **self-signed development certificate**. Nginx also listens on `http://localhost:3000`. The APIs are available through `/config/`, `/data/`, `/report/`, and `/identity/` under the HTTPS origin. Direct development ports 44400–44403 and PostgreSQL port 5432 bind only to the host loopback interface. Do not expose this setup to an untrusted network.

The `watchdog` and `eventlogutility` images are command-line tools, so they are in the optional `tools` profile instead of running with no command during initial startup. After configuring devices, inspect their commands with `docker compose --profile tools run --rm watchdog generate --help` and `docker compose --profile tools run --rm eventlogutility log --help`. A separate scheduler is needed for recurring collection and scans; `docker compose up` alone does not schedule them. See [DatabaseInstaller commands](DatabaseInstaller/README.md) for migration, transfer, and test-data options.

## Troubleshooting

- **Unset or empty variable:** Fill the named field in `.env`; rerun `docker compose config --quiet`. Compose uses `.env` next to the YAML file by default. For a separate file, use `docker compose --env-file <path> ...`.
- **Missing certificate file:** Confirm `aspnetapp.crt`, `aspnetapp.key`, and `aspnetapp.pfx` are all in `Atspm/nginx/certs`. A custom `CERT_LOCATION` must use the container destination described above.
- **Certificate password or permission error:** The PFX password must equal `CERT_PASSWORD`; on Linux make the PFX readable by the Config API container's `app` user for local testing.
- **Database connection failure:** Confirm PostgreSQL is healthy with `docker compose ps postgres`, and use `postgres` as the database host inside containers. `localhost` inside an API container is not the PostgreSQL service.
- **Installer did not finish:** Read `docker compose logs database-installer` and check `docker compose ps -a database-installer`. Do not treat running API containers as proof that all migrations and seeding succeeded.
- **Ports already in use:** Change only the host side of a Compose port mapping, or stop the conflicting local service. Internal container ports and the UI's `/config`, `/data`, `/report`, and `/identity` paths stay the same.
- **Existing data before this revision:** The old Compose file did not declare `postgres_data`. Preserve the old database/volume and make a verified backup before switching to the new named volume. Removing volumes with `docker compose down -v` deletes their data.

To stop the stack while keeping the named PostgreSQL volume, run `docker compose down`. This guide does not include production backup, TLS trust, secret management, or controller-network hardening.
