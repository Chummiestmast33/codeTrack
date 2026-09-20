# Development environment

[Español](../entorno-pruebas.md) - [Secrets](secrets.md) - [Deployment](deployment.md)

Use PowerShell 7, .NET SDK 10, Node.js 24/npm, Git and Docker Desktop (Linux containers).
Commands start at the repository root unless stated otherwise. Replace `<...>`/`xxxx` placeholders.
Spanish RF/RN/D specifications remain normative.

## Local configuration

This initializes a new local database configuration with the defaults in `.env.example`.
Preserve existing configuration if your database already works. Changing `.env` does not change
a password inside an existing PostgreSQL volume. The letters/digits restriction below only
simplifies this local example's Compose quoting; it is not an application password rule.

ASP.NET does not read `.env`: Compose maps its keys to .NET configuration. Development loads
UserSecrets, but the EF design-time factory does not. Environment variables override UserSecrets.

```powershell
if (!(Test-Path .env)) { Copy-Item .env.example .env }
$dbPassword = Read-Host 'Local database password' -MaskInput
$jwtBytes = [byte[]]::new(48)
[System.Security.Cryptography.RandomNumberGenerator]::Fill($jwtBytes)
$jwtSecret = [Convert]::ToBase64String($jwtBytes)
if ($dbPassword -notmatch '^[a-zA-Z0-9]{16,}$') { throw 'Use at least 16 letters/digits for this local example.' }
$compose = Get-Content .env -Raw
$compose = $compose -replace '(?m)^POSTGRES_PASSWORD=.*$', "POSTGRES_PASSWORD=$dbPassword"
$compose = $compose -replace '(?m)^JWT_SECRET=.*$', "JWT_SECRET=$jwtSecret"
[IO.File]::WriteAllText((Join-Path (Get-Location) '.env'), $compose)
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=codetrack;Username=codetrack;Password=$dbPassword"
dotnet user-secrets set 'ConnectionStrings:DefaultConnection' $env:ConnectionStrings__DefaultConnection --project Backend
dotnet user-secrets set 'Jwt:Secret' $jwtSecret --project Backend
dotnet user-secrets set 'Jwt:Issuer' 'codetrack' --project Backend
dotnet user-secrets set 'Jwt:Audience' 'codetrack' --project Backend
Remove-Variable dbPassword,jwtSecret,jwtBytes,compose
```

## Start the API

Configure admin, reports and optional storage using [secrets](secrets.md) before starting.
Run in the same terminal as local configuration. If dotnet-ef already exists, replace install
with `dotnet tool update --global dotnet-ef --version '10.0.*'`.

```powershell
dotnet restore Backend/Backend.slnx
dotnet tool install --global dotnet-ef --version '10.0.*'
docker compose up -d db
dotnet ef database update --project Backend --connection $env:ConnectionStrings__DefaultConnection
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = 'http://localhost:5245'
dotnet run --project Backend --no-launch-profile
```

The explicit migration connection is required: `TallerDbContextFactory` reads only
`ConnectionStrings__DefaultConnection`, otherwise it uses dummy credentials. Use Npgsql
`Host=...;...` format here. URI normalization is implemented only in runtime registration.
`migrations list` and `migrations remove` may contact the database; not all EF commands are offline.
The API does not apply migrations at startup. Development exposes Scalar at `/scalar/v1`.

## Verify and start the frontend

In a second terminal at the root, with the configured admin and report headers:

```powershell
$api = 'http://localhost:5245'
Invoke-RestMethod "$api/health"
Invoke-WebRequest "$api/openapi/v1.json" -OutFile (Join-Path $env:TEMP 'codetrack-openapi.json')
$login = @{ controlNumber = '<admin-control-number>'; password = (Read-Host 'Admin password' -MaskInput) }
$auth = Invoke-RestMethod "$api/api/auth/login" -Method Post -ContentType 'application/json' -Body ($login | ConvertTo-Json)
$headers = @{ Authorization = "Bearer $($auth.token)" }
Invoke-RestMethod "$api/api/topics" -Headers $headers
Invoke-WebRequest "$api/api/reports/attendance?format=csv" -Headers $headers -OutFile (Join-Path $env:TEMP 'attendance.csv')
Invoke-WebRequest "$api/api/reports/progress?format=pdf" -Headers $headers -OutFile (Join-Path $env:TEMP 'progress.pdf')
Remove-Variable login,auth,headers
```

`/health` must return HTTP 200 and `status: Healthy`; compare `appliedMigrations` with the
migration files in this checkout, not a permanently fixed count. Login must return 200.

```powershell
if (!(Test-Path frontend/.env)) { Copy-Item frontend/.env.example frontend/.env }
Push-Location frontend
npm ci
npm run dev
Pop-Location
```

`frontend/.env` uses `VITE_API_URL=http://localhost:5245`. Restart Vite after changing it.
`API_PORT=8080` controls the optional Compose API host port, not the frontend API URL.
For the container alternative, configure all required values in `.env` using the secrets mapping:

```powershell
docker compose up -d --build api
Invoke-RestMethod 'http://localhost:8080/health'
```

Set the frontend API URL to port 8080 for this alternative. Compose does not consume UserSecrets.
Stop local containers without deleting database data:

```powershell
docker compose stop api db
```

## Checks and troubleshooting

```powershell
dotnet restore Backend/Backend.slnx
dotnet build Backend/Backend.slnx --no-restore --nologo
dotnet test Backend/Backend.slnx --no-build --nologo
Push-Location frontend
npm ci
npm run lint
npm run build
Pop-Location
```

- Missing JWT/connection: complete configuration and use `Development` with `--no-launch-profile`.
- `28P01`: wrong database password or EF dummy connection; use explicit connection.
- CORS error: configure the frontend origin in `Cors:AllowedOrigins:0` and restart the API.
- D-02 domain is undecided; leave `Auth:AllowedEmailDomain` empty until approved.
- Login 403: check approval and active account status.
- File upload unavailable: follow [storage verification](storage.md).
- Remote setup: [deployment](deployment.md); tests do not target production by default.
