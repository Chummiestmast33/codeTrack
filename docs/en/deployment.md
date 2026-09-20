# Deployment: Supabase, Render and Vercel

[Español](../despliegue.md) - [Secrets](secrets.md) - [Storage](storage.md)

Use PowerShell 7 at the repository root, .NET 10/dotnet-ef 10, Node.js 24/npm,
and authenticated provider accounts. Replace every placeholder before execution.
Examples configure or modify the selected remote environment: use a test project for validation.

## Database and storage

In Supabase's authenticated Connect panel, copy the actual direct and session-pooler host/user.
Do not infer a pooler host from the region. Direct connections normally require IPv6;
the shared session pooler supports IPv4. The project's runtime policy uses session mode
on port 5432, not transaction mode on 6543. This is a project constraint, not a universal
claim that all transaction-pooler applications fail.
See [Supabase connections](https://supabase.com/docs/guides/database/connecting-to-postgres).

Apply migrations explicitly before the first start:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Host=db.<ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password="<database-password>";SSL Mode=Require'
dotnet ef database update --project Backend --connection $env:ConnectionStrings__DefaultConnection
dotnet ef migrations list --project Backend --connection $env:ConnectionStrings__DefaultConnection
Remove-Item Env:ConnectionStrings__DefaultConnection
```

If direct connectivity is unavailable, use the session pooler host and `postgres.<ref>`
username in that same Npgsql command. Quote password values containing semicolons;
double embedded double quotes. Do not percent-encode Npgsql key/value passwords.
Runtime also accepts `postgresql://`/`postgres://` URIs and decodes percent-encoded credentials;
the EF factory does not normalize URIs. Prefer key/value strings, especially for passwords
with connection-string delimiters; the runtime URI converter currently interpolates decoded values.

Create a private bucket and S3 credentials using [storage](storage.md). The API does not
apply migrations or provision storage at startup. Compare the migration listing with the
current checkout; do not hardcode a permanent expected count.

## Render API

In Render's authenticated dashboard create a Docker Web Service linked to your repository.
Use repository root `Backend`, Dockerfile `./Dockerfile` and Docker build context `.`.
Set health path `/health`. The entrypoint binds `0.0.0.0:$PORT` (8080 when PORT is absent).
Render terminates public HTTPS; do not place a URL in `PORT` or `API_PORT`.

Enter these settings in Environment, replacing placeholders there without committing a file.
The three CORS entries deliberately use the same production origin to override all three
localhost defaults from appsettings; for multiple approved frontends, set the corresponding origins.

```dotenv
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=<session-pooler-host>;Port=5432;Database=postgres;Username=postgres.<ref>;Password=<database-password>;SSL Mode=Require
Jwt__Secret=<at-least-32-characters>
Jwt__Issuer=codetrack
Jwt__Audience=codetrack
Jwt__ExpiryMinutes=60
Admin__ControlNumber=<admin-control-number>
Admin__FullName=<admin-name>
Admin__Email=admin@example.invalid
Admin__Password=<admin-password>
Reports__ProjectName=<workshop-name>
Reports__Period=<period>
Reports__Responsible=<responsible-person>
Reports__Advisor=<advisor-name>
Storage__Endpoint=https://xxxx.storage.supabase.co/storage/v1/s3
Storage__Region=<storage-region>
Storage__Bucket=submissions
Storage__AccessKey=<s3-access-key>
Storage__SecretKey=<s3-secret-key>
Storage__UploadUrlExpiryMinutes=15
Storage__DownloadUrlExpiryMinutes=60
Qr__FrontendBaseUrl=https://<your-app>.vercel.app
Qr__ExpiryMinutes=120
Qr__ImageSizePixels=512
Cors__AllowedOrigins__0=https://<your-app>.vercel.app
Cors__AllowedOrigins__1=https://<your-app>.vercel.app
Cors__AllowedOrigins__2=https://<your-app>.vercel.app
Auth__AllowedEmailDomain=
```

Provider creation and Environment editing are dashboard operations in this guide. Local
environment assignments do not change Render. Choose Deploy after saving; for later deploys,
copy the secret hook from Settings and use:

```powershell
$deployHook = Read-Host 'Render deploy hook URL' -MaskInput
Invoke-WebRequest -Uri $deployHook -Method Post | Select-Object StatusCode
Remove-Variable deployHook
```

See [Render deploy hooks](https://render.com/docs/deploy-hooks). Never publish the hook URL.
Admin seed creates an approved admin once; existing accounts are skipped.
If seed fails, the process may still start: verify login separately from health.

## Vercel frontend

Create/link the Vercel project with framework Vite, Root Directory `frontend`, build
`npm run build` and output `dist`. Run the following from the repository root and link that
project (do not additionally change into frontend when its configured Root Directory is frontend):

```powershell
npx vercel login
npx vercel link
npx vercel env add VITE_API_URL production
npx vercel env add VITE_PERIOD production
npx vercel deploy --prod
```

The env prompts receive `https://<your-api>.onrender.com` for `VITE_API_URL` (no trailing slash)
and the desired period. Skip the optional period command if unused.
For existing keys, use `npx vercel env update VITE_API_URL production` and
`npx vercel env update VITE_PERIOD production`, then deploy again.
[CLI environment documentation](https://vercel.com/docs/cli/env).

`VITE_API_URL` must exist on Vercel before compilation. `API_PORT` only selects the Compose
host port. Changing Render variables does not update the frontend bundle. Redeploy and confirm
the browser's Network tab targets your public API instead of localhost.

## Verification and operations

```powershell
$api = 'https://<your-api>.onrender.com'
Invoke-RestMethod "$api/health"
$login = @{ controlNumber = '<admin-control-number>'; password = (Read-Host 'Admin password' -MaskInput) }
$auth = Invoke-RestMethod "$api/api/auth/login" -Method Post -ContentType 'application/json' -Body ($login | ConvertTo-Json)
$headers = @{ Authorization = "Bearer $($auth.token)" }
Invoke-RestMethod "$api/api/topics" -Headers $headers
Invoke-WebRequest "$api/api/reports/progress?format=csv" -Headers $headers -OutFile (Join-Path $env:TEMP 'progress.csv')
Invoke-WebRequest "$api/api/auth/login" -Method Options -Headers @{
  Origin = 'https://<your-app>.vercel.app'
  'Access-Control-Request-Method' = 'POST'
  'Access-Control-Request-Headers' = 'content-type'
} | Select-Object StatusCode,Headers
Remove-Variable login,auth,headers
```

Expect health 200/Healthy, login 200, report download and an `Access-Control-Allow-Origin`
matching the frontend. In the UI verify admin login, student approval, QR scan and the
[file upload flow](storage.md). OpenAPI/Scalar are Development-only.
Use Render Logs for startup/seed errors; do not copy credentials into reports.

To roll back code, redeploy a known-good commit in the provider's dashboard. Database rollback
is separate and requires a reviewed backup/migration plan; do not automatically run down migrations.
Track remaining work in [pendientes](../pendientes.md).

## Optional admin and QR smoke check

Use an existing disposable session and a pending test student. Recreate the admin login/headers
from the verification block above (before its Remove-Variable command). Approval changes test data:

```powershell
$studentId = '<pending-test-student-guid>'
Invoke-RestMethod "$api/api/admin/users/$studentId/approve" -Headers $headers -Method Post | Select-Object id,approvalStatus
$sessionId = '<test-session-guid>'
Invoke-WebRequest "$api/api/admin/sessions/$sessionId/qr/image" -Headers $headers -OutFile (Join-Path $env:TEMP 'attendance-qr.png')
```

Open the PNG and scan it as the approved student; confirm it opens the configured frontend.
