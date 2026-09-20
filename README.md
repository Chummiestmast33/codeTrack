# Introducción a la Programación Competitiva

![CI](https://github.com/Chummiestmast33/codeTrack/actions/workflows/ci.yml/badge.svg)

## Carpetas del proyecto

- `Backend/`: proyecto único ASP.NET Core con `Controllers/`, `Domain/` (entidades, enums y reglas puras), `Application/` (casos de uso Identity con MediatR + FluentValidation), `Infrastructure/` (EF Core + PostgreSQL, hashing, JWT), `Middleware/` (errores RFC 7807) y `OpenApi/` (documento + esquema Bearer).
- `Backend.Tests/`: proyecto de pruebas xUnit que referencia a `Backend/`; cubre casos de riesgo del dominio, aplicación e infraestructura.
- `frontend/`: React 19 + Vite 8 + Tailwind CSS v4 (`npm run dev` / `npm run build` desde `frontend/`). Fuente en `src/` (`api/`, `components/`, `features/`, `pages/`, `routes/`, `styles/`, `types/`).
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

En producción la base será Supabase: conexión directa (puerto 5432, `SSL Mode=Require`, usuario `postgres`) para migraciones; en runtime el shared pooler en modo sesión (`:5432`, usuario `postgres.<ref>`, host copiado del dashboard, soporta prepared statements — no usar el modo transacción `:6543`).

## Orden de construcción

1. Backend primero (dominio, aplicación, infraestructura, endpoints).
2. Frontend con React + Vite + Tailwind v4 ya inicializado en `frontend/`; desarrolla las pantallas sobre las carpetas `src/` existentes.
3. Conecta el frontend con el backend y desarrolla primero usuarios, temas, sesiones y asistencia; continúa con actividades, progreso y reportes según [`docs/requisitos-funcionales.md`](docs/requisitos-funcionales.md).
4. Antes de validar el número de control, consulta [`docs/reglas-negocio.md`](docs/reglas-negocio.md): RN-02 está cancelada. El número sigue siendo obligatorio y único.
