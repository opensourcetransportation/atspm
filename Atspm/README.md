# ATSPM: quick local installation

Start the web UI, APIs, and PostgreSQL on your computer with Docker Compose. **Follow steps 1–5 for a fresh installation.** You can leave the optional settings alone.

You do not need to install .NET, Node.js, or PostgreSQL separately; Docker builds and runs them. Signal configuration and controller events can be imported after startup, so an empty dashboard on the first login is normal.

## Before you start

Have these ready:

- **Docker running**, with Linux containers and Compose v2: `docker compose version`.
- **Git** to clone the repository, or use the ZIP download below.
- **OpenSSL** for the local HTTPS certificate: `openssl version`.

On Windows, if Git is installed in its default location but `openssl` is not found, run this in PowerShell, then retry `openssl version`:

```powershell
Set-Alias -Name openssl -Value "$env:ProgramFiles\Git\usr\bin\openssl.exe"
```

Keep that PowerShell window open for the setup commands. The default setup needs local ports **3000, 3443, 5432, and 44400–44403** available.

## 1. Get the project

```sh
git clone --branch main https://github.com/OpenSourceTransportation/ATSPM.git atspm
cd atspm/Atspm
```

Or [download the ZIP](https://github.com/OpenSourceTransportation/ATSPM/archive/refs/heads/main.zip), extract it, and open a terminal in its **`Atspm` folder**.

**Run all remaining commands from the folder containing `docker-compose.yml` and `.env.example`.**

## 2. Create your .env file

If you already have a `.env`, keep it and read [Existing installations](#existing-installations) before changing it. For a fresh installation, copy the template below.

On Windows PowerShell:

```powershell
Copy-Item .env.example .env
notepad .env
```

On Linux or macOS:

```sh
cp .env.example .env
chmod 600 .env
```

Then open `.env` in your text editor.

### Sample .env file

The [`.env.example` template](.env.example) contains these settings. **Fill the four empty values and change the admin email, then save the file.**

```env
# Database: Docker creates and configures the databases for you.
POSTGRES_USER=atspm
POSTGRES_PASSWORD=

# Your first login.
ADMIN_EMAIL=admin@example.org
ADMIN_PASSWORD=
ADMIN_ROLE=Admin
SEED_ADMIN=true

# API sign-in tokens.
JWT_KEY=
JWT_ISSUER=ATSPM-local
JWT_EXPIRE_DAYS=1

# Password for the HTTPS certificate you create in step 3.
CERT_PASSWORD=

# Local development default.
ALLOWED_HOSTS=*
```

**Change these five settings:**

| Setting | What to enter |
| --- | --- |
| `POSTGRES_PASSWORD` | Choose a database password. Compose passes it to PostgreSQL, the installer, and all four API database contexts automatically. |
| `ADMIN_EMAIL` | Your login email. SMTP is optional for the initial seeded admin login. |
| `ADMIN_PASSWORD` | Your login password. It must have an uppercase letter, lowercase letter, number, and symbol, and be at least 6 characters long. Choose a longer password for your own installation. |
| `JWT_KEY` | Generate a unique random value with the command below, then paste its entire output here. |
| `CERT_PASSWORD` | Choose a certificate password. Enter this same value when prompted in step 3. |

Generate the JWT signing key:

```sh
openssl rand -hex 32
```

For a value containing `$`, `#`, or spaces, use **single quotes in `.env`** so it is read literally, for example `ADMIN_PASSWORD='YourOwnPasswordWith$And#'`. Enter the actual password without these surrounding quotes when logging in or answering the certificate prompt. Do not share the completed `.env` file; it is ignored by Git.

See [Docker's `.env` syntax reference](https://docs.docker.com/compose/how-tos/environment-variables/variable-interpolation/#env-file-syntax) for quoting details.

**These defaults can stay as they are:**

| Setting | What it does |
| --- | --- |
| `POSTGRES_USER=atspm` | Creates the PostgreSQL login used by the application. |
| `ADMIN_ROLE=Admin` | Gives the seeded account administrator access. |
| `SEED_ADMIN=true` | Creates the admin account during first installation. |
| `JWT_ISSUER=ATSPM-local` | Identifies the issuer of the API sign-in tokens. |
| `JWT_EXPIRE_DAYS=1` | Makes those tokens expire after one day. |
| `ALLOWED_HOSTS=*` | Allows host headers for this local setup. |

You do not need to write connection strings or choose database providers for the bundled PostgreSQL setup. Compose uses `postgres:5432` and creates Config, Aggregation, EventLog, and Identity databases through the installer.

## 3. Create the local HTTPS certificate

Run **one** of the following blocks. When prompted, enter the `CERT_PASSWORD` saved in `.env`.

<details open>
<summary>Windows PowerShell</summary>

```powershell
New-Item -ItemType Directory -Force nginx/certs | Out-Null
$env:CERT_PASSWORD = Read-Host 'Enter CERT_PASSWORD from your .env'
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout nginx/certs/aspnetapp.key -out nginx/certs/aspnetapp.crt -subj '/CN=localhost' -addext 'subjectAltName=DNS:localhost,IP:127.0.0.1'
openssl pkcs12 -export -out nginx/certs/aspnetapp.pfx -inkey nginx/certs/aspnetapp.key -in nginx/certs/aspnetapp.crt -passout env:CERT_PASSWORD
Remove-Item Env:CERT_PASSWORD
```

</details>

<details>
<summary>Linux or macOS</summary>

```sh
mkdir -p nginx/certs
printf 'Enter CERT_PASSWORD from your .env: '
read -r CERT_PASSWORD
export CERT_PASSWORD
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout nginx/certs/aspnetapp.key -out nginx/certs/aspnetapp.crt -subj '/CN=localhost' -addext 'subjectAltName=DNS:localhost,IP:127.0.0.1'
openssl pkcs12 -export -out nginx/certs/aspnetapp.pfx -inkey nginx/certs/aspnetapp.key -in nginx/certs/aspnetapp.crt -passout env:CERT_PASSWORD
unset CERT_PASSWORD
chmod 644 nginx/certs/aspnetapp.pfx
```

The PFX must be readable by the Config API container's `app` user. Keep the containing development directory private.

</details>

You should now have **three files** in `nginx/certs`: `aspnetapp.crt`, `aspnetapp.key`, and `aspnetapp.pfx`. Keep them private. The default Compose mounts are already configured.

## 4. Build and start ATSPM

```sh
docker compose config --quiet
docker compose up --build -d
docker compose ps -a
```

The first command finishes silently when the configuration is valid. The first build downloads dependencies and can take time. Compose waits for PostgreSQL, runs the database installer, then starts the APIs, UI, and Nginx.

**Check that installation succeeded:**

```sh
docker compose ps -a database-installer
docker compose logs --tail=80 database-installer
```

The installer should show **`Exited (0)`**, meaning its one-time job finished successfully. PostgreSQL should be healthy and the APIs, UI, and Nginx should be running. If the installer exits with a nonzero code, check its logs before continuing.

## 5. Open ATSPM and sign in

Open **[https://localhost:3443](https://localhost:3443)** on the same computer.

Accept the browser's warning for the self-signed local certificate. Sign in with **`ADMIN_EMAIL` and `ADMIN_PASSWORD` from `.env`**. The installer already created the account; registration is not needed for that admin.

**You are ready to configure locations and import event data.** Empty charts are expected until you add data. See the [database installer guide](DatabaseInstaller/README.md) for transfer and test-data commands.

## Stop, start again, or update

| Task | Command, run from `Atspm` |
| --- | --- |
| Stop and keep your database | `docker compose down` |
| Start again | `docker compose up -d` |
| View service status | `docker compose ps -a` |
| View recent API errors | `docker compose logs --tail=80 identityapi configapi dataapi reportapi` |
| Get newer source | `git pull --ff-only origin main` after saving local changes |
| Rebuild after a source update | `docker compose up --build -d` |

Keep your private `.env`, certificates, and database backups when updating. **Do not add `-v` to the stop command unless you intend to delete the database volume.**

## Troubleshooting

| Problem | What to check |
| --- | --- |
| `docker` cannot connect | Start Docker and select Linux containers. Retry `docker compose version` and startup. |
| A required variable is missing | Fill the empty value in `.env`, save it, and rerun `docker compose config --quiet`. Check that the filename is `.env`, not `.env.txt`, beside `docker-compose.yml`. |
| `openssl` is not found on Windows | Use the Git OpenSSL alias under [Before you start](#before-you-start), or make your installed OpenSSL available on PATH. |
| Certificate file/password error | Check that all three certificate files exist. The PFX export password must match `.env` exactly. If you used the wrong password, repeat step 3, then run `docker compose up -d`. |
| Database installer failed | Read `docker compose logs --tail=80 database-installer`. Check the database password and admin password requirements. Inside containers the bundled database host is `postgres`, not `localhost`. |
| A port is already in use | Stop the application using the conflicting port, or change only the host side of its Compose port mapping. |
| UI opens but requests fail | Check the installer first, then API logs using the command above. Keep `frontend-env.txt`'s relative `/config`, `/data`, `/report`, and `/identity` endpoints for this setup. |
| Login fails after editing `ADMIN_PASSWORD` | Seeding does not reset an existing account's password. Use its existing password or the application's password-reset process once email delivery is configured. |

## Optional settings

<details>
<summary>Email delivery and WatchDog</summary>

Add these to `.env` only when you need email delivery. Use your SMTP provider's actual settings:

```env
EmailConfiguration__Host=smtp.example.org
EmailConfiguration__Port=587
EmailConfiguration__UserName=you@example.org
EmailConfiguration__Password='your-smtp-password'
EmailConfiguration__EnableSsl=true
```

These configure Identity email and WatchDog alerts. They are separate from your admin login credentials.

WatchDog and event collection are optional command-line services. They do not run or schedule collection just because the web stack is up. After configuring devices, inspect their commands:

```sh
docker compose --profile tools run --rm watchdog generate --help
docker compose --profile tools run --rm eventlogutility log --help
```

For WatchDog, set `WatchdogConfiguration__TimeZoneId` to the location's time zone, such as `America/Denver`, and optionally set `WatchdogConfiguration__DefaultEmailAddress` for alerts. A scheduler is needed for recurring collection and scans.

</details>

<details>
<summary>Database overrides, certificates, and local ports</summary>

For a separate database server, Compose accepts `DatabaseConfiguration__<Context>__...` overrides. The four context names and bundled database defaults are:

| Context | Database |
| --- | --- |
| `ConfigContext` | `ATSPM-Config` |
| `AggregationContext` | `ATSPM-Aggregation` |
| `EventLogContext` | `ATSPM-EventLogs` |
| `IdentityContext` | `ATSPM-Identity` |

Each context supports `DBType`, `Host`, `Port`, `Database`, `User`, and `Password`. For example, `DatabaseConfiguration__ConfigContext__Host` overrides only the Config database host. Use an address reachable from the containers. The APIs and installer read the same overrides. Leave these unset for the bundled PostgreSQL installation.

`CERT_LOCATION` optionally overrides the API certificate mount. Its default is `./nginx/certs:/app/certs:ro`; a custom mount must keep `aspnetapp.pfx` at `/app/certs/aspnetapp.pfx` inside the APIs. Nginx uses the CRT and key in `/etc/nginx/certs`.

| Service | Local address |
| --- | --- |
| Web UI through Nginx | `https://localhost:3443` or `http://localhost:3000` |
| Config API | `https://localhost:44400` |
| Data API | `https://localhost:44401` |
| Report API | `https://localhost:44402` |
| Identity API | `https://localhost:44403` |
| PostgreSQL | `localhost:5432` |

Published ports bind to `127.0.0.1`. This guide is for local development and evaluation. A shared or public deployment needs its own trusted certificates, network settings, secret management, and backups.

</details>

## Existing installations

Keep a private backup of your settings and database before switching versions. Compare your `.env` with [`.env.example`](.env.example); the older `CONFIG_CONNECTION`/`ConfigContext_Provider` style keys are not used by this Compose configuration.

The current PostgreSQL data mount uses the named `postgres_data` volume. **It does not automatically import an older anonymous volume.** Preserve the old volume and use a verified backup/restore when moving existing data. Changing `POSTGRES_PASSWORD` in `.env` alone also does not change the password of an already initialized PostgreSQL database.

Older API certificate mounts under `/root/.aspnet/https` need updating to the current `/app/certs` destination. Keep the same admin credentials for an existing account; repeated seeding does not reset its password.
