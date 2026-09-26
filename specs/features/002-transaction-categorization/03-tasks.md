# Tareas de Implementación: Motor de Categorización Inteligente, Merchant Intelligence y Desglose de Gastos

- **ID de la Feature**: `002-transaction-categorization`
- **Especificaciones de Referencia**:
  - Funcional: [01-spec.md](./01-spec.md)
  - Técnica: [02-tech-spec.md](./02-tech-spec.md)
- **Estado**: En Ejecución (Gate 3 Aprobado - Fases 1 a 6 Abiertas en GitHub)

---

## Checklist de Progreso

### Fase 1: Dominio (`Nexus.Domain`)
- [x] **Tarea 1.1**: Enums de Dominio Financiero:
  - Crear `FlowType`, `ExpenseNature`, `ExpenseFlexibility`, `RecurrencePeriod`, `CategorizationSource`, `RuleMatchType`, `ExternalPlatform` en `Nexus.Domain/Enums/`.
  - *Criterio de verificación*: Archivos en `Nexus.Domain/Enums/`, compilación limpia.
- [x] **Tarea 1.2**: Entidades de Dominio:
  - Crear entidad `Category` (auto-referenciada para Padre/Hijas, icono, color HSL, `FlowType`, `DefaultExpenseNature`).
  - Crear entidad `CategorizationRule` (patrón normalizado, tipo de concordancia, prioridad, categoría destino y flags de recurrencia/naturaleza).
  - Crear entidad `TransactionSplitItem` (FK a `BankTransaction`, FK a `Category`, importe, descripción y `ExpenseNature`).
  - Modificar `BankTransaction` para incluir los nuevos campos de enriquecimiento (`CleanMerchantName`, `CategoryId`, `SubcategoryId`, `CategorizationSource`, `IsManual`, `IsRecurring`, `RecurrencePeriod`, `ExpenseFlexibility`, `ExpenseNature`, `ExternalPlatform`, `PendingPlatformEnrichment`, `HasSplits`, `SplitItems`).
  - *Criterio de verificación*: Entidades compilan sin errores en `Nexus.Domain`.
- [x] **Tarea 1.3**: Servicios Puros y Validadores de Dominio:
  - `MerchantNormalizer`: Extractor y normalizador de texto limpio eliminando prefijos de TPV, pasarelas (`TPV`, `SUMUP`, `PAYPAL`, `STRIPE`, `REDYS`), fechas, códigos postales y sufijos societarios (`S.A.`, `S.L.`).
  - `BizumConceptClassifier`: Clasificador semántico de conceptos de Bizum (`"cena"`, `"pádel"`, `"gasolina"`, etc.).
  - `TransactionSplitValidator`: Validador estricto de la invariante contable $\sum \text{SplitItems.Amount} == \text{Transaction.Amount}$ con tolerancia 0.00 €.
  - `CategorizationEngine`: Algoritmo en cascada que implementa la prevalencia **RN-01** (Manual > Regla Usuario > Nativa Banco > Regla Sistema > Bizum).
  - Interfaces de Repositorio: `ICategoryRepository` e `ICategorizationRuleRepository`.
  - *Criterio de verificación*: 43/43 tests unitarios superados al 100% en `Nexus.UnitTests`.

### Fase 2: Infraestructura y Persistencia (`Nexus.Infrastructure`)
- [x] **Tarea 2.1**: Configuraciones de Entidad EF Core (Fluent API) con `snake_case`:
  - `CategoryConfiguration`: Tabla `categories`, auto-referencia `parent_category_id`, FK opcional a `users`, índice condicional único `(user_id, parent_category_id, normalized_name)` con filtro `is_deleted = false`.
  - `CategorizationRuleConfiguration`: Tabla `categorization_rules`, FK a `users` y `categories`, índice en `(user_id, is_active, priority)`.
  - `TransactionSplitItemConfiguration`: Tabla `transaction_split_items`, FK con cascade delete a `bank_transactions`, precisión decimal `(18,2)`.
  - Actualizar `BankTransactionConfiguration` con los nuevos campos, FK a categorías con `OnDelete(SetNull)` e índices estratégicos.
  - Actualizar `ApplicationDbContext` con `DbSet<Category>`, `DbSet<CategorizationRule>`, `DbSet<TransactionSplitItem>`.
- [x] **Tarea 2.2**: Catálogo Semilla de Categorías del Sistema (Seed Data):
  - Definir las 8 categorías padre y sus correspondientes subcategorías con Guids fijos deterministas, iconos y colores HSL (40 categorías iniciales).
- [x] **Tarea 2.3**: Repositorios de Infraestructura:
  - Implementar `CategoryRepository` (árbol jerárquico unificado sistema + usuario).
  - Implementar `CategorizationRuleRepository` (búsqueda y ordenación por prioridad).
  - Extender `BankTransactionRepository` con soporte para splits (`GetByIdWithSplitsAsync`) y actualización en lote para reglas retroactivas.
- [x] **Tarea 2.4**: Migración EF Core y Actualización de Base de Datos:
  - Generar migración: `20260927_AddTransactionCategorizationAndSplits`
  - Aplicar migración en base de datos PostgreSQL: `dotnet ef database update`
  - *Criterio de verificación*: Tablas `categories`, `categorization_rules`, `transaction_split_items` creadas e índices y 40 categorías del sistema verificadas en PostgreSQL.

### Fase 3: Capa de Aplicación (`Nexus.Application`)
- [ ] **Tarea 3.1**: DTOs y Mappings:
  - `CategoryDto`, `CategoryTreeDto`, `CategorizationRuleDto`, `SplitItemDto`, `SplitItemInput`, `TransactionDetailDto`, `TransactionFilterQuery`.
- [ ] **Tarea 3.2**: Casos de Uso y Comandos de Categorías:
  - `GetCategoryTreeQuery` & Handler.
  - `CreateCategoryCommand` & Handler.
  - `UpdateCategoryCommand` & Handler.
  - `DeleteCategoryCommand` & Handler.
- [ ] **Tarea 3.3**: Casos de Uso y Comandos de Reglas:
  - `GetRulesQuery` & Handler.
  - `CreateCategorizationRuleCommand` & Handler (con soporte de aplicación retroactiva).
  - `UpdateCategorizationRuleCommand` & Handler.
  - `DeleteCategorizationRuleCommand` & Handler.
  - `ApplyRulesRetroactiveCommand` & Handler (actualiza transacciones automáticas respetando `IsManual == false`).
- [ ] **Tarea 3.4**: Casos de Uso de Transacciones:
  - `CategorizeTransactionCommand` & Handler (asignación manual + toggle de auto-creación de regla).
  - `SplitTransactionCommand` & Handler (validación con `TransactionSplitValidator` y guardado atómico).
  - `ResetTransactionSplitsCommand` & Handler (eliminar splits y restaurar categoría única).
  - `GetTransactionsQuery` & Handler (consulta paginada con filtros avanzados de categoría, merchant, recurrencia, fechas, cuenta y splits).
- [ ] **Tarea 3.5**: Integración en el Pipeline de Ingesta (`ConfirmStatementImportCommandHandler`):
  - Invocar `MerchantNormalizer`, `ExternalPlatformDetector` y `CategorizationEngine` antes de persistir las transacciones del extracto.
  - *Criterio de verificación*: `dotnet build` en `Nexus.Application` con 0 errores.

### Fase 4: Presentación API (`Nexus.Api`)
- [ ] **Tarea 4.1**: Endpoints de Categorías (FastEndpoints):
  - `GET /api/categories` (`GetCategoryTreeEndpoint`)
  - `POST /api/categories` (`CreateCategoryEndpoint`)
  - `PUT /api/categories/{id}` (`UpdateCategoryEndpoint`)
  - `DELETE /api/categories/{id}` (`DeleteCategoryEndpoint`)
- [ ] **Tarea 4.2**: Endpoints de Reglas:
  - `GET /api/rules` (`GetRulesEndpoint`)
  - `POST /api/rules` (`CreateCategorizationRuleEndpoint`)
  - `PUT /api/rules/{id}` (`UpdateCategorizationRuleEndpoint`)
  - `DELETE /api/rules/{id}` (`DeleteCategorizationRuleEndpoint`)
  - `POST /api/rules/apply-retroactive` (`ApplyRulesRetroactiveEndpoint`)
- [ ] **Tarea 4.3**: Endpoints de Transacciones y Splits:
  - `GET /api/transactions` (`GetTransactionsEndpoint`)
  - `GET /api/transactions/{id}` (`GetTransactionByIdEndpoint`)
  - `PATCH /api/transactions/{id}/categorize` (`CategorizeTransactionEndpoint`)
  - `POST /api/transactions/{id}/split` (`SplitTransactionEndpoint`)
  - `DELETE /api/transactions/{id}/split` (`ResetTransactionSplitsEndpoint`)
- [ ] **Tarea 4.4**: Registro de Servicios en Dependency Injection.
  - *Criterio de verificación*: `dotnet build` global y Swagger interactivo con los nuevos endpoints operativos.

### Fase 5: Pruebas Unitarias y de Integración (`Nexus.UnitTests`)
- [ ] **Tarea 5.1**: Tests Unitarios de Dominio:
  - `MerchantNormalizerTests`: Validación de casos de prueba reales (Mercadona, TPV, SumUp, Cabify, PayPal, Amazon, Iberdrola).
  - `BizumConceptClassifierTests`: Conceptos semánticos (cena, fútbol, padel, gasolina, fallback).
  - `TransactionSplitValidatorTests`: Invariante contable estricta $\sum \text{Splits} == \text{Total}$ y control de tolerancias.
  - `CategorizationEngineTests`: Reglas de prevalencia RN-01 y detección de suscripciones periódicas.
- [ ] **Tarea 5.2**: Tests Unitarios de Aplicación:
  - `SplitTransactionCommandHandlerTests`: Verificación de creación de partidas y bloqueo si los importes no cuadran.
  - `ApplyRulesRetroactiveCommandHandlerTests`: Inmutabilidad de transacciones con `IsManual == true`.
  - `ConfirmStatementImportCommandHandlerTests`: Auto-enriquecimiento de transacciones al importar extractos de BBVA e ING.
- [ ] **Tarea 5.3**: Script E2E Automatizado (`tests/e2e_categorization_test.ps1`):
  - Ingesta de extracto con TPVs, Bizums y suscripciones.
  - Verificación de comercios limpios y categorías asignadas.
  - Desglose de un ticket en 2 partidas con verificación de saldo intacto.
  - Categorización manual con creación de regla y comprobación de aplicación retroactiva.
  - *Criterio de verificación*: 100% de tests unitarios y script E2E superados.

### Fase 6: Frontend (`frontend/`)
- [ ] **Tarea 6.1**: Modelos TypeScript en `types/category.ts`, `types/rule.ts` y actualización de `types/transaction.ts`.
- [ ] **Tarea 6.2**: Hooks con TanStack React Query (`useCategories.ts`, `useRules.ts`, `useTransactionSplits.ts`).
- [ ] **Tarea 6.3**: Componentes de UI:
  - `CategoryBadge`: Renderiza icono Lucide y color HSL de la categoría.
  - `TransactionRow`: Muestra `CleanMerchantName`, badge de categoría, badge de recurrencia y botón acordeón `[2 partidas]`.
  - `SplitTransactionModal`: Modal interactivo que permite añadir $N$ líneas de desglose con barra de balance en tiempo real ("Restante por asignar: 0,00 €").
  - `QuickCategorizePopover`: Selector rápido de categoría con toggle *"Crear regla para comercios similares"*.
  - `RulesManagerModal`: Vista de gestión y reordenación de reglas del usuario.
  - *Criterio de verificación*: `npm run build` sin errores en frontend.

---

## Mapeo con GitHub Issues

- **Epic Padre**: [#8 - [Feature 002] Motor de Categorización Inteligente, Merchant Intelligence y Desglose de Gastos](https://github.com/serchodm/Nexus/issues/8)

| Fase | Issue | Título |
| :--- | :--- | :--- |
| **Fase 1: Dominio** | [#9](https://github.com/serchodm/Nexus/issues/9) | `[002-Fase 1] Dominio: Taxonomía de Categorías, Merchant Normalizer y Validación de Splits` |
| **Fase 2: Infraestructura** | [#10](https://github.com/serchodm/Nexus/issues/10) | `[002-Fase 2] Infraestructura: Esquema EF Core, Seed Data de Categorías y Migración PostgreSQL` |
| **Fase 3: Aplicación** | [#11](https://github.com/serchodm/Nexus/issues/11) | `[002-Fase 3] Aplicación: Casos de uso de Categorización, Motor de Reglas Retroactivas y Splits` |
| **Fase 4: API** | [#12](https://github.com/serchodm/Nexus/issues/12) | `[002-Fase 4] API: Endpoints FastEndpoints para Categorías, Reglas y Desglose de Gastos` |
| **Fase 5: Pruebas** | [#13](https://github.com/serchodm/Nexus/issues/13) | `[002-Fase 5] Pruebas: Cobertura Unitaria Exhaustiva y Script de Verificación E2E` |
| **Fase 6: Frontend** | [#14](https://github.com/serchodm/Nexus/issues/14) | `[002-Fase 6] Frontend: Visualización de Splits, Badges HSL y Modales de Categorización` |
