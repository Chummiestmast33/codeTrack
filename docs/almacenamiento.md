# Almacenamiento compatible con S3

[English](en/storage.md) - [Secretos](secretos.md) - [Despliegue](despliegue.md)

Los bytes viven fuera de PostgreSQL (RN-05). La API valida el ticket, firma una URL PUT y
acepta metadatos de entrega. El cliente envía bytes directamente a S3.
El límite es 5 MB (RN-06); también aplican modalidad, estado y fechas de actividad.
Usa una actividad/proyecto desechable para estos comandos. Requisitos: PowerShell 7, API
en ejecución, alumno aprobado y activo, actividad publicada que admita archivos.
Ejecuta desde la raíz y sustituye todos los placeholders.

## Crear y configurar

En el panel autenticado de Supabase Storage crea el bucket privado `submissions`.
Genera credenciales S3 dedicadas y copia endpoint y región de ese proyecto.
Crear credenciales es un paso de panel, no un comando local.
Las claves S3 pueden saltarse RLS y tener acceso amplio al storage; no presupongas que
existen claves limitadas por bucket. Usa restricciones del proveedor cuando están disponibles.
No expongas estas credenciales ni uses service-role como secreto S3.
[Autenticación S3 Supabase](https://supabase.com/docs/guides/storage/s3/authentication).

```powershell
dotnet user-secrets set 'Storage:Endpoint' 'https://xxxx.storage.supabase.co/storage/v1/s3' --project Backend
dotnet user-secrets set 'Storage:Region' '<storage-region>' --project Backend
dotnet user-secrets set 'Storage:Bucket' 'submissions' --project Backend
dotnet user-secrets set 'Storage:AccessKey' '<s3-access-key>' --project Backend
dotnet user-secrets set 'Storage:SecretKey' '<s3-secret-key>' --project Backend
dotnet user-secrets set 'Storage:UploadUrlExpiryMinutes' '15' --project Backend
dotnet user-secrets set 'Storage:DownloadUrlExpiryMinutes' '60' --project Backend
```

Reinicia la API tras cambiar valores. Para Compose/Render usa los mapeos de [secretos](secretos.md).
Sin endpoint/región/bucket/access key/secret se registra `UnconfiguredFileStorage`:
la API arranca pero fallan los tickets.
El adaptador admite proveedores compatibles como R2/MinIO con sus endpoint/región.
El PUT directo desde navegador requiere CORS del proveedor que permita el frontend;
CORS de la API no configura el almacenamiento.

Instala AWS CLI v2 para las comprobaciones independientes de bucket/descarga. Autentica
con credenciales S3 (no un personal access token Supabase) y verifica el bucket privado:

```powershell
$endpoint = 'https://xxxx.storage.supabase.co/storage/v1/s3'
$env:AWS_DEFAULT_REGION = '<storage-region>'
$env:AWS_ACCESS_KEY_ID = Read-Host 'S3 access key' -MaskInput
$env:AWS_SECRET_ACCESS_KEY = Read-Host 'S3 secret key' -MaskInput
aws s3api head-bucket --bucket submissions --endpoint-url $endpoint
```

head-bucket correcto termina con código 0. No publiques el bucket para evitar errores de firma.

## Ticket, subida y entrega

Elige un `.txt` permitido, no vacío y menor a 5 MB. La actividad debe aceptar solo archivo;
si exige URL además, añade `$submission.url = 'https://example.org/submission'` antes del POST.

```powershell
$api = 'http://localhost:5245'
$activityId = '<published-activity-guid>'
$login = @{ controlNumber = '<approved-student-control-number>'; password = (Read-Host 'Student password' -MaskInput) }
$auth = Invoke-RestMethod "$api/api/auth/login" -Method Post -ContentType 'application/json' -Body ($login | ConvertTo-Json)
$headers = @{ Authorization = "Bearer $($auth.token)" }
$file = Get-Item '<absolute-path-to-test-file.txt>'
$metadata = @{ fileName = $file.Name; contentType = 'text/plain'; fileSizeBytes = $file.Length }
$ticket = Invoke-RestMethod "$api/api/activities/$activityId/upload-ticket" -Headers $headers -Method Post -ContentType 'application/json' -Body ($metadata | ConvertTo-Json)
Invoke-WebRequest $ticket.uploadUrl -Method Put -InFile $file.FullName -ContentType $metadata.contentType | Select-Object StatusCode
$submission = $metadata.Clone()
$submission.storagePath = $ticket.storagePath
$submission.comment = 'Storage verification'
$result = Invoke-RestMethod "$api/api/activities/$activityId/submissions" -Headers $headers -Method Post -ContentType 'application/json' -Body ($submission | ConvertTo-Json)
$result | Select-Object id,versionNumber,status
Invoke-RestMethod "$api/api/activities/$activityId/submissions/me" -Headers $headers | Select-Object id,versionNumber,status
```

El ticket devuelve `uploadUrl`, `storagePath`, `expiresAt` y `versionNumber`.
El POST devuelve HTTP 201; cada reentrega conserva versiones anteriores.
Usa exactamente el Content-Type del ticket. Validar metadatos del ticket no implica inspeccionar
los bytes subidos; no describas PUT firmado como validación completa del tamaño real.

## Descarga independiente y limpieza

El adaptador S3 implementa GET firmado, pero ningún controlador actual expone descarga
y `SubmissionDto` contiene `storagePath`, no una URL firmada. Verifica S3 independientemente
mediante AWS CLI usando las variables anteriores; no es una prueba E2E de descarga de la aplicación:

```powershell
$downloadUrl = aws s3 presign "s3://submissions/$($ticket.storagePath)" --endpoint-url $endpoint --expires-in 60
$downloadPath = Join-Path $env:TEMP 'codetrack-storage-check.txt'
Invoke-WebRequest $downloadUrl -OutFile $downloadPath
if ((Get-FileHash $file.FullName).Hash -ne (Get-FileHash $downloadPath).Hash) { throw 'Downloaded content differs' }
# Only the disposable verification object:
aws s3api delete-object --bucket submissions --key $ticket.storagePath --endpoint-url $endpoint
Remove-Item Env:AWS_ACCESS_KEY_ID,Env:AWS_SECRET_ACCESS_KEY,Env:AWS_DEFAULT_REGION
Remove-Variable login,auth,headers,ticket,downloadUrl
```

Elimina solo el objeto desechable de verificación. Sus metadatos quedan en la base de pruebas;
no ejecutes esta limpieza sobre entregas reales.

## Fallos y rotación

- Fallo de ticket: revisa configuración y restricciones de actividad.
- PUT 403: verifica endpoint, región, expiración, reloj y Content-Type; tolerancias de reloj dependen del proveedor.
- Objeto ausente: verifica bucket/key y que PUT acabó antes de confirmar.
- Health comprueba PostgreSQL; no demuestra que storage funcione.
- Clave expuesta: revoca en el panel, crea reemplazo, repite configuración, reinicia y
  repite ticket/PUT/confirmación/descarga con credenciales de prueba.

E2E contra proveedor real requiere credenciales privadas y debe figurar sin verificar hasta ejecutarse.
