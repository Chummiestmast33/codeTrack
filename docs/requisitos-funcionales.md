# Requisitos funcionales

Proyecto: **Introducción a la Programación Competitiva** · Periodo: **AGO-DIC 2026**.

Los identificadores RF-01 a RF-24 se conservan para seguimiento en GitHub. Las alternativas que aparezcan como sugerencias requieren una decisión antes de implementarse.

Los requisitos funcionales describen **lo que el sistema debe hacer**.

## Módulo de usuarios y acceso

### RF-01: Registro de cuenta de alumno

El sistema debe permitir que un alumno cree una cuenta.

Datos mínimos:

- Número de control.
- Nombre completo.
- Correo del dominio permitido (dominio exacto pendiente de definir).
- Contraseña.

No se solicitará semestre.

**Número de control:** dato obligatorio y único por cuenta. No se exige un formato institucional fijo (véanse RN-01 y RN-02). Al terminar el registro, la cuenta queda pendiente de aprobación administrativa (D-01).

Criterio de aceptación:

- Si falta el número de control o está vacío después de quitar espacios al principio y al final, se rechaza el registro con un mensaje claro.
- Si el número de control ya está asignado a otra cuenta, se rechaza el registro.
- El registro no se rechaza por no coincidir con un año, un prefijo de escuela, una longitud de ocho caracteres o una expresión regular institucional.
- Se rechaza un correo que no pertenezca al dominio permitido configurado (D-02).
- Si los datos son válidos, la solicitud queda pendiente; el alumno no puede iniciar sesión hasta que el administrador la apruebe.

### RF-02: Inicio de sesión

El sistema debe permitir iniciar sesión usando:

- Número de control.
- Contraseña.

Criterio de aceptación:

- Si las credenciales son correctas y la cuenta está aprobada y activa, el usuario entra al sistema.
- Si la cuenta está pendiente o fue rechazada, se informa que no tiene acceso.
- Si son incorrectas, se muestra un mensaje de error.
- Las contraseñas no deben guardarse en texto plano.

### RF-03: Roles de usuario

El sistema debe manejar al menos dos roles:

| Rol | Descripción |
|---|---|
| Administrador / Instructor | Gestiona temas, sesiones, asistencia, actividades, reportes |
| Estudiante | Consulta su progreso, sube entregas y registra asistencia |

Los alumnos del taller tendrán rol de estudiante. El responsable puede recibir reportes exportados sin requerir una cuenta; la asignación de cuentas administrativas se define al implementarse.

### RF-04: Gestión básica de usuarios

El administrador debe poder:

- Ver lista de usuarios registrados.
- Revisar solicitudes de registro y aprobarlas o rechazarlas.
- Activar o desactivar usuarios.
- Editar nombre completo.
- Restablecer contraseña en caso necesario.

No es obligatorio eliminar físicamente usuarios; se recomienda borrado lógico.

## Módulo de temas

### RF-05: Registro de temas impartidos

El sistema debe permitir registrar los temas del taller.

Temas oficiales según la solicitud:

1. Fundamentos de programación: entrada, salida, condicionales, ciclos y funciones.
2. Estructuras de datos básicas.
3. Técnicas algorítmicas iniciales.
4. Recursión y backtracking.
5. Optimización y complejidad, Big O.
6. Preparación para competencias y entrevistas técnicas.

Criterio de aceptación:

- El sistema debe permitir crear, editar, ordenar y desactivar temas.
- Debe existir una carga inicial con los seis temas oficiales del taller.

## Módulo de sesiones

### RF-06: Registro de sesiones

El sistema debe permitir registrar sesiones del taller.

Cada sesión debe incluir:

- Fecha.
- Título.
- Descripción opcional.
- Tema o temas relacionados.
- Estado: planeada, impartida, cancelada.

Criterio de aceptación:

- El instructor puede crear una sesión.
- Puede marcarla como impartida.
- Puede asociarla a uno o varios temas.

## Módulo de asistencia

### RF-07: Registro manual de asistencia

El sistema debe permitir registrar asistencia de forma manual.

Estados posibles:

- Presente.
- Falta.
- Retardo.
- Justificado.

Criterio de aceptación:

- El instructor puede marcar asistencia por alumno en una sesión.
- No puede haber más de un registro de asistencia para el mismo alumno en la misma sesión.

### RF-08: Generación de QR para asistencia

El sistema debe generar un código QR para registrar asistencia en una sesión.

El QR debe estar asociado a:

- Una sesión específica.
- Un token o código único.
- Una URL de registro de asistencia.

Criterio de aceptación:

- Cada sesión puede tener un QR único.
- El QR puede regenerarse si es necesario.
- El QR puede expirar al finalizar la sesión o después de un tiempo configurable.

### RF-09: Descarga del QR como imagen

El sistema debe permitir descargar el QR como imagen.

Formato sugerido:

```text
PNG
```

Resolución mínima sugerida:

```text
512 x 512 px
```

Criterio de aceptación:

- El instructor puede descargar el QR.
- El QR descargado puede proyectarse o imprimirse.
- La imagen debe ser legible para escanearse con un celular.

### RF-10: Registro de asistencia mediante QR

El sistema debe permitir que un alumno registrado marque asistencia escaneando el QR.

Flujo:

```text
Alumno escanea QR
→ Se abre URL de asistencia
→ El alumno inicia sesión si no lo está
→ El sistema registra asistencia
```

Criterio de aceptación:

- Un alumno autenticado con cuenta aprobada puede registrar su asistencia una sola vez por sesión (D-04).
- Si ya registró asistencia, el sistema no debe duplicarla.
- Si el QR expiró, se muestra un mensaje claro.

### RF-11: Modo contingencia sin internet

Como puede haber alumnos sin internet o fallas de red, el sistema debe contemplar una alternativa.

Opciones recomendadas:

1. El instructor registra asistencia manualmente.
2. El QR se descarga e imprime como evidencia visual.
3. Se usa un código corto de sesión que el alumno pueda escribir si tiene acceso más tarde.

Criterio de aceptación:

- Si los alumnos no tienen internet, el instructor puede tomar asistencia manualmente.
- La asistencia registrada manualmente queda guardada igual que la asistencia por QR.
- El sistema no depende exclusivamente del QR para funcionar.

## Módulo de actividades

### RF-12: Creación de actividades

El sistema debe permitir al instructor crear actividades.

Cada actividad debe incluir:

- Título.
- Descripción en Markdown.
- Tema relacionado.
- Sesión relacionada, opcional.
- Fecha límite opcional.
- Estado: borrador, publicada, cerrada.
- Tipo de entrega permitido.

### RF-13: Editor Markdown para actividades

El sistema debe incluir un editor Markdown para escribir la descripción de la actividad.

El editor debe permitir al menos:

- Títulos.
- Negritas.
- Cursivas.
- Listas.
- Bloques de código.
- Enlaces.
- Vista previa.

Criterio de aceptación:

- El instructor puede escribir la actividad en Markdown.
- El sistema muestra una vista previa.
- El contenido se guarda como texto Markdown en base de datos.
- Al renderizar Markdown, el sistema debe sanitizar el HTML para evitar XSS.

### RF-14: Configuración de entrega de actividad

Cada actividad debe poder indicar qué tipo de entrega acepta.

Opciones sugeridas:

| Modo | Descripción |
|---|---|
| Solo URL | El alumno entrega una liga |
| Solo archivo | El alumno sube un archivo |
| URL o archivo | El alumno puede entregar cualquiera de los dos |
| URL y archivo | El alumno debe entregar ambos |

Para tu caso, una opción genérica simple sería:

```text
URL opcional
Archivo opcional
Pero al menos uno de los dos obligatorio
```

### RF-15: Entrega de actividades por alumnos

El sistema debe permitir que un alumno entregue una actividad.

La entrega puede incluir:

- Una URL.
- Un archivo.
- Comentario opcional.

Ejemplos de URL válidas:

```text
https://github.com/usuario/repo
https://codeforces.com/...
https://docs.google.com/...
```

Criterio de aceptación:

- El alumno puede entregar una actividad publicada.
- El sistema guarda fecha y hora de entrega.
- El sistema debe indicar si la entrega fue antes o después de la fecha límite.
- Si la actividad admite entregas, el alumno puede enviar una nueva versión sin eliminar las anteriores (D-03).

### RF-16: Validación de archivos entregados

El sistema debe validar archivos subidos.

Límites sugeridos:

- Tamaño máximo: 5 MB.
- Extensiones permitidas para código:

```text
.cpp
.c
.h
.py
.java
.txt
.md
```

Extensiones permitidas para evidencia:

```text
.png
.jpg
.jpeg
.webp
.pdf
```

Extensiones bloqueadas:

```text
.exe
.dll
.bat
.sh
.out
.msi
```

Criterio de aceptación:

- Si el archivo supera 5 MB, se rechaza.
- Si la extensión no está permitida, se rechaza.
- El sistema guarda el archivo en almacenamiento externo, no en base de datos.

### RF-17: Estados de entrega

El sistema debe manejar estados para las entregas.

Estados sugeridos:

```text
Pendiente
Entregado
Revisado
Incompleto
```

Criterio de aceptación:

- El instructor puede cambiar el estado de una entrega.
- El alumno puede ver el estado de la versión vigente de su entrega y consultar sus versiones anteriores.

### RF-18: Historial de entregas

El sistema debe mantener historial de entregas.

Cada entrega y reentrega debe guardarse como una versión con:

- Número o identificador de versión y fecha de entrega.
- URL entregada y/o ruta del archivo en storage.
- Estado, comentario del alumno y observación del instructor.

Las versiones anteriores no se sobrescriben. La última versión es la vigente (D-03).

## Módulo de progreso

### RF-19: Mostrar progreso por tema

El sistema debe mostrar el progreso de cada alumno por tema.

Estados sugeridos:

```text
No iniciado
En proceso
Completado
```

Criterio de aceptación:

- El instructor puede ver el progreso de todos los alumnos.
- Cada alumno puede ver solo su propio progreso.
- El progreso se muestra por tema oficial del taller.

### RF-20: Cálculo de progreso

El sistema debe calcular el progreso por tema usando reglas simples.

Regla definida en D-05:

| Evidencia del alumno en el tema | Estado automático |
|---|---|
| Sin asistencia ni actividades revisadas | No iniciado |
| Asistencia o actividad revisada, pero aún falta el otro elemento | En proceso |
| Asistencia y al menos una actividad revisada | Completado |

El instructor puede establecer un estado manual y dejar un motivo. Ese estado prevalece hasta que se retire el ajuste.

Criterio de aceptación:

- El sistema calcula automáticamente el progreso.
- El instructor puede modificar manualmente el estado, guardar el motivo y volver al cálculo automático.

## Módulo de reportes

### RF-21: Generar lista de asistencia

El sistema debe generar una lista de asistencia.

La lista debe incluir:

- Nombre del proyecto.
- Periodo escolar.
- Responsable del proyecto.
- Estudiante asesor.
- Lista de alumnos.
- Asistencia por sesión.
- Porcentaje de asistencia.

Ejemplo de encabezado:

```text
Proyecto: Introducción a la Programación Competitiva
Periodo: AGO-DIC 2026
Responsable: Ing. Armando López Cisenna
Estudiante asesor: Pablo Cortez Rodríguez
```

### RF-22: Generar tabla de progreso

El sistema debe generar una tabla de progreso por tema.

Columnas sugeridas:

```text
Alumno
Número de control
Tema 1
Tema 2
Tema 3
Tema 4
Tema 5
Tema 6
Progreso general
```

Esto cumple directamente con la evidencia solicitada:

> Lista de asistencia y tabla de progreso de los temas.

### RF-23: Exportación de reportes

El sistema debe permitir exportar reportes.

Formatos recomendados:

```text
PDF
CSV
XLSX
```

Prioridad para MVP:

```text
PDF y CSV
```

Criterio de aceptación:

- El instructor puede descargar la lista de asistencia.
- El instructor puede descargar la tabla de progreso.
- Los reportes incluyen el encabezado oficial del proyecto.

### RF-24: Auditoría básica

El sistema debe registrar información básica de auditoría.

Por ejemplo:

- Fecha de creación.
- Fecha de actualización.
- Usuario que creó.
- Usuario que actualizó.

Aplica para entidades importantes:

- Usuarios.
- Sesiones.
- Actividades.
- Entregas.
- Asistencias.

---
