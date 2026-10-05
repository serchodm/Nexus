---
description: Recoge la tarea pendiente del buzón del agente, implementa el código y valida la compilación.
---

# Workflow: `/opsx-pickup`

Este comando es ejecutado por cualquier agente especialista para procesar el ticket asignado en su buzón.

## Pasos de Ejecución:
1. **Localizar Buzón**:
   - Inspecciona tu carpeta `.agent-inbox/{mi-nombre-de-agente}/`.
   - Busca el ticket pendiente (`<feature-id>.md`). Si existen varios, toma el de menor numeración.
2. **Procesar Orden**:
   - Lee las tareas y el fragmento técnico de `02-tech-spec.md`.
   - Implementa los cambios exclusivamente dentro de tu **Ámbito Permitido**.
3. **Validación Automática**:
   - Ejecuta el comando de validación asignado a tu rol (ej. `dotnet build`, `npm run build`, `dotnet test`, etc.).
   - Si se produce un error de compilación o linter:
     - Corrige el código de inmediato de forma autónoma (hasta 3 intentos).
4. **Comprobación de Incongruencias**:
   - Si detectas una contradicción entre specs, una imposibilidad arquitectónica o el error persiste tras 3 intentos:
     - Crea `.agent-inbox/{mi-nombre-de-agente}/BLOCKED_{feature-id}.md` con:
       - Descripción del conflicto.
       - Archivo donde se genera.
       - Propuesta de solución para el arquitecto.
     - Finaliza tu ejecución notificando el bloqueo.
5. **Cierre de Tarea Exitoso**:
   - Si la validación es 100% exitosa:
     - Actualiza el archivo `specs/features/<feature-id>/03-tasks.md` marcando con `[x]` las tareas completadas.
     - Elimina tu ticket `.agent-inbox/{mi-nombre-de-agente}/<feature-id>.md`.
     - Reporta éxito a `@jarvis`.
