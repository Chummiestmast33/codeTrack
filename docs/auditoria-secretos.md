# Auditoría de secretos y validación documental

Fecha: 2026-09-20. Base revisada: `6fb74ef`.
Se revisaron 22 commits alcanzables después de obtener ramas remotas y etiquetas,
más los cambios documentales de este PR. El clon no es shallow.
Este informe no contiene valores de credenciales ni identificadores de despliegues.

## Método y resultado

- Patrones de tokens GitHub, claves AWS/Supabase, JWT y claves privadas en archivos versionados e historial: sin coincidencias de credenciales reales.
- Búsquedas `git log --all -p -S` para `Password=`, `Jwt:Secret`, `JWT_SECRET`, `SecretKey`, `AccessKey`, `service_role` y `postgresql://`: coincidencias revisadas como configuración, ejemplos o fixtures.
- Revisión adicional de asignaciones literales de password/secret/access key/token: fixtures, ejemplo HTTP local y etiquetas de traducción.
- `.env` y `frontend/.env`: ignorados y no versionados; sin historial de esos dos paths.
- Guías ES/EN y README: URLs personales sustituidas; enlaces relativos locales verificados. RF/RN/D permanecen intactos.
- No se identificó una credencial real que requiera rotación. No se modificaron credenciales ni se reescribió historial.

| Referencias revisadas | Clasificación |
|---|---|
| `9c235e8`: `Backend/Backend.http`, tests de Identity y wiring | Ejemplo local, contraseñas de prueba y configuración ficticia |
| `2b9ab2e`: `Backend.Tests/Infrastructure/OperationalTests.cs` | Fixture de seed admin |
| `daadaae`: `Backend.Tests/Infrastructure/S3StorageClientTests.cs`, guías y plantillas | Credenciales ficticias y referencias de configuración |
| `de44b5a`: `frontend/src/locales/es.json` | Etiquetas UI, no secretos |
| `6fb74ef`, `92a0007`, `9ca3bf0`, `4a52fc9`, `bf07846` | Normalización/conexiones, documentación y referencias de configuración; sin credenciales reales identificadas |

## Repetir comprobaciones sin imprimir valores

Desde la raíz en PowerShell. El pipeline de pickaxe consume los patches en memoria y solo
imprime conteos; no publicar patches de secretos candidatos sin sanitizarlos.

```powershell
git ls-files -- .env frontend/.env
git check-ignore .env frontend/.env
git log --all --format='%h' -- .env frontend/.env
foreach ($needle in @('Password=', 'Jwt:Secret', 'JWT_SECRET', 'SecretKey', 'AccessKey', 'service_role', 'postgresql://')) {
    $patch = @(git log --all -p -S $needle --format='COMMIT:%h')
    [pscustomobject]@{ Pattern = $needle; Commits = @($patch | Where-Object { $_ -like 'COMMIT:*' }).Count }
    Remove-Variable patch
}
```

## Validación y límites

Backend: restore/build correctos, 122 tests aprobados. Frontend: npm ci, lint y build
correctos; chunk principal de 565,68 KB, advertencia registrada en pendientes.
Se comprobó sintaxis PowerShell y correspondencia entre bloques operativos ES/EN.
Se verificaron rutas/payloads y claves contra controllers/options/Compose/factory EF.
El daemon Docker local no estaba disponible: no se ejecutaron migraciones ni quickstart E2E.
No se ejecutaron despliegues ni operaciones S3 contra cuentas privadas; los ejemplos de
proveedores se contrastaron con sus referencias oficiales enlazadas en las guías.
CI remoto se informa en el PR para el commit publicado.

El barrido es por patrones y revisión contextual: no garantiza ausencia absoluta de secretos.
Cubre historial alcanzable disponible, no commits inaccesibles, forks ajenos, logs externos,
adjuntos, paneles de proveedores ni valores privados de archivos ignorados.
Un hallazgo real posterior debe revocarse/rotarse antes de publicar; borrar texto no revoca claves.
