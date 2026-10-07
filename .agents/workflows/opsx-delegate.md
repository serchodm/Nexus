---
description: Delega una tarea específica a un agente especialista escribiendo en su buzón de .agent-inbox/ cumpliendo con Definition of Ready (DoR).
---

# Workflow: `/opsx-delegate`

Este comando es utilizado por `@jarvis` (Tech Lead & Scrum Master) para preparar y depositar una orden de trabajo en el buzón de un agente especialista.

## Parámetros Esperados
- `$ARGUMENTS`: `<agente> <feature-id>` (ej. `/opsx-delegate back-domain 001-user-profile-management`)

## Pasos de Ejecución:

1. **Validar existencia de especificaciones**:
   - Comprueba que existen `specs/features/<feature-id>/01-spec.md`, `02-tech-spec.md` y `03-tasks.md`.
2. **Filtrar tareas del especialista**:
   - Identifica en `03-tasks.md` las tareas no completadas (`- [ ]`) que pertenecen al ámbito del especialista indicado:
     - `back-domain`: Tareas de Domain y Application (Entidades, Value Objects, CQRS, Validadores).
     - `back-infra`: Tareas de EF Core, PostgreSQL, Repositorios, Migraciones.
     - `back-api`: Tareas de FastEndpoints, DTOs REPR, Contratos Espejo, Swagger.
     - `back-test`: Tareas de tests xUnit con Fakes en memoria y pruebas de integración.
     - `front-logic`: Tareas de DTOs TypeScript espejo, TanStack Query hooks, Zustand stores.
     - `front-ui`: Tareas de componentes React 19, maquetación Tailwind, Capacitor Safe Areas, vistas.
     - `front-test`: Tareas de pruebas de componentes y linter `oxlint`.
3. **Garantizar Definition of Ready (DoR) en el Ticket**:
   - `@jarvis` se asegura de que el ticket NO contenga tareas ambiguas. Cada tarea debe delimitar archivos exactos y propiedades requeridas.
4. **Generar Ticket en Buzón**:
   - Escribe el archivo `.agent-inbox/<agente>/<feature-id>.md` con el siguiente contenido:

```markdown
# Ticket de Trabajo (Sprint Task): <feature-id>
- **Agente Asignado**: <agente>
- **Fecha de Asignación**: {CURRENT_DATE}
- **Estado DoR**: Verificado por @jarvis

## 1. Contexto de User Story (extraído de 01-spec.md)
{User Story y Criterios de Aceptación BDD relevantes para esta capa}

## 2. Tareas Delimitadas a Resolver (extraídas de 03-tasks.md)
- [ ] Tarea 1: Ruta exacta, campos y requisitos atómicos...
- [ ] Tarea 2: Ruta exacta, campos y requisitos atómicos...

## 3. Contratos Técnicos Específicos (extraído de 02-tech-spec.md)
{Fragmento exacto de modelos, contratos DTO espejo, o firmas de métodos}

## 4. Definition of Done (DoD) & Comandos de Validación
- [ ] El código compila sin errores.
- [ ] Comando de validación ejecutable: `{Comando específico de este agente}`
```

5. **Confirmación**:
   - Informa a `@jarvis` que el buzón está listo para ser recogido por el especialista.
