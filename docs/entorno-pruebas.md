# Entorno local y pruebas (guía onboarding)

Guía completa para que cualquier persona externa levante el backend desde cero,
pruebe los endpoints con Scalar y apague todo. Empieza aquí si es tu primera
vez en el repo. Todo corre en tu máquina con Docker; la base de producción
será Supabase.

## 0. Mapa rápido del repo

- `Backend/`: API ASP.NET Core (`Controllers/`, `Domain/`, `Application/`,
  `Infrastructure/`, `Middleware/`, `OpenApi/`).
- `Backend.Tests/`: pruebas xUnit (dominio, aplicación, infraestructura y API).
- `frontend/src/`: estructura vacía; el frontend se construye después del backend.
- `docs/`: requisitos (`requisitos-funcionales.md`), reglas (`reglas-negocio.md`,
  incluye la histórica RN-02 cancelada), decisiones (`decisiones-pendientes.md`),
  arquitectura y esta guía.
- `docker-compose.yml`: Postgres local para desarrollo.
- `.env.example`: plantilla de variables para Compose (nunca commitear `.env`).

## 1. Requisitos

- .NET 10 SDK (`dotnet --version`).
- Docker Desktop activo (si está en pausa, `docker compose` falla).
- Puertos libres: `5432` (Postgres) y `8080` o `5245` (API).
- Herramienta `dotnet-ef` (ya usada en este proyecto; si falta:
  `dotnet tool install --global dotnet-ef`).

## 2. Secretos y variables: cómo funciona

Hay **dos mundos** y no se mezclan solos:

| Mundo | Lee de | Formato de nombres | Cuándo se usa |
|---|---|---|---|
| Docker Compose | archivo `.env` | `JWT_SECRET`, `POSTGRES_PASSWORD` | `docker compose up` |
| .NET (`dotnet run`, `dotnet ef`) | entorno del proceso + UserSecrets + `appsettings.json` | jerárquico con `__`: `Jwt__Secret`, `ConnectionStrings__DefaultConnection` | todo lo que corras en tu terminal |

Reglas clave:

- **ASP.NET no lee `.env`.** Es convención de Compose; quien traduce
  `JWT_SECRET` → `Jwt__Secret` es el servicio `api` del compose. En tu
  terminal, `JWT_SECRET` plano **no lo lee la app**: debe ser `Jwt__Secret`.
- **Las `$env:` de PowerShell mueren al cerrar la terminal.** Por eso en
  desarrollo usamos **UserSecrets**: persisten por usuario, viven fuera del
  repo (`%APPDATA%\Microsoft\UserSecrets\`) y nunca se commitean.
- El `fail-fast` al arrancar (`Jwt Secret must be at least 32 characters`,
  `ConnectionStrings:DefaultConnection is not configured`) es intencional:
  impide correr sin secretos (RNF-14). Si lo ves, faltan variables, no es
  un bug del código.

### 2.1. Primera vez: registrar secretos dev

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=codetrack;Username=codetrack;Password=<tu-password-local>" --project Backend
dotnet user-secrets set "Jwt:Secret" "<secreto-de-minimo-32-caracteres>" --project Backend
dotnet user-secrets set "Jwt:Issuer" "codetrack" --project Backend
dotnet user-secrets set "Jwt:Audience" "codetrack" --project Backend
```

Verifica solo las **claves** (nunca pegues valores en chats ni issues):

```powershell
dotnet user-secrets list --project Backend
```

### 2.2. `.env` para Compose

```powershell
Copy-Item .env.example .env
```

Pon valores a `POSTGRES_PASSWORD` y `JWT_SECRET` (mínimo 32 caracteres).
`ALLOWED_EMAIL_DOMAIN` se deja vacío hasta definir el dominio (D-02).
El password de `.env` y el de UserSecrets deben coincidir en local
(ambos apuntan al mismo Postgres).

## 3. Arranque desde cero

```powershell
docker compose up -d db
dotnet ef database update --project Backend
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5245"
dotnet run --project Backend --no-launch-profile
```

La variable `ASPNETCORE_ENVIRONMENT` es **obligatoria**: con
`--no-launch-profile` no se lee `launchSettings.json` y el default es
`Production`, donde ni UserSecrets ni Scalar están disponibles.
Confirma en el log la línea `Hosting environment: Development`.

Notas:

- `database update` necesita conexión **real**: la toma de UserSecrets,
  de `$env:ConnectionStrings__DefaultConnection` o del flag
  `--connection "<cadena>"`. Sin ella verás `28P01: password authentication
  failed` (usa un placeholder interno) — pon tu cadena y reintenta.
- Los comandos que **no** conectan (`migrations add/list/script/remove`)
  funcionan sin variables gracias al factory de diseño
  (`Infrastructure/Persistence/DesignTime/`).
- Alternativa contenerizada: `docker compose up api` (puerto `8080`;
  ciclo de cambios más lento que `dotnet run`).

### 3.1. Cuenta admin inicial y salud

- Si configuras la sección `Admin` (UserSecrets `Admin:*` o `ADMIN_*` en
  `.env`), al arrancar se crea el administrador una sola vez si no existe
  (idempotente; nunca loguea secretos). Sin config, el arranque avisa y sigue.
- Verifica `GET /health` → `200 {"status":"Healthy",...}` con Postgres
  arriba; `503` con la base caída. Compose lo usa como healthcheck del api.
- Auditoría RF-24: `CreatedAt/UpdatedAt` (UTC) más `CreatedBy/UpdatedBy`
  (actor del JWT, nulo en seed/sistema). Migración `AuditActors`.

## 4. Probar la API con Scalar

Abre `http://localhost:5245/scalar/v1` (solo existe en `Development`).
El JSON OpenAPI está en `http://localhost:5245/openapi/v1.json`.

Flujo guiado:

1. `POST /api/auth/register` con número de control, nombre, correo y
   contraseña (mínimo 8). Respuesta `201`; la cuenta queda pendiente (D-01).
2. `POST /api/auth/login` con tus credenciales. Si la cuenta sigue
   pendiente, recibirás `403`. Para avanzar, aprueba la cuenta directo en
   la base (solo dev):
   ```sql
   UPDATE "Users" SET "ApprovalStatus" = 'Approved' WHERE "ControlNumber" = 'tu-numero';
   ```
3. Vuelve a `POST /api/auth/login` y copia el `token` de la respuesta.
4. En Scalar pulsa **Authorize**, pega el token como Bearer y prueba:
   - `GET /api/users/me` → `200` con tus datos (nunca incluye el hash).
   - `GET /api/admin/users/pending` → `403` con rol estudiante;
     con un token de administrador → `200`.

Respuestas de error (RFC 7807, `application/problem+json`):

| Situación | Status |
|---|---|
| Validación (campos, dominio de correo si configurado) | `400` con `errors` |
| Sin token o token inválido | `401` |
| Cuenta pendiente/rechazada/desactivada, o sin rol | `403` |
| Recurso inexistente | `404` |
| Número de control duplicado | `409` |
| Fallo inesperado (detalle genérico, se loguea) | `500` |

`Backend/Backend.http` tiene los mismos flujos listos para VS/Rider.

## Frontend (fase 1: auth, layouts, 404, admin usuarios)

```powershell
cd frontend
npm install
npm run dev
```

La app usa `VITE_API_URL` para el backend (default
`http://localhost:5245`; créalo en `frontend/.env` si usas otro puerto).
`VITE_PERIOD` muestra el periodo junto al nombre del taller (vacío = oculto);
ver `frontend/.env.example`. Ningún dato personal vive en el repo: encabezado
de reportes por `Reports:*` y periodo visible por `VITE_PERIOD`.
Rutas: `/login`, `/register`, `/pending`, `/app/*` (estudiante aprobado),
`/admin/*` (rol Administrator), `*` → 404. El token vive en
`localStorage`; el rol para guardias sale del JWT (el backend revalida
todo). Textos en `src/locales/*.json` (`es` completo, resto con fallback).

La API solo acepta los orígenes de `Cors:AllowedOrigins`
(`http://localhost:5173`, `:5199`, `:8080` por default; en producción se
fijan con `Cors__AllowedOrigins__0`, ...). Si el navegador bloquea el login
con error de CORS, verifica que el origen del frontend esté en esa lista.

Entregas con archivo: ver `docs/almacenamiento.md` (bucket Supabase,
mapeo de variables y verificación ticket → PUT → confirmación).
## 5. Troubleshooting (errores ya vistos en este proyecto)

| Error | Causa | Fix |
|---|---|---|
| `Jwt Secret must be at least 32 characters` al arrancar o con `dotnet ef` | Faltan variables en esa terminal, o usaste `JWT_SECRET` plano en vez de `Jwt__Secret` / `Jwt:Secret` | Registra UserSecrets (§2.1) o exporta `$env:Jwt__Secret` en la sesión |
| `28P01: password authentication failed for user "codetrack"` en `database update` | Sin conexión real configurada (se usó el placeholder interno) | Pon `ConnectionStrings:DefaultConnection` real o usa `--connection` |
| `Unable to resolve service for DbContextOptions<TallerDbContext>` | Secuela del error anterior: el arranque falló antes de registrar servicios | Arregla las variables y reintenta; no es bug del contexto |
| `Docker Desktop is manually paused` / compose no responde | Docker en pausa | Reanúdalo desde el menú Whale o el Dashboard |
| `The "POSTGRES_PASSWORD" variable is not set` en `docker compose ps` | No hay `.env` o la terminal no lo ve | Crea `.env` desde `.env.example` (§2.2) |
| `Jwt Secret must be at least 32 characters` al arrancar aunque ya guardaste UserSecrets | El host está en `Production`: UserSecrets **solo se cargan en Development**. Pasa con `--no-launch-profile` sin exportar la variable | Exporta `$env:ASPNETCORE_ENVIRONMENT = "Development"` y verifica `Hosting environment: Development` en el log |
| Scalar 404 (`/scalar/v1` no existe) | API corriendo fuera de `Development` (misma causa anterior) | Misma solución: entorno `Development` |

## 6. Apagado

1. Detén la API con `Ctrl+C`.
2. `docker compose stop db` (los datos persisten en el volumen `pgdata`).
3. Solo si quieres empezar de cero: `docker compose down -v` (borra la base
   local) y repite desde el paso 3.

## 7. Producción (Supabase)

Usa su conexión **directa** (puerto 5432, `SSL Mode=Require`) para
migraciones; el pooler (6543) puede usarse en runtime. Los secretos de
producción van en el proveedor de hosting, nunca en el repo ni en `.env`.

### Encabezado oficial de reportes (RN-08)
Proyecto, periodo, responsable y asesor son configuración, no código:

```powershell
dotnet user-secrets set "Reports:ProjectName" "..." --project Backend
dotnet user-secrets set "Reports:Period" "..." --project Backend
dotnet user-secrets set "Reports:Responsible" "..." --project Backend
dotnet user-secrets set "Reports:Advisor" "..." --project Backend
```

En Compose equivalen a `REPORT_PROJECT_NAME`, `REPORT_PERIOD`,
`REPORT_RESPONSIBLE` y `REPORT_ADVISOR` del `.env`. Sin estos valores,
exportar un reporte falla con un error claro en vez de salir en blanco.

## 8. CI y freno local

- **GitHub Actions** (`.github/workflows/ci.yml`): corre en cada `push` y
  `pull request`, más disparo manual desde la pestaña Actions. Backend:
  restore + build + `dotnet test` (sin BD ni secretos). Frontend:
  `npm ci` + `lint` + `build`. El badge del README refleja `main`.
  Recomendado: activar branch protection en `main` exigiendo el check.
- **Hook pre-push local** (opcional pero recomendado): mismo freno antes
  de subir. Instalar una vez con Git Bash:
  ```powershell
  Copy-Item scripts/pre-push .git/hooks/pre-push
  ```
  Corre `dotnet test` siempre y `lint` solo si el push toca `frontend/`.
  Bypass (no recomendado): `git push --no-verify`. La garantía real es
  el CI; el hook es solo ahorro de tiempo.
