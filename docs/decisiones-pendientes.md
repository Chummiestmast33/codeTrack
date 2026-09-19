# Decisiones del proyecto

Estas decisiones aplican a la versión inicial. Los identificadores D-01 a D-05 se conservan para seguimiento en GitHub.

## D-01: Registro de alumnos con aprobación

El alumno registra su cuenta con número de control, nombre, correo y contraseña. La cuenta queda **pendiente** hasta que un administrador la apruebe. Solo después de la aprobación puede iniciar sesión y utilizar las funciones para alumnos. El administrador puede revisar, aprobar o rechazar solicitudes. El número de control es obligatorio y único, sin validar el formato cancelado de RN-02.

## D-02: Correo obligatorio y restablecimiento de contraseña

Se pedirá un correo al registrarse y se validará que pertenezca al **dominio permitido**. El dominio exacto todavía está pendiente de especificar; debe poder configurarse sin dejarlo fijo en el código.

Por ahora, **el administrador restablece las contraseñas**. En el futuro se podrá incorporar un sistema de recuperación por correo. La existencia de un correo registrado no activa por sí sola esa función.

## D-03: Reentregas permitidas

El alumno puede volver a entregar una actividad cuando esta admita entregas. Cada reentrega crea una nueva versión con su fecha y contenido; las anteriores se conservan. La última versión es la vigente y es la que se usa para mostrar el estado actual, sin borrar el historial. Las condiciones de cierre y fecha límite siguen la configuración de la actividad.

## D-04: Asistencia por QR con alumno autenticado

Para registrar asistencia mediante QR, el alumno debe haber iniciado sesión con una cuenta aprobada. Si no hay sesión iniciada, se pide autenticación antes de completar el registro. Se mantiene la asistencia manual del instructor como contingencia.

## D-05: Progreso con asistencia y actividades

El progreso automático de cada tema considera **ambos elementos**: asistencia a sesiones del tema y actividades revisadas del mismo tema. Si no hay evidencia de ninguno, el estado es «No iniciado»; si hay evidencia parcial, «En proceso»; y si hay asistencia y al menos una actividad revisada, «Completado». El instructor puede ajustar manualmente el estado y registrar el motivo; el ajuste manual prevalece hasta que se retire.

## Dato pendiente

- Especificar el dominio permitido para el correo, por ejemplo la parte que sigue a `@`. Hasta definirlo no se puede configurar la validación exacta de D-02.

## Regla histórica

- **RN-02:** cancelada la validación del formato del número de control. Se conserva el identificador en [reglas de negocio](reglas-negocio.md); el número sigue siendo obligatorio y único.
