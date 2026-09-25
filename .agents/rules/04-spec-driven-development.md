# Directrices SDD: Spec-Driven Development (Desarrollo Guiado por Especificaciones)

## 1. Principio Fundamental
**Ninguna línea de código de producción o refactorización estructural se escribe sin una especificación previa aprobada por el usuario.**
La especificación almacenada en `specs/features/` constituye la **Única Fuente de Verdad (Single Source of Truth - SSOT)** del sistema. Cualquier contradicción entre código y especificación se resuelve actualizando la especificación con consentimiento del usuario o alineando el código con la especificación.

---

## 2. Estructura de Directorios
Toda funcionalidad o módulo nuevo debe residir en `specs/features/<num>-<nombre-kebab-case>/`:

```text
specs/
├── SDD_FRAMEWORK.md                    # Marco normativo y reglas de SDD
├── templates/                          # Plantillas oficiales
│   ├── 01-functional-spec.template.md
│   ├── 02-technical-spec.template.md
│   └── 03-tasks.template.md
└── features/
    └── 001-user-profile-management/    # Ejemplo de feature
        ├── 01-spec.md                  # Especificación funcional
        ├── 02-tech-spec.md             # Especificación técnica y arquitectura
        └── 03-tasks.md                 # Tareas atómicas y checklist de ejecución
```

---

## 3. Ciclo de Vida Obligatorio de 4 Fases

### Fase 1: Especificación Funcional (`01-spec.md`)
- **Objetivo**: Definir el *QUÉ* y el *POR QUÉ* desde la perspectiva de negocio y usuario.
- **Secciones obligatorias**:
  1. *Contexto & Resumen Ejecutivo*: Qué problema resuelve y a quién beneficia.
  2. *Casos de Uso & Historias de Usuario*: Actores y flujos principales.
  3. *Alcance Estricto*: Definir claramente *In Scope* vs. *Out of Scope*.
  4. *Reglas de Negocio*: Invariantes, restricciones y validaciones requeridas.
  5. *Criterios de Aceptación*: Formato BDD / Gherkin (*Given-When-Then*).
- 🛑 **Gate 1**: Prohibido avanzar a la Fase 2 sin la aprobación explícita del usuario.

### Fase 2: Especificación Técnica (`02-tech-spec.md`)
- **Objetivo**: Diseñar la arquitectura, modelos y contratos antes de codificar.
- **Secciones obligatorias**:
  1. *Dominio*: Entidades, Value Objects, interfaces y agregados.
  2. *Aplicación*: Comandos/Queries CQRS, DTOs y reglas de validación FluentValidation.
  3. *Infraestructura & Base de Datos*: Esquema PostgreSQL, relaciones, índices y migración EF Core.
  4. *API REST (FastEndpoints)*: Endpoints, rutas, contratos de request/response y códigos HTTP.
  5. *Frontend & Mobile Shell*: Modelos TypeScript, hooks React Query, gestión de estado y componentes UI.
  6. *Seguridad & Rendimiento*: Permisos, políticas de autorización, auditoría y optimizaciones.
- 🛑 **Gate 2**: Prohibido avanzar a la Fase 3 sin la aprobación explícita de la arquitectura por el usuario.

### Fase 3: Desglose de Tareas Atómicas (`03-tasks.md`)
- **Objetivo**: Planificar la secuencia exacta de implementación con granularidad fina.
- **Estructura de tareas**:
  - Orden vertical dependiente (Bottom-Up: Domain -> Application -> Infrastructure -> Api -> Frontend -> Tests).
  - Tareas autocontenidas y verificables de forma independiente.
  - Cada tarea incluye criterio de verificación (e.g., `dotnet build` sin errores, test unitario exitoso, etc.).

### Fase 4: Implementación Guiada & Verificación Continua
- **Reglas durante el desarrollo**:
  1. Implementar exclusivamente lo detallado en la tarea en curso.
  2. Ejecutar validación inmediata (compilación backend y frontend).
  3. Actualizar el estado en `03-tasks.md` (`[ ]` -> `[x]`).
  4. Si durante la implementación surge un imprevisto técnico o cambio de requerimiento, **primero se actualiza `02-tech-spec.md`**, se valida con el usuario y luego se prosigue.
  5. Commits atómicos con referencia al número de feature (e.g. `feat(001): implement domain entity and unit of work`).

---

## 4. Prohibiciones Explícitas
- ❌ **No "Code First" sin Spec**: No crear archivos `.cs` o `.tsx` de una nueva feature sin sus archivos en `specs/features/`.
- ❌ **No asumir reglas de negocio no especificadas**: Si un comportamiento no está en `01-spec.md`, se debe consultar al usuario antes de inventarlo en el código.
- ❌ **No saltarse los Gates de aprobación**: Esperar la confirmación del usuario al terminar `01-spec.md` y `02-tech-spec.md`.
