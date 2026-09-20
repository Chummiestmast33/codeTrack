# Entorno de desarrollo

[English](en/environment.md) - [Secretos](secretos.md) - [Despliegue](despliegue.md)

Requisitos: PowerShell 7, .NET SDK 10, Node.js 24/npm, Git y Docker Desktop (contenedores Linux).
Ejecuta desde la raíz salvo indicación contraria. Sustituye `<...>`/`xxxx`.
Las especificaciones RF/RN/D en español siguen siendo normativas.

## Configuración local

Este bloque inicia una configuración local nueva con los defaults de `.env.example`.
Conserva los valores si ya tienes una base operativa. Cambiar `.env` no cambia la contraseña
del volumen PostgreSQL existente. La restricción de letras/dígitos del ejemplo solo simplifica
el escapado de Compose; no es una regla de contraseñas de la aplicación.

ASP.NET no lee `.env`: Compose traduce sus claves. Development carga UserSecrets, pero la
factory de EF no los carga. Las variables de entorno prevalecen sobre UserSecrets.

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

## Arrancar la API

Configura admin, reportes y storage opcional con [secretos](secretos.md) antes de arrancar.
Usa la misma terminal de configuración. Si dotnet-ef ya existe, sustituye install por
`dotnet tool update --global dotnet-ef --version '10.0.*'`.

```powershell
dotnet restore Backend/Backend.slnx
dotnet tool install --global dotnet-ef --version '10.0.*'
docker compose up -d db
dotnet ef database update --project Backend --connection $env:ConnectionStrings__DefaultConnection
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = 'http://localhost:5245'
dotnet run --project Backend --no-launch-profile
```

La conexión explícita es necesaria: `TallerDbContextFactory` solo lee
`ConnectionStrings__DefaultConnection`; sin ella usa credenciales ficticias. Usa formato Npgsql
`Host=...;...`. La normalización URI está implementada solo en el registro del runtime.
`migrations list` y `migrations remove` pueden consultar la base; no todos los comandos EF son offline.
La API no aplica migraciones al arrancar. Development expone Scalar en `/scalar/v1`.

## Verificar y arrancar frontend

En otra terminal en la raíz, con admin y encabezados de reportes configurados:

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

`/health` debe devolver HTTP 200 y `status: Healthy`; compara `appliedMigrations` con los archivos
de migración del checkout, no con un conteo fijo permanente. Login debe devolver 200.

```powershell
if (!(Test-Path frontend/.env)) { Copy-Item frontend/.env.example frontend/.env }
Push-Location frontend
npm ci
npm run dev
Pop-Location
```

`frontend/.env` usa `VITE_API_URL=http://localhost:5245`. Reinicia Vite al modificarlo.
`API_PORT=8080` controla el puerto host de la API de Compose, no la URL del frontend.
Alternativa contenerizada: configura todos los valores necesarios en `.env` según la tabla de secretos:

```powershell
docker compose up -d --build api
Invoke-RestMethod 'http://localhost:8080/health'
```

Configura el frontend con puerto 8080 para esta alternativa. Compose no consume UserSecrets.
Detén los contenedores sin eliminar datos:

```powershell
docker compose stop api db
```

## Checks y diagnóstico

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

- Falta JWT/conexión: completa la configuración y usa `Development` con `--no-launch-profile`.
- `28P01`: contraseña incorrecta o conexión ficticia de EF; usa conexión explícita.
- Error CORS: configura el origen frontend en `Cors:AllowedOrigins:0` y reinicia la API.
- D-02 sigue pendiente; deja vacío `Auth:AllowedEmailDomain` hasta aprobar el dominio.
- Login 403: revisa aprobación y estado activo.
- Subida no disponible: sigue la [verificación de storage](almacenamiento.md).
- Entorno remoto: [despliegue](despliegue.md); no ejecutes pruebas en producción por defecto.
