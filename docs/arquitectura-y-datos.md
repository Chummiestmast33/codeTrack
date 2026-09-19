# Arquitectura y modelo de datos propuestos

Este archivo reúne **propuestas de implementación**, no decisiones adicionales aprobadas. Los requisitos vinculantes están en [requisitos funcionales](requisitos-funcionales.md), [reglas de negocio](reglas-negocio.md) y [requisitos no funcionales](requisitos-no-funcionales.md).

## Organización sugerida del backend

```text
src/
  Domain/          # Entidades y reglas
  Application/     # Casos de uso, comandos, consultas y DTO
  Infrastructure/  # EF Core, almacenamiento, QR e integraciones
  API/             # Endpoints, autenticación y middleware
```

Un monolito modular con CQRS ligero puede cubrir el alcance del taller. Ejemplos: `CreateActivity`, `RegisterAttendance`, `SubmitActivity`, `GetProgress` y `GetAttendanceReport`. Véase RNF-11.

## Entidades principales

| Entidad | Campos iniciales sugeridos | Relación principal |
| --- | --- | --- |
| `User` | `Id`, `ControlNumber` (texto), `FullName`, `Email`, `PasswordHash`, `Role`, `ApprovalStatus`, `IsActive`, `CreatedAt`, `UpdatedAt` | Alumno, instructor o administrador |
| `Topic` | `Id`, `Name`, `Description`, `OrderNumber`, `IsActive` | Sesiones y actividades |
| `Session` | `Id`, `SessionDate`, `Title`, `Description`, `Status`, `CreatedBy`, `CreatedAt` | Uno o varios temas; asistencias |
| `AttendanceRecord` | `Id`, `SessionId`, `UserId`, `Status`, `Observation`, `RegisteredBy`, `CreatedAt` | Una fila por alumno y sesión |
| `Activity` | `Id`, `Title`, `MarkdownContent`, `TopicId`, `SessionId`, `DueDate`, `Status`, `SubmissionMode`, `CreatedAt`, `UpdatedAt` | Tema, sesión opcional y entregas |
| `Submission` | `Id`, `ActivityId`, `UserId`, `Url`, `FileName`, `ContentType`, `FileSize`, `StoragePath`, `Comment`, `Status`, `VersionNumber`, `SubmittedAt`, `ReviewedAt`, `ReviewedBy` | Actividad y alumno |
| `ProgressRecord` | `Id`, `UserId`, `TopicId`, `Status`, `Percentage`, `ManualStatus`, `AdjustmentReason`, `UpdatedAt` | Alumno y tema; estado automático con posible ajuste manual |

`ControlNumber` debe tratarse como texto, ser obligatorio y tener una restricción de unicidad. El correo es obligatorio y debe pertenecer al dominio permitido configurado. `ApprovalStatus` inicia como pendiente; el administrador aprueba o rechaza la cuenta. Cada reentrega es una versión nueva de `Submission`, y se conserva el historial. No debe existir una restricción de base de datos que imponga el patrón cancelado de RN-02. Para asistencia conviene una restricción única sobre (`SessionId`, `UserId`).

Conforme a RN-09, todos los campos de fecha con hora (`CreatedAt`, `UpdatedAt`, `SessionDate`, `DueDate`, `SubmittedAt`, `ReviewedAt`) son `DateTimeOffset` en C# y `timestamptz` en PostgreSQL. Las comparaciones se hacen en UTC en el backend; la API serializa ISO-8601 con desplazamiento y el frontend lo muestra en la hora local del usuario.

## Tecnologías mencionadas en la propuesta inicial

| Capa | Opciones propuestas |
| --- | --- |
| Backend | ASP.NET Core, EF Core, Npgsql, Docker |
| Frontend | React, Vite, editor Markdown y diseño adaptable |
| Base de datos | PostgreSQL en Supabase o Neon |
| Archivos | Supabase Storage o Cloudflare R2 |
| Despliegue | Frontend en Vercel; backend Docker en Render, según decisión del proyecto |

Mantener secretos fuera de GitHub mediante variables de entorno. Los archivos de ejemplo de configuración pueden documentar nombres de variables, sin valores reales.
