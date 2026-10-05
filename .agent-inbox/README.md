# Agent Inbox

Buzón de mensajería y delegación asíncrona entre `@jarvis` y los agentes especialistas de **Nexus**.

## Estructura
```text
.agent-inbox/
├── back-domain/     # Tareas de Dominio y Aplicación C#
├── back-infra/      # Tareas de EF Core, PostgreSQL y Migraciones
├── back-api/        # Tareas de FastEndpoints y DTOs
├── back-test/       # Tareas de tests unitarios e integración backend
├── front-logic/     # Tareas de TanStack Query, DTOs TS, Zustand y Auth
├── front-ui/        # Tareas de componentes React 19 y Tailwind
└── front-test/      # Tareas de Vitest y React Testing Library
```

## Protocolo de Comunicación
1. **Delegación**: `@jarvis` ejecuta `/opsx-delegate` y escribe `.agent-inbox/{agente}/{feature-id}.md`.
2. **Pickup & Trabajo**: El agente especialista ejecuta `/opsx-pickup`, lee su orden, implementa el código en su ámbito exclusivo y ejecuta su comando de validación.
3. **Cierre de Tarea**: Al compilar sin errores, el agente marca las casillas correspondientes en `specs/features/{feature-id}/03-tasks.md` (`- [ ]` -> `- [x]`) y elimina o archiva su ticket del inbox.
4. **Protocolo de Incongruencia**: Si se detecta una contradicción de diseño o un error irresoluble, escribe `BLOCKED_{agente}.md` en su buzón y detiene la ejecución para requerir intervención del orquestador/usuario.
