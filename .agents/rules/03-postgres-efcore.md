# Directrices de Persistencia: PostgreSQL + Entity Framework Core

## 1. Convenciones y Tipos en PostgreSQL
- **Nomenclatura Snake Case**: Configurar el DbContext con `.UseSnakeCaseNamingConvention()` de Npgsql para que tablas, columnas, restricciones e índices sigan el estándar `snake_case` de PostgreSQL mientras en C# se mantiene `PascalCase`.
- **Manejo Estricto de Fechas y Zonas Horarias**:
  - Toda fecha/hora debe almacenarse como `timestamp with time zone` (`timestamptz`).
  - En C#, usar `DateTimeOffset` o `DateTime` asegurando `DateTimeKind.Utc`. Prohibido persistir fechas en formato local o `DateTimeKind.Unspecified`.

---

## 2. Optimización para Alto Volumen de Lectura y Rendimiento
- **Indexación Estratégica**:
  - Índices B-Tree en todas las claves foráneas y columnas de filtrado frecuente (`status`, `created_at`, `tenant_id`, etc.).
  - Índices compuestos en el orden exacto de los filtros más habituales en las consultas de FastEndpoints.
  - Para búsquedas textuales frecuentes, evaluar índices GIN con trigramas (`pg_trgm`).
- **AsNoTracking Obligatorio**:
  - En toda consulta de solo lectura, invocar `.AsNoTracking()` para desactivar la sobrecarga del Change Tracker de EF Core.
- **Hot-Paths (Rutas Críticas de Lectura)**:
  - En endpoints con miles de peticiones por segundo, utilizar **Compiled Queries** (`EF.CompileAsyncQuery`) o **Dapper** para evitar el coste repetitivo de compilación del árbol de expresiones LINQ.
- **Preparación para Réplicas de Lectura (Read Replicas)**:
  - Estructurar el registro de servicios para permitir la inyección de una cadena de conexión de solo lectura (`IReadDbContext`) apuntando a réplicas de lectura de PostgreSQL, separada del nodo primario de escritura (`IWriteDbContext`).
- **Connection Pooling y Compatibilidad con PgBouncer**:
  - El pooling de conexiones debe ser compatible con gestores externos de transacciones (como PgBouncer / Supabase Pooler).
  - Evitar el uso de transacciones preparadas de larga duración o estados dependientes de la conexión física en Npgsql.

---

## 3. Configuración de Entidades y Modelado
- **Configuración Fluent Externa**:
  - Prohibido acumular configuraciones en `OnModelCreating`.
  - Crear clases dedicadas que implementen `IEntityTypeConfiguration<T>` por cada entidad en la capa de Infrastructure.
  - Cargar automáticamente todas las configuraciones con `modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly)`.
- **Soft Deletes y Auditoría**:
  - Entidades base con campos de auditoría (`created_at`, `updated_at`, `is_deleted`).
  - Configurar filtros de consulta globales (`HasQueryFilter(e => !e.IsDeleted)`) cuando aplique el borrado lógico.

---

## 4. Gestión de Migraciones
- **Migraciones Versionadas y Descriptivas**:
  - Generar migraciones con nombres claros: `dotnet ef migrations add <NombreDescriptivo> --project src/Infrastructure --startup-project src/Api`.
  - Prohibido el uso de `EnsureCreated()` en entornos que no sean pruebas unitarias/integración aisladas.
  - Inspeccionar siempre el código SQL generado antes de aplicar cambios en producción mediante `dotnet ef migrations script`.
