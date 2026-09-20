# S3-compatible file storage

[Español](../almacenamiento.md) - [Secrets](secrets.md) - [Deployment](deployment.md)

File bytes live outside PostgreSQL (RN-05). The API validates ticket requests, signs a PUT URL,
then accepts submission metadata. The client sends bytes directly to S3.
The limit is 5 MB (RN-06); activity submission mode, state and dates also apply.
Use a disposable test activity/project for the commands below. PowerShell 7, a running API,
an approved active student and a published activity allowing files are prerequisites.
Run from the repository root. Replace all placeholders.

## Provision and configure

In the authenticated Supabase Storage dashboard create the private bucket `submissions`.
Generate dedicated S3 credentials and copy endpoint and region from that project.
Credential creation is a dashboard step, not a local command.
S3 access keys can bypass RLS and provide broad storage access; do not assume bucket-scoped
credentials exist. Use provider restrictions where supported. Never expose these credentials
or use a service-role key as the S3 secret.
[Supabase S3 authentication](https://supabase.com/docs/guides/storage/s3/authentication).

```powershell
dotnet user-secrets set 'Storage:Endpoint' 'https://xxxx.storage.supabase.co/storage/v1/s3' --project Backend
dotnet user-secrets set 'Storage:Region' '<storage-region>' --project Backend
dotnet user-secrets set 'Storage:Bucket' 'submissions' --project Backend
dotnet user-secrets set 'Storage:AccessKey' '<s3-access-key>' --project Backend
dotnet user-secrets set 'Storage:SecretKey' '<s3-secret-key>' --project Backend
dotnet user-secrets set 'Storage:UploadUrlExpiryMinutes' '15' --project Backend
dotnet user-secrets set 'Storage:DownloadUrlExpiryMinutes' '60' --project Backend
```

Restart the API after changing settings. For Compose and Render use the exact mappings in
[secrets](secrets.md). Without endpoint/region/bucket/access key/secret, the API registers
`UnconfiguredFileStorage`: startup works but upload tickets fail.
The S3 adapter supports compatible providers such as R2 and MinIO, with their endpoint/region.
Browser direct PUT also requires the provider's CORS configuration to allow your frontend;
API CORS alone does not configure object storage.

Install AWS CLI v2 for the independent bucket/download checks. Authenticate with the S3
credentials (not a Supabase personal access token), and verify the private bucket:

```powershell
$endpoint = 'https://xxxx.storage.supabase.co/storage/v1/s3'
$env:AWS_DEFAULT_REGION = '<storage-region>'
$env:AWS_ACCESS_KEY_ID = Read-Host 'S3 access key' -MaskInput
$env:AWS_SECRET_ACCESS_KEY = Read-Host 'S3 secret key' -MaskInput
aws s3api head-bucket --bucket submissions --endpoint-url $endpoint
```

Successful head-bucket has exit code 0. Do not use a public bucket to bypass signing errors.

## Ticket, upload and submission

Choose a nonempty allowed `.txt` file under 5 MB. The activity must accept file-only submissions;
if its mode requires a URL too, set `$submission.url = 'https://example.org/submission'` before POST.

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

The ticket returns `uploadUrl`, `storagePath`, `expiresAt` and `versionNumber`.
The submission POST returns HTTP 201; every resubmission preserves earlier versions.
Use exactly the ticket's content type for PUT. Ticket metadata validation does not imply the
API has inspected the uploaded bytes; do not describe signed PUT as a complete byte-level size check.

## Independent download and cleanup

Administrators can now download or preview each submitted file in the activity submissions panel.
The panel requests `GET /api/admin/submissions/{id}/file` with the API bearer token.
The backend checks the administrator's current approval/active status, resolves that exact version
and returns `downloadUrl` and `fileName`. Responses use Cache-Control no-store.
Unknown submissions and URL-only submissions return 404; students and inactive administrators
cannot obtain a ticket. Existing files require no migration or re-upload.

The browser fetches the signed URL without the API bearer token. Storage CORS must allow
GET from the frontend origin as well as PUT. Each click requests a fresh signed URL using
`Storage:DownloadUrlExpiryMinutes` (60 by default). Downloads preserve the filename.
Preview supports plain text/code/Markdown, PNG/JPEG/WebP and PDF in a dialog; text is escaped
and PDF bytes are displayed by the native browser viewer after checking the PDF header. Browser PDF support varies; download remains available.
A deleted object, expired URL or storage CORS failure displays an error with retry available.

To check the endpoint from PowerShell, use an authenticated administrator (not the student
headers in the upload example) and an existing submission with a file:

```powershell
$adminLogin = @{ controlNumber = '<admin-control-number>'; password = (Read-Host 'Admin password' -MaskInput) }
$adminAuth = Invoke-RestMethod "$api/api/auth/login" -Method Post -ContentType 'application/json' -Body ($adminLogin | ConvertTo-Json)
$adminHeaders = @{ Authorization = "Bearer $($adminAuth.token)" }
$submissionId = '<submission-guid>'
$fileTicket = Invoke-RestMethod "$api/api/admin/submissions/$submissionId/file" -Headers $adminHeaders
Invoke-WebRequest $fileTicket.downloadUrl -OutFile (Join-Path $env:TEMP 'submission-download.txt')
Remove-Variable adminLogin,adminAuth,adminHeaders,fileTicket
```

Alternatively, verify S3 independently with AWS CLI using the earlier upload variables.
This checks storage access, not application authorization:

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

Delete only the disposable verification object. Its submission metadata remains in the test database;
do not run this cleanup against real submissions.

## Failures and rotation

- Ticket failure: inspect storage configuration and activity constraints.
- PUT 403: verify endpoint, region, expiry, system clock and content type; provider clock tolerances vary.
- Missing object: check bucket/key and whether PUT completed before confirming.
- API health only checks PostgreSQL; it does not prove storage works.
- Leaked key: revoke in the provider dashboard, create replacement, repeat configuration, restart,
  then repeat ticket/PUT/confirmation/download with test credentials.

Real-provider E2E needs private credentials and must be reported as unverified until actually run.
