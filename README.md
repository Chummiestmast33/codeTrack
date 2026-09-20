# CodeTrack - Introduction to Competitive Programming

CodeTrack manages student approval, topics, sessions, manual and QR attendance,
versioned submissions, topic progress, and PDF/CSV reports for a programming workshop.

## Stack and structure

- ASP.NET Core 10 Controllers API, EF Core 10/Npgsql, PostgreSQL 17, MediatR and FluentValidation.
- JWT, ASP.NET password hashing, QRCoder, QuestPDF and AWSSDK.S3 signed uploads.
- React 19, Vite 8, Tailwind CSS 4, React Router and react-i18next. English UI currently falls back to Spanish for missing translations.
- `Backend/`: one modular project with `Controllers/`, `Domain/`, `Application/`, `Infrastructure/`, `Middleware/`, `Api/` and `OpenApi/`. Migrations: `Infrastructure/Persistence/Migrations/`. There is no separate backend `src/` project layout.
- `Backend.Tests/`: xUnit tests, included with the API in `Backend/Backend.slnx`.
- `frontend/src/`: API clients, components, features, pages, routes, styles, locales and types.
- `docker-compose.yml`: local PostgreSQL and optional API container; `Backend/Dockerfile`: API image.
- `docs/`: Spanish specifications and guides; `docs/en/`: English operational mirrors.

## Development quickstart

Install .NET SDK 10, Node.js 24/npm, Docker Desktop with Linux containers, and PowerShell 7.
Start at the repository root. Complete the [local configuration commands](docs/en/environment.md#local-configuration)
first, then configure the initial admin using [secrets](docs/en/secrets.md). In the same terminal:

```powershell
dotnet restore Backend/Backend.slnx
dotnet tool install --global dotnet-ef --version '10.0.*'
docker compose up -d db
dotnet ef database update --project Backend --connection $env:ConnectionStrings__DefaultConnection
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = 'http://localhost:5245'
dotnet run --project Backend --no-launch-profile
```

If dotnet-ef is installed, use `dotnet tool update --global dotnet-ef --version '10.0.*'`.
In another terminal at the repository root:

```powershell
Invoke-RestMethod 'http://localhost:5245/health'
if (!(Test-Path frontend/.env)) { Copy-Item frontend/.env.example frontend/.env }
Set-Location frontend
npm ci
npm run dev
```

Frontend: `http://localhost:5173`; development API explorer: `http://localhost:5245/scalar/v1`;
OpenAPI: `http://localhost:5245/openapi/v1.json`. Students require approval before signing in.

## Validation

From the repository root, backend first:

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

See the [CI workflow](.github/workflows/ci.yml).

## Documentation

| Topic | English | Español |
|---|---|---|
| Development | [Environment](docs/en/environment.md) | [Entorno](docs/entorno-pruebas.md) |
| Production | [Deployment](docs/en/deployment.md) | [Despliegue](docs/despliegue.md) |
| Configuration | [Secrets](docs/en/secrets.md) | [Secretos](docs/secretos.md) |
| Files | [Storage](docs/en/storage.md) | [Almacenamiento](docs/almacenamiento.md) |

Spanish remains normative: [RF](docs/requisitos-funcionales.md), [RN](docs/reglas-negocio.md),
[D](docs/decisiones-pendientes.md), [RNF](docs/requisitos-no-funcionales.md).
Also see [architecture](docs/arquitectura.md), [data](docs/arquitectura-y-datos.md),
[UI style](docs/guia-estilos.md) and [open work](docs/pendientes.md).
RN-02 remains canceled: control numbers are required and unique without a fixed institutional pattern.
Domain instants use UTC (RN-09). English guides do not redefine specification identifiers.

See the [sanitized secret audit and validation limits](docs/auditoria-secretos.md).
