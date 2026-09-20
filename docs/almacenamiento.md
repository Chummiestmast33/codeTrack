# Almacenamiento de archivos (Supabase Storage)

Las entregas con archivo usan subida directa: el backend firma una URL,
el cliente sube los bytes a Supabase y luego confirma la entrega con la
ruta (`StoragePath`). La base de datos guarda solo metadata (RN-05) y el
backend nunca toca los bytes (RNF-08, RNF-17).

## 1. Crear el proyecto y el bucket

1. Crea el proyecto en Supabase y anota su URL (`https://xxxx.supabase.co`).
2. En Storage crea el bucket **`submissions`** como **privado** (nada público).
3. En Project Settings → API copia la **`service_role`** key. Es la única
   que usa el backend. **Jamás** uses la `anon` key en el servidor y nunca
   la expongas al frontend: la service key opera sobre RLS y el bucket
   queda cerrado al público.

## 2. Mapeo de configuración

| .NET (`Storage:`) | Compose `.env` | Valor |
|---|---|---|
| `Endpoint` | `SUPABASE_URL` | `https://xxxx.supabase.co` |
| `SecretKey` | `SUPABASE_SERVICE_KEY` | service_role (solo servidor) |
| `Bucket` | `SUPABASE_BUCKET` | `submissions` |
| `UploadUrlExpiryMinutes` | `STORAGE_UPLOAD_EXPIRY_MINUTES` | `15` |
| `DownloadUrlExpiryMinutes` | `STORAGE_DOWNLOAD_EXPIRY_MINUTES` | `60` |

- Desarrollo (`dotnet run`): UserSecrets `Storage:Endpoint`, etc.
- Compose: variables del `.env` (ver `.env.example`).
- Producción (Render): mismas claves con `__` (`Storage__Endpoint`, …).
- Sin `Endpoint` o `SecretKey` el arranque sigue funcionando, pero pedir
  un ticket responde 500 claro (`UnconfiguredFileStorage`).

## 3. Verificar la conexión

Con el backend corriendo y un alumno autenticado:

1. `POST /api/activities/{id}/upload-ticket` con `fileName`, `contentType`
   y `fileSizeBytes` → responde `uploadUrl`, `storagePath` y expiración.
2. `PUT <uploadUrl>` con los bytes y `Content-Type` declarado.
3. `POST /api/activities/{id}/submissions` con el `storagePath` → `201`.
4. El objeto debe aparecer en el bucket del dashboard.

Fallos típicos:

| Síntoma | Causa |
|---|---|
| 500 al pedir ticket | Falta config (ver §2) |
| 401 firmando/descargando | Key `anon` en vez de service_role, o bucket con otro nombre |
| 404 en descarga | Objeto o bucket inexistente |
| PUT rechazado | `Content-Type` distinto al declarado en el ticket |

## 4. E2E pendiente (asistido)

La verificación contra un proyecto real requiere URL + service key de
prueba (pásalas por aquí al ejecutar; no se registran en ningún reporte).
Flujo: ticket → `PUT` → confirmación → descarga firmada → limpieza de
objetos de prueba.
