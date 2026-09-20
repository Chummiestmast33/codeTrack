# Almacenamiento de archivos (S3-compatible)

Las entregas con archivo usan subida directa con URLs firmadas S3: el
backend firma, el cliente sube los bytes al storage y luego confirma la
entrega con la ruta (`StoragePath`). La base de datos guarda solo metadata
(RN-05) y el backend nunca toca los bytes (RNF-08, RNF-17).

Funciona con Supabase Storage (vía su endpoint S3), Cloudflare R2 o MinIO
cambiando endpoint, región y credenciales: nada del código depende de un
proveedor concreto.

## 1. Crear el bucket (Supabase)

1. En Storage crea el bucket **`submissions`** como **privado** (nada público).
2. En la sección de S3 access keys genera una key dedicada a este backend y
   anota: endpoint S3 (`https://<ref>.storage.supabase.co/storage/v1/s3`),
   región (p. ej. `us-west-2`), access key id y secret.
3. No se necesitan policies públicas ni la key `service_role` para archivos:
   todo el acceso pasa por URLs firmadas de corta vida.

## 2. Mapeo de configuración

| .NET (`Storage:`) | Compose `.env` | Valor |
|---|---|---|
| `Endpoint` | `S3_ENDPOINT` | URL S3 del storage |
| `Region` | `S3_REGION` | p. ej. `us-west-2` |
| `Bucket` | `S3_BUCKET` | `submissions` |
| `AccessKey` | `S3_ACCESS_KEY_ID` | access key id (solo servidor) |
| `SecretKey` | `S3_SECRET_ACCESS_KEY` | secret (solo servidor) |
| `UploadUrlExpiryMinutes` | `STORAGE_UPLOAD_EXPIRY_MINUTES` | `15` |
| `DownloadUrlExpiryMinutes` | `STORAGE_DOWNLOAD_EXPIRY_MINUTES` | `60` |

- Desarrollo (`dotnet run`): UserSecrets `Storage:Endpoint`, etc. Comandos
  copiables (valores de ejemplo, pon los reales):
  ```powershell
  dotnet user-secrets set "Storage:Endpoint" "https://xxxx.storage.supabase.co/storage/v1/s3" --project Backend
  dotnet user-secrets set "Storage:Region" "us-west-2" --project Backend
  dotnet user-secrets set "Storage:Bucket" "submissions" --project Backend
  dotnet user-secrets set "Storage:AccessKey" "<access-key-id>" --project Backend
  dotnet user-secrets set "Storage:SecretKey" "<secret>" --project Backend
  ```
- Compose: variables del `.env` (ver `.env.example`), ya mapeadas a
  `Storage__*` en el servicio `api`.
- Producción (Render): mismas claves con `__` (`Storage__Endpoint`, …).
- Sin credenciales el arranque sigue funcionando, pero pedir un ticket
  responde 500 claro (`UnconfiguredFileStorage`).

## 2.1. Alcance mínimo y rotación de keys

- Genera la S3 key dedicada a este backend y, si el panel lo permite,
  limítala al bucket `submissions`. Nunca uses una key de otro servicio
  ni la `service_role` para archivos.
- Si una key se expone fuera del servidor (chat, logs, repo): revócala en
  el dashboard de Supabase, genera una nueva, actualiza UserSecrets /
  `.env` / variables de Render según el entorno y reinicia la API. Luego
  verifica con el §3 que los tickets vuelven a firmarse.

## 3. Verificar la conexión

Con el backend corriendo y un alumno autenticado:

1. `POST /api/activities/{id}/upload-ticket` con `fileName`, `contentType`
   y `fileSizeBytes` → responde `uploadUrl`, `storagePath` y expiración.
2. `PUT <uploadUrl>` con los bytes y el `Content-Type` declarado.
3. `POST /api/activities/{id}/submissions` con el `storagePath` → `201`.
4. El objeto debe aparecer en el bucket; `GET` de descarga usa URL firmada
   con expiración nativa S3.

Fallos típicos:

| Síntoma | Causa |
|---|---|
| 500 al pedir ticket | Falta config (ver §2) |
| Firma inválida / 403 en PUT | Región o endpoint incorrectos, `Content-Type` distinto al declarado, o reloj desviado (las firmas S3 toleran ±15 min) |
| 404 en descarga | Objeto o bucket inexistente |

## 4. E2E pendiente (asistido)

La verificación contra un proyecto real requiere endpoint, región,
bucket y credenciales S3 de prueba (pásalas al ejecutar; no se registran
en ningún reporte). Flujo: ticket → `PUT` → confirmación → descarga
firmada → limpieza de objetos de prueba.
