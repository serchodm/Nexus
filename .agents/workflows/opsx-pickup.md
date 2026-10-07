---
description: Recoge la tarea pendiente del buzón del agente, audita la Definition of Ready (DoR), implementa el código y valida la Definition of Done (DoD).
---

# Workflow: `/opsx-pickup`

Este comando es ejecutado por cualquier agente especialista para procesar el ticket asignado en su buzón bajo salvaguardas de calidad ágil.

## Pasos de Ejecución:

1. **Localizar Buzón**:
   - Inspecciona tu carpeta `.agent-inbox/{mi-nombre-de-agente}/`.
   - Busca el ticket pendiente (`<feature-id>.md`). Si existen varios, toma el de menor numeración.

2. **🛑 Paso 0: Auditoría Obligatoria de Definition of Ready (DoR)**:
   - Antes de modificar o crear un solo archivo, evalúa el ticket asignado:
     - [ ] ¿Están definidos los archivos exactos a modificar o crear?
     - [ ] ¿Están detallados los campos, entidades, contratos o firmas sin ambigüedad?
     - [ ] ¿Pertenece 100% a mi ámbito permitido de trabajo?
     - [ ] ¿Está claro el comando de validación para certificar la DoD?
   - **SI CUALQUIERA DE ESTAS RESPUESTAS ES "NO"**:
     1. 🛑 DETENTE DE INMEDIATO. Queda expresamente prohibido escribir código o adivinar requerimientos.
     2. Crea el archivo `.agent-inbox/{mi-nombre-de-agente}/BLOCKED_DOR_{feature-id}.md` detallando exactamente qué información técnica o delimitación falta.
     3. Finaliza tu ejecución notificando el rechazo por DoR para que `@jarvis` refine la tarea.

3. **Procesar Orden e Implementación**:
   - Si la DoR es válida, implementa los cambios exclusivamente dentro de tu **Ámbito Permitido** y respetando las reglas técnicas de tu prompt.

4. **Validación Automática de Definition of Done (DoD)**:
   - Ejecuta el comando de validación asignado a tu rol (ej. `dotnet build`, `npm run build`, `dotnet test`, etc.).
   - Si se produce un error de compilación, linter o tests:
     - Corrige el código de inmediato de forma autónoma (hasta 3 intentos).

5. **Comprobación de Incongruencias / Impedimentos**:
   - Si detectas una contradicción entre specs, una imposibilidad arquitectónica o el error persiste tras 3 intentos:
     - Crea `.agent-inbox/{mi-nombre-de-agente}/BLOCKED_{feature-id}.md` con:
       - Descripción del conflicto o fallo.
       - Archivo donde se genera.
       - Propuesta de solución para el arquitecto.
     - Finaliza tu ejecución notificando el bloqueo.

6. **Cierre de Tarea Exitoso**:
   - Si la validación es 100% exitosa (DoD cumplida):
     - Actualiza el archivo `specs/features/<feature-id>/03-tasks.md` marcando con `[x]` las tareas completadas.
     - Elimina tu ticket `.agent-inbox/{mi-nombre-de-agente}/<feature-id>.md`.
     - Reporta éxito a `@jarvis`.
