# Guía de estilos del frontend

La segunda revisión visual sustituye la primera interfaz oscura plana por una composición con transparencias, iluminación ambiental y degradados. El cambio de dirección fue solicitado explícitamente; no modifica reglas de negocio ni contratos de API.

## UI-01: paleta y superficies — revisión 2

Tema oscuro fijo. Los tokens están en `frontend/src/styles/index.css`, integrados con Tailwind mediante `@theme`. Los colores se definen aquí, no en cada pantalla.

| Token | Valor | Uso |
| --- | --- | --- |
| canvas | #10141F | Fondo profundo |
| surface | #1E2635 | Superficie sólida y alternativa sin transparencias |
| raised | #2B364A | Controles al apuntar |
| line | #404C63 | Separadores |
| control-line | #7C8CA8 | Límites de controles |
| ink | #F4F3FB | Texto principal |
| muted | #B7C2D6 | Texto secundario |
| accent | #46EFB5 | Verde principal |
| accent-hover | #85F9D0 | Acción principal al apuntar |
| lilac | #B9ACFF | Acento violeta decorativo |
| glass | rgb(25 33 49 / 90%) | Panel translúcido legible |
| glass-edge | rgb(184 207 255 / 19%) | Borde decorativo de panel |
| success / success-soft | #72E5BB / #24483E | Éxito |
| warning / warning-soft | #FFDA88 / #4D4029 | Advertencia |
| danger / danger-soft | #FFB4BD / #512F3A | Error |

- Degradados verde/aguamarina en acciones; verde/azul/violeta en titulares. Los valores complementarios y la iluminación se centralizan en el CSS.
- El cristal usa una base oscura al 90 % para mantener la legibilidad. El desenfoque de fondo se limita a paneles destacados y navegación, con superficie legible cuando no está disponible.
- Los botones principales llevan texto oscuro. Los estados siempre incluyen texto.
- El QR conserva su fondo blanco; no se aplican efectos ni cambios de color al código.

## UI-02: composición y medidas — revisión 2

- Escala de separación: **4, 8, 12, 16, 24, 32 y 48 px**. Etiqueta/campo: 8 px; campos: 16 px; bloques: 24 px; secciones: 32 px. Cero reinicia márgenes.
- Tarjetas: 16 px, 24 px desde tablet. Paneles destacados: 24/32 px; hero de escritorio: 48 px. Las celdas de tablas usan 16 px.
- Controles: mínimo 44 px de alto; acceso: 48 px. Radios: controles 12 px, insignias de iconos 16 px, paneles 24 px. La ilustración usa formas geométricas de hasta 32 px de radio y círculos.
- Tipografía del sistema: cuerpo 16 px; auxiliar y navegación 14 px; leyendas y etiquetas editoriales 12 px; subtítulos 20 px; títulos de página 24/32 px. Titulares promocionales fluidos de 32–64 px; el 404 usa 60 px.
- Acceso: presentación del taller y formulario separados en dos columnas desde 768 px. En móvil se abrevia el contenido promocional y se oculta la ilustración, manteniendo todos los campos y acciones.
- Inicio: presentación y accesos a sesiones, actividades y progreso. La ilustración y sus números de secuencia son decorativos; no representan estadísticas ni progreso real.
- Administración: los formularios de temas, sesiones y actividades se muestran junto a los datos desde 1280 px; se apilan debajo de ese ancho.
- Perfil: identidad y datos separados visualmente; los datos reales se conservan.

Componentes: `AuthLayout`, `FormField`, `Icon`, `LearningGraphic`, `VisualEffects` y los layouts compartidos. Las clases `btn`, `field`, `card`, `glass-panel`, `table-panel`, `badge`, `alert`, `form-actions` y `workspace-grid` definen apariencias y distribuciones reutilizables. No duplicar variantes de colores o medidas en cada página.

## UI-03: adaptación y accesibilidad — revisión 2

- Contenedor máximo de 1280 px; márgenes interiores de 16/24/32 px. El panel de acceso puede alcanzar 480 px para acomodar la nueva composición.
- Navegación superior flotante desde 1280 px; menú desplegable en anchos menores. Escape cierra y devuelve el foco; navegar cierra el menú.
- Tablas y bloques de código desplazan dentro de su contenedor. No ocultar desbordamientos de toda la página para disimular errores de layout.
- Etiquetas visibles en formularios, foco visible, enlace para saltar al contenido y regiones de tabla accesibles por teclado. Los iconos decorativos se excluyen del árbol de accesibilidad; los botones de solo icono tienen nombre accesible.
- Los textos de interfaz se mantienen en react-i18next. No cargar tipografías, imágenes o bibliotecas visuales externas.
- `prefers-reduced-transparency` sustituye el cristal por superficies sólidas. En colores forzados se conserva el texto de los degradados, los límites y el foco.
- Mantener autenticación, permisos, Markdown seguro y manejo de fechas existentes.

## UI-04: movimiento y efectos

- Iluminación de fondo: desplazamiento lento de 24 segundos. Nodos de la ilustración: flotación de 8 segundos, desplazamiento máximo 8 px. Sin parpadeos.
- Interacciones: transiciones de 180–200 ms; entrada del formulario de 480 ms. Solo se desplazan controles al apuntar cuando el dispositivo admite hover.
- El control «Pausar animaciones de fondo» está en la barra de acceso y en los controles de navegación. Guarda la preferencia en `localStorage` (`codetrack-motion`); si el almacenamiento falla, funciona durante la sesión.
- `prefers-reduced-motion: reduce` desactiva todas las animaciones y transiciones, y oculta el control de pausa porque ya no hay movimiento. Esta preferencia del sistema prevalece sobre la local.
- Las capas ambientales son decorativas, no interceptan eventos ni forman parte del orden de foco. Se animan transformaciones, sin JavaScript en cada fotograma.

## Validación

Compilar backend antes del frontend, ejecutar lint y revisar los anchos de 320, 360, 768, 1024, 1280 y 1440 px. Comprobar textos largos, estados vacíos/error, tablas, formularios, menú con teclado y preferencias de accesibilidad.

En esta revisión se comprobaron 22 rutas en esos seis anchos usando Edge sin ventana y datos simulados: sin desbordamiento horizontal global ni controles visibles menores de 44 px. También se probaron Escape y foco del menú, cierre al navegar, persistencia de pausa, movimiento/transparencia reducidos y reflujo equivalente al 200 % en cinco pantallas representativas. No aparecieron excepciones JavaScript.

Contrastes calculados: texto principal/superficie 13.77:1, secundario/superficie 8.46:1, texto de botón principal 12.50:1 y borde de control/superficie elevada 3.57:1. El secundario sobre una composición conservadora más clara (#414854) conserva 5.13:1.

Las pruebas visuales con respuestas simuladas no sustituyen integración con el backend real, comprobación manual del zoom o escaneo de QR. Vite mantiene su aviso por el tamaño del paquete JavaScript principal.
