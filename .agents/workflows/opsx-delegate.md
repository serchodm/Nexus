---
description: Delega una tarea específica a un agente especialista escribiendo en su buzón de .agent-inbox/
---

# Workflow: `/opsx-delegate`

Este comando es utilizado por `@jarvis` para preparar y depositar una orden de trabajo en el buzón de un agente especialista.

## Parámetros Esperados
- `$ARGUMENTS`: `<agente> <feature-id>` (ej. `/opsx-delegate back-domain 001-user-profile-management`)

## Pasos de Ejecución:
1. **Validar existencia de la feature**:
   - Comprueba que existe `specs/features/<feature-id>/02-tech-spec.md` y `03-tasks.md`.
2. **Filtrar tareas del especialista**:
   - Identifica en `03-tasks.md` las tareas no completadas (`- [ ]`) que pertenecen al ámbito del especialista indicado:
     - `back-domain`: Tareas de Domain y Application (Entidades, CQRS, Validadores).
     - `back-infra`: Tareas de EF Core, PostgreSQL, Repositorios, Migraciones.
     - `back-api`: Tareas de FastEndpoints, DTOs REPR, Swagger.
     - `back-test`: Tareas de tests xUnit y pruebas de integración backend.
     - `front-logic`: Tareas de DTOs TypeScript, TanStack Query hooks, Zustand stores.
     - `front-ui`: Tareas de componentes React 19, maquetación Tailwind, vistas.
     - `front-test`: Tareas de pruebas Vitest, Testing Library y linter.
3. **Generar Ticket en Buzón**:
   - Escribe el archivo `.agent-inbox/<agente>/<feature-id>.md` con el siguiente contenido:

```markdown
# Ticket de Trabajo: <feature-id>
- **Agente Asignado**: <agente>
- **Fecha de Asignación**: {CURRENT_DATE}

## Tareas a Resolver (extraídas de 03-tasks.md)
- [ ] Tarea 1 ...
- [ ] Tarea 2 ...

## Contexto Técnico Específico (extraído de 02-tech-spec.md)
{Fragmento relevante de la especificación técnica para esta capa}

## Comandos de Validación Requeridos
{Comando específico de validación de este agente}
```

4. **Confirmación**:
   - Informa al `@jarvis` que el buzón está listo para ser recogido por el especialista.
