# AGENTS.md — Introducción a la Programación Competitiva

These instructions apply to the entire repository. Before implementing a task, read the relevant documents in `docs/`, especially `requisitos-funcionales.md`, `reglas-negocio.md`, and `decisiones-pendientes.md`. If a rule or decision changes, update its document and identifier. Do not change the scope silently.

## Project layout (current state)

- `Backend/`: single ASP.NET Core project with `Controllers/` (`Auth`, `Users`, `Admin`, topics, sessions, attendance/QR, activities, submissions, progress, reports), `Domain/` (pure entities, enums, rules), `Application/` (`Features/` Identity, Topics, Sessions, Attendance, Activities, Submissions, Progress, Reporting with commands, queries, validators plus `Abstractions/` + `Common/` pipeline), `Infrastructure/` (`Persistence/` EF Core context, configurations, repositories, `Security/` hashing + JWT, `Qr/` PNG generation, `Reporting/` CSV + PDF export), `Middleware/` (global RFC 7807 error handling incl. 410 Gone), `Migrations` under `Infrastructure/Persistence/` (incl. official-topics seed), `Program.cs`, and `Backend.csproj`. There is no `src/Api, Application, Domain, Infrastructure` split yet. File storage is a contract only (`IFileStorage` signed-URL flow + `StorageOptions` for Supabase; real client pending).
- `Backend/` dependencies (`MediatR`, `FluentValidation`, `EF Core`, `Npgsql.EntityFrameworkCore.PostgreSQL`) are installed; MediatR + FluentValidation are in use by Identity, EF Core maps all nine entities with unique constraints per RN-01 and RN-04.
- `Backend.Tests/`: xUnit test project referencing `Backend/Backend.csproj`; covers Domain risk cases.
- `frontend/`: React + Vite + Tailwind CSS v4 (`package.json`, `vite.config.js`, `index.html` at its root; source under `src/` with `api/`, `components/`, `features/`, `pages/`, `routes/`, `styles/`, `types/`). UI text lives in `src/locales/*.json` via react-i18next (`es` complete, other languages fall back to `es`); never hardcode display strings in JSX. Dev server and build run from `frontend/`.
- `docs/`: project requirements and decisions.

Current stack: ASP.NET Core 10 with Controllers-based API (not Minimal APIs); EF Core with Npgsql and PostgreSQL (local Docker for dev, Supabase as target); React 19 + Vite 8 + Tailwind CSS v4; `docker-compose.yml` with Postgres for local dev, VS-generated `Dockerfile`, `/health` still pending. Build order is backend first, frontend afterwards. Use a modular monolith with lightweight CQRS as the backend grows. If MediatR and FluentValidation are introduced, document their use and follow this file's conventions for new use cases. Do not add classes or dependencies without a concrete use case.

## Rules that must remain consistent

- **RN-01 and RN-02:** the student control number is required and unique. Trim leading and trailing whitespace and store it as text. RN-02, which required the format `year + 13 + four digits`, is canceled. Do not enforce that pattern or assume an institutional length or prefix.
- **D-01:** students register with a control number, name, email address, and password. Accounts start in pending status; an administrator approves or rejects them. Only approved and active accounts may sign in.
- **D-02:** an email address is required and must use the allowed domain. The exact domain has not been defined yet. Make it configurable and do not invent a value. For now, only an administrator resets passwords; do not implement automatic email-based recovery.
- **D-03:** resubmissions are allowed. Store every version and preserve its history; the latest version is current. Respect the activity's status and dates.
- **D-04 and RN-04:** QR attendance requires an authenticated student with an approved account. Allow only one attendance record per student and session; keep manual attendance as an alternative.
- **D-05:** automatic progress per topic uses both attendance **and** reviewed activities. With neither, the status is `No iniciado` (not started); with only one, `En proceso` (in progress); with both, `Completado` (completed). The instructor may set a manual status with a reason; it takes precedence until removed.
- **RN-05 and RN-06:** store uploaded files outside the database and reject files larger than 5 MB.
- **RN-09:** store every domain instant in UTC with offset (`DateTimeOffset` in C#, `timestamptz` in PostgreSQL). The backend decides date comparisons in UTC; the API serializes ISO-8601 with offset and the frontend renders it in the user's local time only for display.

## Implementation

- Validate inputs and permissions in the backend, even when the frontend also validates for user feedback. Protect administrator operations. Never expose passwords or secrets in responses, logs, or Git. Local secrets use UserSecrets for `dotnet` commands and `.env` for Compose (hierarchical names with `__`, e.g. `Jwt__Secret`); see `docs/entorno-pruebas.md`.
- Hash passwords securely, sanitize HTML when rendering Markdown, and validate uploaded files according to the requirements.
- Enforce uniqueness in PostgreSQL as well. A pre-insert lookup can improve error messages, but it does not replace a unique constraint or concurrent-conflict handling.
- Keep reads and writes clear; return DTOs from the API rather than domain entities. Propagate cancellation through asynchronous operations.
- Handle time per RN-09: use `DateTimeOffset.UtcNow` through an injectable `TimeProvider` (never `DateTime.Now`, `DateTime.Today`, `DateTime.ToLocalTime()`, `DateTimeKind.Local`/`Unspecified`, or `DateOnly.FromDateTime(DateTime.Now)`).
- If a feature uses MediatR, place its command or query and handler under the corresponding `Backend/Application/Features/` folder once it exists. Use FluentValidation for input rules that need it. Do not require an empty validator for every command.
- Put tests under `Backend.Tests/`. Test meaningful risk cases: approval, uniqueness and concurrency, the allowed email domain once defined, resubmissions, unique attendance, and manual progress adjustments. Run tests relevant to the change and report any limitations.

## Before finishing a task

1. Confirm that the change follows current decisions and does not reactivate RN-02.
2. Update documentation and migrations when rules or data change.
3. Check that the affected project builds and relevant tests pass once runnable projects exist.
4. Summarize what changed and how it was verified.
