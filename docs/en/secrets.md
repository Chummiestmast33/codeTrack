# Secrets and configuration

[Español](../secretos.md) - [Environment](environment.md) - [Deployment](deployment.md) - [Storage](storage.md)

Run commands in PowerShell 7 from the repository root. Replace every `<...>`/`xxxx` placeholder
before use. Example domains and names are fictional. Do not commit secrets, paste them in PRs,
or print configuration values. `VITE_*` values are public and embedded during frontend compilation.

## Configuration map

UserSecrets apply to the Development host; Compose consumes root `.env`; Render uses hierarchical
environment names. A dash means Compose has no explicit mapping for that option.
ConnectionStrings in Compose is constructed from `POSTGRES_*`; the blank
`ConnectionStrings__DefaultConnection` entry in the template is not consumed by Compose.
Defaults below come from options/appsettings/Compose; placeholders are deployment-specific inputs.

| .NET / UserSecrets | Render / environment | Compose .env | Example / default |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | `POSTGRES_USER / POSTGRES_PASSWORD / POSTGRES_DB` | `<Npgsql-connection>` |
| `Jwt:Secret` | `Jwt__Secret` | `JWT_SECRET` | `<at-least-32-characters>` |
| `Jwt:Issuer` | `Jwt__Issuer` | `JWT_ISSUER` | `codetrack` |
| `Jwt:Audience` | `Jwt__Audience` | `JWT_AUDIENCE` | `codetrack` |
| `Jwt:ExpiryMinutes` | `Jwt__ExpiryMinutes` | `JWT_EXPIRY_MINUTES` | `60` |
| `Admin:ControlNumber` | `Admin__ControlNumber` | `ADMIN_CONTROL_NUMBER` | `<admin-control-number>` |
| `Admin:FullName` | `Admin__FullName` | `ADMIN_FULL_NAME` | `<admin-name>` |
| `Admin:Email` | `Admin__Email` | `ADMIN_EMAIL` | `admin@example.invalid` |
| `Admin:Password` | `Admin__Password` | `ADMIN_PASSWORD` | `<admin-password>` |
| `Reports:ProjectName` | `Reports__ProjectName` | `REPORT_PROJECT_NAME` | `<workshop-name>` |
| `Reports:Period` | `Reports__Period` | `REPORT_PERIOD` | `<period>` |
| `Reports:Responsible` | `Reports__Responsible` | `REPORT_RESPONSIBLE` | `<responsible-person>` |
| `Reports:Advisor` | `Reports__Advisor` | `REPORT_ADVISOR` | `<advisor-name>` |
| `Storage:Endpoint` | `Storage__Endpoint` | `S3_ENDPOINT` | `https://xxxx.storage.supabase.co/storage/v1/s3` |
| `Storage:Region` | `Storage__Region` | `S3_REGION` | `<storage-region>` |
| `Storage:Bucket` | `Storage__Bucket` | `S3_BUCKET` | `submissions` |
| `Storage:AccessKey` | `Storage__AccessKey` | `S3_ACCESS_KEY_ID` | `<s3-access-key>` |
| `Storage:SecretKey` | `Storage__SecretKey` | `S3_SECRET_ACCESS_KEY` | `<s3-secret-key>` |
| `Storage:UploadUrlExpiryMinutes` | `Storage__UploadUrlExpiryMinutes` | `STORAGE_UPLOAD_EXPIRY_MINUTES` | `15` |
| `Storage:DownloadUrlExpiryMinutes` | `Storage__DownloadUrlExpiryMinutes` | `STORAGE_DOWNLOAD_EXPIRY_MINUTES` | `60` |
| `Qr:FrontendBaseUrl` | `Qr__FrontendBaseUrl` | `QR_FRONTEND_BASE_URL` | `http://localhost:5173` |
| `Qr:ExpiryMinutes` | `Qr__ExpiryMinutes` | `QR_EXPIRY_MINUTES` | `120` |
| `Qr:ImageSizePixels` | `Qr__ImageSizePixels` | — | `512` |
| `Cors:AllowedOrigins:0` | `Cors__AllowedOrigins__0` | `CORS_ALLOWED_ORIGIN_0` | `http://localhost:5173` |
| `Cors:AllowedOrigins:1` | `Cors__AllowedOrigins__1` | `CORS_ALLOWED_ORIGIN_1` | `http://localhost:5199` |
| `Cors:AllowedOrigins:2` | `Cors__AllowedOrigins__2` | `CORS_ALLOWED_ORIGIN_2` | `http://localhost:8080` |
| `Auth:AllowedEmailDomain` | `Auth__AllowedEmailDomain` | `ALLOWED_EMAIL_DOMAIN` | `(empty)` |

## Set development values

Generate JWT and configure the local database using [environment](environment.md#local-configuration).
The following covers every other mapped option. Replace admin/storage/report placeholders.
Keep D-02's domain empty until approved. Omit storage settings entirely if testing URL-only submissions.

```powershell
dotnet user-secrets set 'Jwt:Issuer' 'codetrack' --project Backend
dotnet user-secrets set 'Jwt:Audience' 'codetrack' --project Backend
dotnet user-secrets set 'Jwt:ExpiryMinutes' '60' --project Backend
dotnet user-secrets set 'Admin:ControlNumber' '<admin-control-number>' --project Backend
dotnet user-secrets set 'Admin:FullName' '<admin-name>' --project Backend
dotnet user-secrets set 'Admin:Email' 'admin@example.invalid' --project Backend
dotnet user-secrets set 'Admin:Password' '<admin-password>' --project Backend
dotnet user-secrets set 'Reports:ProjectName' '<workshop-name>' --project Backend
dotnet user-secrets set 'Reports:Period' '<period>' --project Backend
dotnet user-secrets set 'Reports:Responsible' '<responsible-person>' --project Backend
dotnet user-secrets set 'Reports:Advisor' '<advisor-name>' --project Backend
dotnet user-secrets set 'Storage:Endpoint' 'https://xxxx.storage.supabase.co/storage/v1/s3' --project Backend
dotnet user-secrets set 'Storage:Region' '<storage-region>' --project Backend
dotnet user-secrets set 'Storage:Bucket' 'submissions' --project Backend
dotnet user-secrets set 'Storage:AccessKey' '<s3-access-key>' --project Backend
dotnet user-secrets set 'Storage:SecretKey' '<s3-secret-key>' --project Backend
dotnet user-secrets set 'Storage:UploadUrlExpiryMinutes' '15' --project Backend
dotnet user-secrets set 'Storage:DownloadUrlExpiryMinutes' '60' --project Backend
dotnet user-secrets set 'Qr:FrontendBaseUrl' 'http://localhost:5173' --project Backend
dotnet user-secrets set 'Qr:ExpiryMinutes' '120' --project Backend
dotnet user-secrets set 'Qr:ImageSizePixels' '512' --project Backend
dotnet user-secrets set 'Cors:AllowedOrigins:0' 'http://localhost:5173' --project Backend
dotnet user-secrets set 'Cors:AllowedOrigins:1' 'http://localhost:5199' --project Backend
dotnet user-secrets set 'Cors:AllowedOrigins:2' 'http://localhost:8080' --project Backend
dotnet user-secrets set 'Auth:AllowedEmailDomain' '' --project Backend
```

The admin seed is implemented and idempotent by control number: it skips an existing account,
does not reset its password, and logs a failure while allowing startup to continue.
Set all four admin fields and choose a strong password. The seed does not run the student
registration validator; do not claim an enforced seed password-length rule.
Reports require all four header fields (`reports.not-configured` otherwise).
Missing storage configuration allows startup but upload tickets fail.

## Compose and production values

To edit local Compose values without printing secrets (preserve existing values):

```powershell
if (!(Test-Path .env)) { Copy-Item .env.example .env }
notepad .env
```

Use the Compose column for admin, reports, storage, QR and CORS; use the local environment
guide to set JWT/database values. `API_PORT` is a numeric host port, normally `8080`.
For a process environment example (Render requires the same keys in its dashboard):

```powershell
$env:Reports__ProjectName = '<workshop-name>'
$env:Admin__ControlNumber = '<admin-control-number>'
$env:Cors__AllowedOrigins__0 = 'https://<your-app>.vercel.app'
$env:Qr__FrontendBaseUrl = 'https://<your-app>.vercel.app'
```

The deployment guide includes a complete Render key/value block. Local `$env:` commands do
not update a hosted service. Frontend configuration, from the root:

```powershell
if (!(Test-Path frontend/.env)) { Copy-Item frontend/.env.example frontend/.env }
notepad frontend/.env
```

Set `VITE_API_URL=https://<your-api>.onrender.com` and optional `VITE_PERIOD=<period>`.
Restart local Vite or redeploy Vercel. Setting these on Render does not configure Vercel.

## Verify names and rotate

The first command filters UserSecrets output to names; never run it unfiltered for diagnostics.
The second should print nothing; the third should print both ignored paths.

```powershell
dotnet user-secrets list --project Backend | ForEach-Object { ($_ -split ' = ', 2)[0] }
git ls-files -- .env frontend/.env
git check-ignore .env frontend/.env
docker compose config --quiet
```

If a real secret leaks, revoke/rotate it in the owning provider before pushing. Update
UserSecrets with the same `dotnet user-secrets set` command for that key, update private
`.env`/Render values, restart the service and run health/login/storage verification.
JWT rotation invalidates existing tokens. Admin seed values do not reset an existing password;
use the administrator password reset flow. Provider credential creation/revocation happens
in its authenticated dashboard; do not invent a local command that rotates remote credentials.
