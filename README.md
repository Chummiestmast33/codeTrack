# Introducción a la Programación Competitiva

Taller AGO-DIC 2026. Responsable: Ing. Armando López Cisenna. Estudiante asesor: Pablo Cortez Rodríguez.

## Carpetas del proyecto

- `Backend/`: proyecto único ASP.NET Core con `Controllers/`, `Domain/` (entidades, enums y reglas puras), `Application/` (casos de uso Identity con MediatR + FluentValidation), `Infrastructure/` (EF Core + PostgreSQL, hashing, JWT), `Middleware/` (errores RFC 7807) y `OpenApi/` (documento + esquema Bearer).
- `Backend.Tests/`: proyecto de pruebas xUnit que referencia a `Backend/`; cubre casos de riesgo del dominio, aplicación e infraestructura.
- `frontend/src/`: estructura vacía (`api/`, `components/`, `features/`, `pages/`, `routes/`, `styles/`, `types/` con `.gitkeep`). El scaffold React + Vite + Tailwind está pendiente y se construirá después del backend.
- `docs/`: requisitos, reglas de negocio, decisiones, arquitectura y guía del entorno de pruebas.
- `docker-compose.yml`: Postgres local para desarrollo; `.env.example` con las variables necesarias (nunca commitear `.env`).

## Primeros pasos

1. Desarrollar primero el backend en `Backend/` (modelo, EF Core + PostgreSQL, autenticación, endpoints Controllers). La división en `src/Api, Application, Domain, Infrastructure` y los proyectos en `backend/tests/` se harán como evolución cuando haya casos de uso reales.
2. Después inicializar React con Vite dentro de `frontend/` conservando las carpetas `src/` actuales. Si el generador crea un `src/` propio, integrar sus archivos en estas carpetas.

## Desarrollo local y pruebas

Guía completa en [`docs/entorno-pruebas.md`](docs/entorno-pruebas.md): levantar
Postgres, aplicar migraciones, correr la API y probar los endpoints con
Scalar (`/scalar/v1`, solo Development). Secretos locales con UserSecrets
(`dotnet run`) y `.env` (solo Compose); ver la guía para el formato de nombres.

En producción la base será Supabase: usa su conexión directa (puerto 5432, `SSL Mode=Require`) para migraciones; el pooler (6543) puede usarse en runtime.

## Orden de construcción

1. Backend primero (dominio, aplicación, infraestructura, endpoints).
2. Después inicializar React con Vite dentro de `frontend/` conservando las carpetas `src/` actuales. Si el generador crea un `src/` propio, integrar sus archivos en estas carpetas.
3. Conecta el frontend con el backend y desarrolla primero usuarios, temas, sesiones y asistencia; continúa con actividades, progreso y reportes según [`docs/requisitos-funcionales.md`](docs/requisitos-funcionales.md).
4. Antes de validar el número de control, consulta [`docs/reglas-negocio.md`](docs/reglas-negocio.md): RN-02 está cancelada. El número sigue siendo obligatorio y único.
