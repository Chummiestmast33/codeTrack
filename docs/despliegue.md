# Despliegue: Supabase, Render y Vercel

[English](en/deployment.md) - [Secretos](secretos.md) - [Storage](almacenamiento.md)

Requisitos: PowerShell 7 desde la raíz, .NET 10/dotnet-ef 10, Node.js 24/npm y cuentas
autenticadas en los proveedores. Sustituye todos los placeholders antes de ejecutar.
Los ejemplos modifican el entorno remoto elegido: usa un proyecto de pruebas para validarlos.

## Base y storage

Copia host/usuario directos y de sesión del panel Connect autenticado de Supabase.
No deduzcas el host por región. La conexión directa normalmente requiere IPv6; el pooler
compartido de sesión admite IPv4. La política del proyecto usa sesión en 5432, no transacción
en 6543. Es una restricción del proyecto, no una afirmación universal sobre todos los clientes.
Consulta [conexiones Supabase](https://supabase.com/docs/guides/database/connecting-to-postgres).

Aplica migraciones explícitamente antes del primer arranque:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Host=db.<ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password="<database-password>";SSL Mode=Require'
dotnet ef database update --project Backend --connection $env:ConnectionStrings__DefaultConnection
dotnet ef migrations list --project Backend --connection $env:ConnectionStrings__DefaultConnection
Remove-Item Env:ConnectionStrings__DefaultConnection
```

Si no hay conectividad directa, usa host del pooler de sesión y usuario `postgres.<ref>`
en el mismo comando Npgsql. Usa comillas en contraseñas con punto y coma y duplica comillas
dobles internas. No uses percent-encoding para contraseñas en formato clave/valor.
El runtime admite URI `postgresql://`/`postgres://` y decodifica credenciales percent-encoded;
la factory EF no normaliza URI. Prefiere clave/valor, especialmente con delimitadores de
cadena de conexión: el conversor URI del runtime actualmente interpola valores decodificados.

Crea bucket privado y credenciales S3 según [storage](almacenamiento.md).
La API no aplica migraciones ni crea storage al arrancar. Compara la lista de migraciones
con el checkout actual; no fijes un conteo esperado permanente.

## API en Render

En el panel autenticado crea un Web Service Docker ligado al repositorio.
Root Directory `Backend`, Dockerfile `./Dockerfile` y Docker build context `.`.
Health path `/health`. El entrypoint escucha en `0.0.0.0:$PORT` (8080 sin PORT).
Render termina HTTPS público; no pongas una URL en `PORT` ni `API_PORT`.

Introduce estos valores en Environment, sustituyendo placeholders allí sin versionar archivos.
Los tres orígenes CORS iguales sustituyen los tres defaults localhost de appsettings;
si autorizas varios frontends, asigna sus respectivos orígenes.

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

La creación del servicio y edición de Environment se hacen en el panel en esta guía.
Las asignaciones locales no cambian Render. Elige Deploy tras guardar. Para despliegues
posteriores, copia el hook secreto de Settings y ejecuta:

```powershell
$deployHook = Read-Host 'Render deploy hook URL' -MaskInput
Invoke-WebRequest -Uri $deployHook -Method Post | Select-Object StatusCode
Remove-Variable deployHook
```

Consulta [deploy hooks Render](https://render.com/docs/deploy-hooks). Nunca publiques su URL.
El seed crea un admin aprobado una vez y omite cuentas existentes.
Un fallo del seed puede permitir que arranque el proceso: verifica login además de health.

## Frontend en Vercel

Crea/vincula el proyecto con framework Vite, Root Directory `frontend`, build `npm run build`
y salida `dist`. Ejecuta desde la raíz y vincula ese proyecto (no entres además en frontend
cuando Root Directory ya está configurado como frontend):

```powershell
npx vercel login
npx vercel link
npx vercel env add VITE_API_URL production
npx vercel env add VITE_PERIOD production
npx vercel deploy --prod
```

Responde con `https://<your-api>.onrender.com` para `VITE_API_URL` (sin barra final) y el
periodo deseado. Omite el comando del periodo si no se usa.
Para claves existentes, usa `npx vercel env update VITE_API_URL production` y
`npx vercel env update VITE_PERIOD production`; después redespliega.
[Documentación CLI de variables](https://vercel.com/docs/cli/env).

`VITE_API_URL` debe existir en Vercel antes de compilar. `API_PORT` solo selecciona el puerto
host de Compose. Cambiar Render no actualiza el bundle frontend. Redespliega y confirma en
Network del navegador que las peticiones van a tu API pública y no a localhost.

## Verificación y operación

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

Espera health 200/Healthy, login 200, descarga del reporte y `Access-Control-Allow-Origin`
igual al frontend. En la UI verifica login admin, aprobación de alumnos, escaneo QR y
[subida de archivos](almacenamiento.md). OpenAPI/Scalar solo existen en Development.
Revisa fallos de arranque/seed en Render Logs sin copiar credenciales a reportes.

Para revertir código, redespliega un commit conocido desde el panel del proveedor.
La reversión de base requiere un plan revisado de respaldo/migraciones; no ejecutes down
migrations automáticamente. Consulta [pendientes](pendientes.md).

## Prueba opcional de admin y QR

Usa una sesión desechable existente y un alumno de prueba pendiente. Recrea login/headers admin
con el bloque anterior (antes de Remove-Variable). Aprobar modifica datos de prueba:

```powershell
$studentId = '<pending-test-student-guid>'
Invoke-RestMethod "$api/api/admin/users/$studentId/approve" -Headers $headers -Method Post | Select-Object id,approvalStatus
$sessionId = '<test-session-guid>'
Invoke-RestMethod "$api/api/admin/sessions/$sessionId/qr/regenerate" -Headers $headers -Method Post | Out-Null
Invoke-WebRequest "$api/api/admin/sessions/$sessionId/qr/image" -Headers $headers -OutFile (Join-Path $env:TEMP 'attendance-qr.png')
```

Abre el PNG y escanéalo como alumno aprobado; confirma que abre el frontend configurado.
