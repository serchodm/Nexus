# Tareas de Implementación: [Título de la Feature]

- **ID de la Feature**: [Ej. 001-user-profiles]
- **Especificaciones de Referencia**:
  - Funcional: `01-spec.md`
  - Técnica: `02-tech-spec.md`

---

## Checklist de Progreso

### Fase 1: Dominio (`Nexus.Domain`)
- [ ] **Tarea 1.1**: Crear entidad e interfaces de dominio.
  - *Criterio de verificación*: Compilación exitosa en `Nexus.Domain`.
- [ ] **Tarea 1.2**: Definir interfaz de repositorio y métodos requeridos.

### Fase 2: Aplicación (`Nexus.Application`)
- [ ] **Tarea 2.1**: Crear DTOs de request/response.
- [ ] **Tarea 2.2**: Implementar Command / Query y sus respectivos Handlers.
- [ ] **Tarea 2.3**: Implementar validadores con `FluentValidation`.
  - *Criterio de verificación*: `dotnet build` exitoso.

### Fase 3: Infraestructura (`Nexus.Infrastructure`)
- [ ] **Tarea 3.1**: Configurar EntityTypeConfiguration en EF Core (nombres snake_case, índices, restricciones).
- [ ] **Tarea 3.2**: Implementar repositorio en infraestructura.
- [ ] **Tarea 3.3**: Generar y aplicar migración en PostgreSQL.
  - *Criterio de verificación*: `dotnet ef database update` aplicado sin errores en contenedor PostgreSQL.

### Fase 4: Presentación API (`Nexus.Api`)
- [ ] **Tarea 4.1**: Crear Endpoint(s) FastEndpoints con patrón REPR.
- [ ] **Tarea 4.2**: Configurar seguridad/autorización y documentación OpenAPI/Swagger.
  - *Criterio de verificación*: Endpoint visible en Swagger y accesible vía HTTP.

### Fase 5: Frontend (`frontend/`)
- [ ] **Tarea 5.1**: Definir interfaces en `types/`.
- [ ] **Tarea 5.2**: Crear custom hook con TanStack Query para fetching/mutation.
- [ ] **Tarea 5.3**: Desarrollar componente de UI con diseño responsive y soporte móvil.
  - *Criterio de verificación*: `npm run build` sin errores de tipos ni warnings de bundle.

### Fase 6: Verificación E2E y Cierre
- [ ] **Tarea 6.1**: Ejecutar flujo completo de extremo a extremo.
- [ ] **Tarea 6.2**: Validar todos los Criterios de Aceptación de `01-spec.md`.
- [ ] **Tarea 6.3**: Commit git con mensaje atómico descriptivo.
