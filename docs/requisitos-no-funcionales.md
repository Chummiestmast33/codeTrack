# Requisitos no funcionales

Proyecto: **Introducción a la Programación Competitiva** · Periodo: **\<periodo>** (configurable por despliegue).

Las metas marcadas como sugeridas son objetivos de planificación, sujetos a la infraestructura elegida.

Los requisitos no funcionales describen **cómo debe comportarse el sistema**.

## RNF-01: Seguridad de acceso

Solo usuarios autorizados deben poder acceder al sistema.

Detalles:

- Contraseñas con hash seguro.
- Autenticación por token o cookie segura.
- Autorización por rol.
- Protección de endpoints según rol.

Recomendación técnica:

```text
ASP.NET Core Identity
o autenticación JWT simple
```

## RNF-02: Privacidad y minimización de datos

El sistema debe pedir solo datos necesarios.

Datos requeridos para el registro:

```text
Número de control
Nombre completo
Correo del dominio permitido
Contraseña
```

No se solicitará:

```text
Semestre
Teléfono
Dirección
```

## RNF-03: Validación de entradas

Toda entrada debe validarse en backend.

Ejemplos:

- Número de control obligatorio y único, sin validar formato institucional (RN-01 y RN-02).
- Correo obligatorio con dominio permitido configurable; el dominio exacto está pendiente (D-02).
- URL válida.
- Archivo dentro del límite.
- Markdown sanitizado.
- IDs existentes.

No confiar solo en validaciones del frontend.

## RNF-04: Rendimiento

Las pantallas principales deben cargar rápido.

Objetivos sugeridos:

| Acción | Objetivo |
|---|---|
| Cargar listas pequeñas | Menos de 1 segundo |
| Registrar asistencia | Menos de 2 segundos |
| Subir archivo menor a 5 MB | Menos de 10 segundos con conexión normal |
| Generar reporte simple | Menos de 15 segundos |

Para el QR de asistencia:

```text
El sistema debería soportar varios registros de asistencia en poco tiempo.
```

Meta sugerida:

```text
50 alumnos registrando asistencia en menos de 3 minutos.
```

## RNF-05: Disponibilidad

El sistema debe estar disponible durante el semestre del taller.

Objetivo sugerido:

```text
99% de disponibilidad durante horario del taller
```

Pero hay que considerar limitaciones de servicios gratuitos como Render o Supabase.

Mitigación:

- Tener reportes exportables.
- Tener modo manual de asistencia.
- Poder generar evidencia aunque falle la plataforma temporalmente.

## RNF-06: Usabilidad

El sistema debe ser fácil de usar para alumnos y para el instructor.

Características:

- Interfaz en español.
- Navegación simple.
- Mensajes de error claros.
- Botones visibles para acciones frecuentes.
- Diseño responsive para celulares.

Importante porque el QR probablemente se usará con celulares.

## RNF-07: Escalabilidad

El sistema debe soportar crecimiento del taller.

Meta inicial sugerida:

```text
Hasta 100 alumnos registrados
```

Aunque el grupo real sea menor, esto deja margen.

## RNF-08: Almacenamiento externo de archivos

Los archivos deben guardarse en un almacenamiento tipo BLOB/Object Storage.

Opciones:

```text
Supabase Storage
Cloudflare R2
```

La base de datos solo guarda referencias.

## RNF-09: Límites de archivos

El sistema debe rechazar archivos mayores a:

```text
5 MB
```

También debe validar extensiones permitidas.

## RNF-10: Compatibilidad web

El sistema debe funcionar en navegadores modernos.

Ejemplos:

- Chrome.
- Edge.
- Firefox.
- Safari.
- Navegador móvil.

No se requiere aplicación móvil nativa en la primera versión.

## RNF-11: Arquitectura cercana a CQRS y DDD

El backend debe tener una arquitectura limpia o modular, cercana a CQRS y DDD, sin sobreingeniería extrema.

Recomendación:

```text
Modular Monolith
```

Separación sugerida:

```text
Domain
Application
Infrastructure
API
```

Ejemplo:

```text
Domain/
    Entities/
    ValueObjects/
    Rules/

Application/
    Commands/
    Queries/
    DTOs/
    Interfaces/

Infrastructure/
    Data/
    Storage/
    QrGenerator/
    ExternalServices/

API/
    Endpoints/
    Middleware/
    Auth/
```

CQRS ligero:

```text
Commands: CreateActivity, RegisterAttendance, SubmitActivity
Queries: GetProgress, GetAttendanceReport, GetActivityById
```

No es necesario usar microservicios.

## RNF-12: Uso de Entity Framework

El backend debe usar Entity Framework Core.

Recomendación:

```text
EF Core + Npgsql
```

Base de datos:

```text
PostgreSQL
```

Debe incluir:

- Migraciones.
- DbContext.
- Entidades mapeadas.
- Configuración por fluent API o data annotations.

## RNF-13: Containerización con Docker

Cada componente debe ser containerizable.

Para la primera versión:

```text
El backend debe correr en un contenedor Docker.
```

El backend debe incluir:

```text
Dockerfile
```

Idealmente también:

```text
docker-compose.yml
```

Aunque en producción uses Supabase como base de datos y storage.

El contenedor debe:

- Recibir configuración por variables de entorno.
- Exponer endpoint de salud.
- Ser construible desde GitHub.

Ejemplo de endpoint:

```text
GET /health
```

## RNF-14: Configuración por variables de entorno

Toda configuración sensible debe ir en variables de entorno.

Ejemplos:

```text
ConnectionStrings__DefaultConnection
SUPABASE_URL
SUPABASE_SERVICE_ROLE_KEY
SUPABASE_BUCKET
FrontendUrl
JwtSecret
```

Nunca deben ir en el código fuente.

## RNF-15: Observabilidad básica

El sistema debe registrar eventos importantes.

Ejemplos:

- Errores no controlados.
- Fallos de autenticación.
- Problemas al subir archivos.
- Problemas al generar reportes.

Para MVP no necesitas algo complejo, basta con logs estructurados.

## RNF-16: Seguridad en renderizado Markdown

Como usarás Markdown, el sistema debe evitar XSS.

Recomendación:

```text
Guardar Markdown como texto.
Renderizar en frontend o backend con librería segura.
Sanitizar HTML si se permite HTML embebido.
```

## RNF-17: Seguridad en archivos subidos

El sistema no debe ejecutar archivos subidos.

Recomendaciones:

- Validar extensión.
- Validar tamaño.
- Guardar con nombre único.
- No servir archivos como ejecutables.
- Preferir bucket privado con URLs firmadas.

## RNF-18: Backups y exportación

El sistema debe permitir exportar información importante.

Mínimo:

- Lista de alumnos.
- Asistencia.
- Actividades.
- Entregas.
- Progreso.

Aunque la base de datos esté en Supabase, conviene poder descargar reportes.

---
