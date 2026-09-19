# Reglas de negocio

Proyecto: **Introducción a la Programación Competitiva** · Periodo: **AGO-DIC 2026**.

## RN-01: Número de control único

Cada número de control se asigna a una sola cuenta. Antes de guardarlo o comprobar duplicados, se quitan los espacios exteriores. El sistema debe impedir duplicados también en la base de datos.

## RN-02: Formato de número de control — cancelada

**Estado:** cancelada para esta versión. Se conserva el identificador RN-02 como registro histórico y para que los enlaces o issues de GitHub sigan teniendo una referencia estable.

No se implementará la regla propuesta de `2 dígitos de año + 13 + 4 dígitos`, ni su expresión regular. El número de control **sí es obligatorio** para registrar al alumno e iniciar sesión. Se rechaza cuando falte o quede vacío después de quitar espacios exteriores; se aplica RN-01 para evitar duplicados. No se exige longitud, prefijo, año ni solo dígitos. Se almacena como texto para conservar ceros iniciales. Si se define un formato en el futuro, se aprobará como un cambio nuevo.

## RN-03: No se valida semestre

No se solicitará ni validará semestre.

## RN-04: Asistencia única por sesión

Un alumno puede tener un solo registro de asistencia por sesión.

## RN-05: Archivos fuera de base de datos

Los archivos entregados se guardarán fuera de la base de datos. En esta se registrarán nombre original, tipo, tamaño, ruta de almacenamiento y fecha de subida.

## RN-06: Límite de archivos

Cada archivo subido tendrá un tamaño máximo de **5 MB**.

## RN-07: Entrega genérica

La entrega de una actividad puede contener una URL, un archivo o ambos, de acuerdo con la configuración de la actividad. La lógica se reutiliza para las distintas actividades.

## RN-08: Reportes oficiales

Los reportes incluirán el nombre del proyecto, periodo AGO-DIC 2026, responsable y estudiante asesor.

## RN-09: Tiempo canónico en UTC

Todos los instantes de dominio (entregas, fechas límite, sesiones, asistencia, expiración de QR y auditoría de RF-24) se guardan en UTC con desplazamiento. En C# se usa `DateTimeOffset` y en PostgreSQL `timestamptz`. Las comparaciones de negocio (entrega antes o después de la fecha límite, QR expirado, estado de actividad) las decide el backend en UTC; el reloj del cliente nunca es autoridad.

La API serializa ISO-8601 con desplazamiento y el frontend lo convierte a la hora local del usuario solo para mostrarlo. No se persiste hora local sin desplazamiento ni `DateTimeKind.Local` o `Unspecified`.
