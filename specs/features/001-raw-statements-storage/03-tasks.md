# Tareas de Implementación: Ingesta y Almacenamiento Inmutable de Extractos Multi-Banco

- **ID de la Feature**: `001-raw-statements-storage`
- **Especificaciones de Referencia**:
  - Funcional: [01-spec.md](./01-spec.md)
  - Técnica: [02-tech-spec.md](./02-tech-spec.md)
- **Estado**: Completado con Éxito (100% de Pruebas Unitarias y E2E Verificadas)

---

## Checklist de Progreso

### Fase 1: Dominio (`Nexus.Domain`)
- [x] **Tarea 1.1**: Crear enums de dominio (`AccountType`, `HolderRole`, `BankType`).
  - *Criterio de verificación*: Archivos en `Nexus.Domain/Enums/`, compilación limpia.
- [x] **Tarea 1.2**: Implementar servicios puros y algoritmos de dominio:
  - `TextSanitizer`: colapso de espacios múltiples y enmascaramiento de PAN (`**** **** **** 6589`).
  - `TransactionFingerprintGenerator`: hash SHA-256 determinista.
  - `BankEntityResolver`: resolución oficial de 4 dígitos de entidad bancaria por IBAN.
  - *Criterio de verificación*: Métodos estáticos puros y probados.
- [x] **Tarea 1.3**: Implementar entidades de dominio e interfaces:
  - `BankAccount` (con `HolderRole`, `OwnershipPercentage`, `CurrentBalance`, `LinkedCardsLast4`).
  - `StatementImport` (con `FileHashSha256`, saldos inicial/final cronológicos).
  - `BankTransaction` (canónica, `RawDataJson`, `Fingerprint`, `StatementImportId`).
  - Interfaces de repositorio: `IBankAccountRepository`, `IStatementImportRepository`, `IBankTransactionRepository`.
  - *Criterio de verificación*: `dotnet build` en `Nexus.Domain` con 0 errores y 0 advertencias.

### Fase 2: Infraestructura y Base de Datos (`Nexus.Infrastructure`)
- [x] **Tarea 2.1**: Instalar paquetes NuGet requeridos:
  - `ExcelDataReader` y `ExcelDataReader.DataSet` (v3.7+)
  - `System.Text.Encoding.CodePages`
  - *Criterio de verificación*: Restauración NuGet exitosa.
- [x] **Tarea 2.2**: Crear configuraciones EF Core (Fluent API) con nombres `snake_case`:
  - `BankAccountConfiguration` (índice condicional único por IBAN no borrado).
  - `StatementImportConfiguration` (índice único compuesto `(bank_account_id, file_hash_sha256)` con filtro condicional de borrado lógico).
  - `BankTransactionConfiguration` (mapeo `raw_data_json` a `jsonb`, índices `fingerprint_unique`, `statement_import_id`, `account_date` con filtro condicional).
  - Actualizar `ApplicationDbContext` con los 3 `DbSet`.
- [x] **Tarea 2.3**: Implementar repositorios de infraestructura (`BankAccountRepository`, `StatementImportRepository`, `BankTransactionRepository`).
- [x] **Tarea 2.4**: Generar y aplicar migración EF Core a PostgreSQL 17:
  - `dotnet ef migrations add 20260927_AddMultiBankStatementsSchema`
  - `dotnet ef migrations add 20260927_AddSoftDeleteQueryFiltersAndIndexes`
  - `dotnet ef database update`
  - *Criterio de verificación*: Tablas e índices creados correctamente en el contenedor PostgreSQL `nexus-postgres`.

### Fase 3: Capa de Aplicación (`Nexus.Application`)
- [x] **Tarea 3.1**: Implementar la arquitectura de Parsers (Strategy Pattern):
  - Contratos `IStatementParser`, `ParsedRawTransaction`, `StatementParseResult`.
  - `BbvaStatementParser` (soporte CSV/TSV y Excel, ordenación cronológica ascendente, parseo decimal español).
  - `IngDirectStatementParser` (preservación de `BankCategory` y `BankSubcategory`, orden cronológico).
  - `StatementParserFactory` (auto-detección por cabeceras).
- [x] **Tarea 3.2**: Implementar DTOs y Comandos CQRS:
  - `CreateBankAccountCommand` y Handler con validación `FluentValidation`.
  - `PreviewStatementCommand` y Handler (con `IMemoryCache` de 15 minutos, generación de `previewToken`, detección de `BALANCE_DISCONTINUITY` y `CARD_MISMATCH`).
  - `ConfirmStatementImportCommand` y Handler (recuperación de caché por `previewToken`, deduplicación por fingerprint, bulk insert y actualización de `CurrentBalance`).
  - `RollbackStatementImportCommand` y Handler (borrado en cascada y reajuste de saldo).
  - `GetBankAccountsQuery` (ponderación patrimonial según `HolderRole`).
  - `GetAccountTransactionsQuery` (paginado y filtros de fecha).
  - *Criterio de verificación*: `dotnet build` en `Nexus.Application` con 0 errores.

### Fase 4: Presentación API (`Nexus.Api`)
- [x] **Tarea 4.1**: Crear endpoints FastEndpoints:
  - `POST /api/accounts` (`CreateBankAccountEndpoint`)
  - `GET /api/accounts` (`GetBankAccountsEndpoint`)
  - `POST /api/accounts/{id}/statements/preview` (`PreviewStatementEndpoint`)
  - `POST /api/accounts/{id}/statements/confirm` (`ConfirmStatementEndpoint`)
  - `DELETE /api/statements/{importId}` (`RollbackStatementEndpoint`)
  - `GET /api/accounts/{id}/transactions` (`GetAccountTransactionsEndpoint`)
- [x] **Tarea 4.2**: Registrar servicios en Dependency Injection (`IStatementParser`, `StatementParserFactory`, `MemoryCache`, codificaciones `CodePagesEncodingProvider`).
  - *Criterio de verificación*: `dotnet build` global y endpoints visibles en OpenAPI/Swagger.

### Fase 5: Pruebas y Verificación E2E
- [x] **Tarea 5.1**: Pruebas unitarias para parsers, enmascaramiento PAN y deduplicación con los datos reales de BBVA e ING Direct.
  - *Resultado*: 19 tests unitarios superados al 100% en `Nexus.UnitTests`.
- [x] **Tarea 5.2**: Script automatizado PowerShell E2E que ejecute:
  - Creación de cuentas (BBVA e ING con distintos roles: Titular vs Cotitular vs Apoderado).
  - Previsualización y validación de alertas.
  - Confirmación con `previewToken`.
  - Re-subida para comprobar deduplicación por fingerprint (`skippedDuplicates`).
  - Rollback y comprobación de integridad.
  - *Criterio de verificación*: Script E2E `tests/e2e_statements_test.ps1` superado al 100%.

### Fase 6: Frontend (`frontend/`)
- [x] **Tarea 6.1**: Definir interfaces en `types/account.ts` y `types/statement.ts`.
- [x] **Tarea 6.2**: Crear hooks en `hooks/useAccounts.ts` y `hooks/useStatements.ts` con TanStack React Query.
- [x] **Tarea 6.3**: Implementar componentes de UI:
  - `AccountSelector` con badges de titularidad y saldo.
  - `StatementDropzone` para arrastrar ficheros.
  - `StatementPreviewModal` con alertas de discrepancia y confirmación con `previewToken`.
  - `StatementsManager` con listado de movimientos y botón de Rollback en historial.
  - *Criterio de verificación*: `npm run build` sin errores en frontend (dist generado en 1.18s).
