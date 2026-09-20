# Despliegue a producción (Render + Vercel + Supabase)

Orden: Supabase listo → backend en Render → frontend en Vercel con la URL
real (los QR y el CORS dependen de ella, y `VITE_API_URL` se hornea al compilar).

## 1. Supabase (una sola vez)

- Base: migraciones ya aplicadas (verificar con `SELECT COUNT(*) FROM "__EFMigrationsHistory"` = 4). Futuras migraciones: `dotnet ef database update --connection "<directa>"`.
- Storage: bucket privado + S3 keys dedicadas (ver `docs/almacenamiento.md`).

## 2. Backend en Render

Nuevo **Web Service** → **Docker**: repo, rama, `Dockerfile` en `Backend/`,
health check path `/health`. La imagen escucha `$PORT` vía `entrypoint.sh`
(8080 local por default).

Environment (todas con `__`, valores reales solo en el dashboard):

| Clave | Valor |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` (cierra Scalar/OpenAPI y UserSecrets) |
| `ConnectionStrings__DefaultConnection` | pooler **sesión** `:5432`, usuario `postgres.<ref>`, `SSL Mode=Require` |
| `Jwt__Secret` | ≥32 chars (generar nuevo para prod) |
| `Jwt__Issuer`, `Jwt__Audience` | `codetrack` (igual que en los tokens) |
| `Reports__ProjectName`, `Reports__Period`, `Reports__Responsible`, `Reports__Advisor` | encabezado oficial |
| `Admin__ControlNumber`, `Admin__FullName`, `Admin__Email`, `Admin__Password` | solo si el admin aún no existe (el seed lo omite si ya está) |
| `Storage__Endpoint`, `Storage__Region`, `Storage__Bucket`, `Storage__AccessKey`, `Storage__SecretKey` | S3 de Supabase |
| `Cors__AllowedOrigins__0` | https://code-track-two.vercel.app (URL pública de Vercel; sin esto el navegador bloquea el login) |
| `Qr__FrontendBaseUrl` | https://code-track-two.vercel.app (los QR apuntan aquí) |

Verificación: `GET https://<tu-api>.onrender.com/health` → 200 con
`appliedMigrations`; login del admin → 200. Primer arranque lento por
migraciones pendientes: ninguna (ya aplicadas).

## 3. Frontend en Vercel (cierre del círculo)

Ya desplegado con `VITE_API_URL` temporal. Ahora: Settings → Environment
`VITE_API_URL=https://<tu-api>.onrender.com` (+ `VITE_PERIOD`) → **Redeploy**.
Probar: login real, panel admin, descarga de reporte, registro por QR.

## 4. Notas operativas

- Free tier de Render duerme el servicio (~1 min de cold start); el primer
  request lo despierta. Plan pago si necesitas siempre-activo.
- Rollback: re-deploy de un commit anterior en Render/Vercel. Las
  migraciones solo van hacia adelante (`database update` a migración
  anterior si hay que revertir esquema).
- Logs: Render → Logs del servicio; errores 500 del backend se loguean
  con método y path (sin secretos).
- Rotar cualquier secreto expuesto fuera de su servidor (ver
  `docs/secretos.md` §rotación).
