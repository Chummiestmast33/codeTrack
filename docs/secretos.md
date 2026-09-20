# Secretos y configuración (referencia única)

Todos los secretos y valores por despliegue del proyecto en un solo lugar.
Reglas de oro (RNF-14):

- Nunca van en el repo, ni en chats, ni en logs. Solo nombres de claves aquí.
- Desarrollo (`dotnet run`): **UserSecrets** (`dotnet user-secrets set "Seccion:Clave" ... --project Backend`).
- Compose: **`.env`** (ver `.env.example`, jamás commitear `.env`).
- Producción (Render): **Environment** con nombres jerárquicos (`Seccion__Clave`).
- Frontend (Vercel): `VITE_*` se incrustan al compilar (`frontend/.env`, jamás commitear).
- Ver solo nombres sin exponer valores: revisa las claves, no los valores
  (p. ej. confirma qué claves existen antes de asumir que falta config).

## 1. Arranque (obligatorios, fail-fast sin ellos)

| .NET (`dotnet user-secrets set`) | `.env` / Render | Notas |
|---|---|---|
| `ConnectionStrings:DefaultConnection` acepta formato `Clave=Valor`
(`Host=...;Port=...;...`) o URI `postgresql://user:pass@host:port/db`
(se normaliza al arrancar; error claro si no se entiende). | Compose: vía `POSTGRES_*` / Render: `ConnectionStrings__DefaultConnection` | Local: `Host=localhost;...`; migraciones Supabase: directa; runtime: session pooler. Ver §7 |
| `Jwt:Secret` (≥32 chars) | `JWT_SECRET` / `Jwt__Secret` | Firma de tokens. Sin esto nada arranca |

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=codetrack;Username=codetrack;Password=<tu-password-local>" --project Backend
dotnet user-secrets set "Jwt:Secret" "<secreto-de-minimo-32-caracteres>" --project Backend
```

## 2. Admin inicial (requerido para operar; arranque avisado sin él)

| .NET | `.env` / Render |
|---|---|
| `Admin:ControlNumber` | `ADMIN_CONTROL_NUMBER` / `Admin__ControlNumber` |
| `Admin:FullName` | `ADMIN_FULL_NAME` / `Admin__FullName` |
| `Admin:Email` | `ADMIN_EMAIL` / `Admin__Email` |
| `Admin:Password` (mín. 8) | `ADMIN_PASSWORD` / `Admin__Password` |

```powershell
dotnet user-secrets set "Admin:ControlNumber" "..." --project Backend
dotnet user-secrets set "Admin:FullName" "..." --project Backend
dotnet user-secrets set "Admin:Email" "..." --project Backend
dotnet user-secrets set "Admin:Password" "..." --project Backend
```

Se crea una sola vez si no existe (idempotente, nunca loguea secretos).

## 3. Encabezado de reportes (requerido para PDF/CSV)

| .NET | `.env` / Render |
|---|---|
| `Reports:ProjectName` | `REPORT_PROJECT_NAME` / `Reports__ProjectName` |
| `Reports:Period` | `REPORT_PERIOD` / `Reports__Period` |
| `Reports:Responsible` | `REPORT_RESPONSIBLE` / `Reports__Responsible` |
| `Reports:Advisor` | `REPORT_ADVISOR` / `Reports__Advisor` |

```powershell
dotnet user-secrets set "Reports:ProjectName" "..." --project Backend
dotnet user-secrets set "Reports:Period" "..." --project Backend
dotnet user-secrets set "Reports:Responsible" "..." --project Backend
dotnet user-secrets set "Reports:Advisor" "..." --project Backend
```

Sin estos, exportar falla claro (`reports.not-configured`) en vez de salir en blanco.

## 4. Storage S3 (requerido para entregas con archivo)

| .NET | `.env` / Render |
|---|---|
| `Storage:Endpoint` | `S3_ENDPOINT` / `Storage__Endpoint` |
| `Storage:Region` | `S3_REGION` / `Storage__Region` |
| `Storage:Bucket` | `S3_BUCKET` / `Storage__Bucket` |
| `Storage:AccessKey` | `S3_ACCESS_KEY_ID` / `Storage__AccessKey` |
| `Storage:SecretKey` | `S3_SECRET_ACCESS_KEY` / `Storage__SecretKey` |
| `Storage:UploadUrlExpiryMinutes` | `STORAGE_UPLOAD_EXPIRY_MINUTES` / igual con `__` |
| `Storage:DownloadUrlExpiryMinutes` | `STORAGE_DOWNLOAD_EXPIRY_MINUTES` / igual con `__` |

```powershell
dotnet user-secrets set "Storage:Endpoint" "https://xxxx.storage.supabase.co/storage/v1/s3" --project Backend
dotnet user-secrets set "Storage:Region" "us-west-2" --project Backend
dotnet user-secrets set "Storage:Bucket" "submissions" --project Backend
dotnet user-secrets set "Storage:AccessKey" "<access-key-id>" --project Backend
dotnet user-secrets set "Storage:SecretKey" "<secret>" --project Backend
```

Detalle completo (bucket privado, rotación, verificación): `docs/almacenamiento.md`.

## 5. Opcionales y por entorno

| Clave | Default | Notas |
|---|---|---|
| `Jwt:Issuer`, `Jwt:Audience` | `codetrack` | Igual en API y tokens |
| `Jwt:ExpiryMinutes` | `60` | Vida del token |
| `Auth:AllowedEmailDomain` | vacío (sin exigir, D-02 pendiente) | `.env`: `ALLOWED_EMAIL_DOMAIN` |
| `Qr:ExpiryMinutes` / `Qr:ImageSizePixels` | `120` / `512` | |
| `Qr:FrontendBaseUrl` | `http://localhost:5173` | **Cambiar en prod** a la URL de Vercel o los QR apuntan a local |
| `Cors:AllowedOrigins` | 3 localhost en `appsettings.json` | **Cambiar en prod**: `Cors__AllowedOrigins__0=https://code-track-two.vercel.app` |
| `VITE_API_URL` (frontend) | `http://localhost:5245` | **URL pública del backend en Vercel** (se incrusta al compilar) |
| `VITE_PERIOD` (frontend) | vacío (oculto) | Periodo visible junto al nombre |

## 6. Checklist producción (Render + Vercel + Supabase)

Backend (Render → Environment), todos con `__`:
`ConnectionStrings__DefaultConnection` (pooler sesión), `Jwt__Secret`,
`Jwt__Issuer`, `Jwt__Audience`, `Reports__ProjectName`, `Reports__Period`,
`Reports__Responsible`, `Reports__Advisor`, `Admin__ControlNumber`,
`Admin__FullName`, `Admin__Email`, `Admin__Password`,
`Storage__Endpoint`, `Storage__Region`, `Storage__Bucket`,
`Storage__AccessKey`, `Storage__SecretKey`,
`Cors__AllowedOrigins__0` (URL de Vercel), `Qr__FrontendBaseUrl` (URL de Vercel).

Frontend (Vercel → Environment, antes del build):
`VITE_API_URL` (URL pública del backend), `VITE_PERIOD` (opcional).

Supabase: migraciones con conexión directa, bucket privado + S3 keys
dedicadas de alcance mínimo. Rotar cualquier key expuesta fuera del
servidor. Verificación post-deploy: `/health` 200, login admin, descarga
de reporte con encabezado, ticket de subida.
