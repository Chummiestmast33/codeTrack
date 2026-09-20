# Pendientes y restricciones operativas

Este registro no modifica ni renumera RF/RN/D. El español sigue siendo normativo.
Ver [decisiones](decisiones-pendientes.md), [RF](requisitos-funcionales.md),
[guías ES](entorno-pruebas.md) y [guías EN](en/environment.md).
Estados basados en codigo; no implican verificación de despliegues privados.

| Tema | Estado y evidencia | Criterio de cierre |
|---|---|---|
| Dominio D-02 | Pendiente de decisión; `AuthOptions.AllowedEmailDomain` configurable, vacío por defecto | Definir dominio, registrar decisión conservando D-02, configurar y probar aceptación/rechazo |
| XLSX | Opcional; `ReportsController` y exportador ofrecen PDF/CSV | Aprobar alcance antes de añadir XLSX; validar formato/exportación |
| Bundle >500 KB | Optimización pendiente; build Vite advierte por tamaño del chunk principal | Medir build y dividir por rutas/dependencias sin romper navegación; justificar umbral si corresponde |
| Pooler transacción | Restricción vigente: usar session pooler 5432; 6543 no autorizado para este proyecto | Mantener guías y despliegue conformes; un cambio exige decisión y pruebas de compatibilidad |
| Mutation testing | Pendiente; CI ejecuta xUnit, no mutation testing | Elegir alcance y umbral, ejecutar sobre reglas de riesgo y documentar supervivientes |
| Seed admin por env | Implementado: `AdminSeedHostedService` crea una vez por control; configuración/verificación por entorno pendiente | Configurar cuatro campos, probar primer login y segundo arranque sin duplicados; comprobar error de seed |
| Traducción UI `en.json` | Parcial; claves ausentes caen a español | Completar paridad de claves y revisar textos/interpolaciones visualmente |
| Descarga de archivos | Adaptador S3 firma GET, pero no hay endpoint de descarga | Definir y proteger endpoint; verificar autorización y descarga E2E antes de anunciar la capacidad |
| Storage E2E | Requiere proyecto y credenciales de prueba | Ejecutar ticket - PUT - confirmación y comprobación independiente S3, dejando claro qué flujo cubre |
