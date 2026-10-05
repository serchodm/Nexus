# Especificación Técnica (Spec de Cambios): Contra-partidas de Gasto, Devoluciones y Reembolsos Bizum

- **ID de la Feature**: `002-transaction-categorization`
- **Tipo de Documento**: Especificación Técnica de Cambios (Technical Change Spec)
- **Especificación Funcional Asociada**: [01-change-spec-refunds-and-reimbursements.md](./01-change-spec-refunds-and-reimbursements.md)
- **Autor & Orquestador**: `@jarvis` (Master Pipeline Orchestrator)
- **Estado**: Aprobado para Implementación (Gate 2)
- **Fecha**: 2026-10-05

---

## 1. Arquitectura y Modelo de Dominio (`Nexus.Domain`)

### 1.1 Nuevo Enum: `TransactionNature`
En `src/Nexus.Domain/Enums/TransactionNature.cs`:

```csharp
namespace Nexus.Domain.Enums;

/// <summary>
/// Naturaleza contable y semántica del movimiento financiero.
/// </summary>
public enum TransactionNature
{
    /// <summary>Gasto ordinario que computa en el presupuesto de salidas.</summary>
    StandardExpense = 0,

    /// <summary>Abono o contra-partida que minora el gasto de una categoría (devoluciones, Bizums de gastos compartidos).</summary>
    ExpenseReimbursement = 1,

    /// <summary>Ingreso real genuino que incrementa el patrimonio neto (nómina, rentas, rendimientos, ventas).</summary>
    TrueIncome = 2,

    /// <summary>Movimiento neutro entre cuentas propias del usuario (cero impacto en P&L).</summary>
    InternalTransfer = 3
}
```

### 1.2 Extensión de la Entidad `BankTransaction`
En `src/Nexus.Domain/Entities/BankTransaction.cs`:

```csharp
// --- Clasificador Océano Azul: Contra-partidas y Traspasos ---
public Enums.TransactionNature TransactionNature { get; set; } = Enums.TransactionNature.StandardExpense;

// Enlace opcional a transacción contraparte (compra original para devolución, o extremo del traspaso)
public Guid? LinkedTransactionId { get; set; }
public BankTransaction? LinkedTransaction { get; set; }
```

### 1.3 Servicio de Dominio: `BizumClassifier` y `ExpenseNettingCalculator`
- **Enriquecimiento de `BizumClassifier`**:
  - Si el importe es positivo (`Amount > 0`) y el concepto contiene palabras de consumo (*"cena"*, *"padel"*, *"gasolina"*, *"comida"*, etc.), el clasificador retorna no solo la categoría sugerida, sino `SuggestedNature = TransactionNature.ExpenseReimbursement`.
- **Servicio `InternalTransferMatcher`**:
  - Algoritmo que detecta si un cargo en `AccountId A` coincide con un abono en `AccountId B` del mismo usuario con:
    - $|\text{Amount}_A| == |\text{Amount}_B|$
    - $|\text{Date}_A - \text{Date}_B| \le 2\text{ días}$
    - Ambas cuentas pertenecen al mismo `UserId`.

---

## 2. Capa de Infraestructura y Base de Datos (`Nexus.Infrastructure`)

### 2.1 Mapeo Fluent API
En `src/Nexus.Infrastructure/Persistence/Configurations/BankTransactionConfiguration.cs`:
- Mapeo de `TransactionNature` como entero o string snake_case.
- Clave foránea autoreferenciada:
  ```csharp
  builder.HasOne(x => x.LinkedTransaction)
         .WithMany()
         .HasForeignKey(x => x.LinkedTransactionId)
         .OnDelete(DeleteBehavior.SetNull);
  ```

### 2.2 Migración EF Core
Nombre de la migración:
`AddTransactionNatureAndLinkedTransaction`
Campos añadidos a la tabla `bank_transactions`:
- `transaction_nature` (`integer`, NOT NULL, default `0`).
- `linked_transaction_id` (`uuid`, NULLABLE, foreign key con índice).

---

## 3. Capa de Aplicación (`Nexus.Application`)

### 3.1 Actualización del Pipeline de Ingesta
En el pipeline de auto-categorización (`ConfirmStatementImportCommandHandler` o `CategorizationEngine`):
- Si `Amount < 0`:
  - Por defecto `TransactionNature = StandardExpense`.
- Si `Amount > 0`:
  - Si el clasificador Bizum o la regla de categorización asigna una categoría de tipo `Expense` (ej. Restaurantes) $\rightarrow$ `TransactionNature = ExpenseReimbursement`.
  - Si el comercio contiene prefijos o descripciones de devolución (`ABONO`, `DEVOLUCION`, `REEMBOLSO`) $\rightarrow$ `TransactionNature = ExpenseReimbursement`.
  - Si coincide con un traspaso propio $\rightarrow$ `TransactionNature = InternalTransfer`.
  - En caso contrario (y categorías de tipo `Income`) $\rightarrow$ `TransactionNature = TrueIncome`.

### 3.2 Casos de Uso Específicos
1. `LinkTransactionCommand(Guid TransactionId, Guid TargetTransactionId)`:
   - Valida que ambas transacciones pertenezcan al mismo usuario.
   - Si es devolución: valida que el abono tenga importe menor o igual al cargo y asigna `ExpenseReimbursement`.
   - Si es traspaso: valida importes idénticos y asigna `InternalTransfer` a ambas transacciones enlazadas.
2. `UpdateTransactionNatureCommand(Guid TransactionId, TransactionNature Nature)`:
   - Permite al usuario reclasificar manualmente si desea tratar un abono como `TrueIncome` o `ExpenseReimbursement`.

---

## 4. Capa de API y Contratos (`Nexus.Api`)

### 4.1 Modificación de DTOs
En `TransactionDto` y `TransactionDetailDto`:
- Añadir `TransactionNature` (string o enum: `"StandardExpense"`, `"ExpenseReimbursement"`, `"TrueIncome"`, `"InternalTransfer"`).
- Añadir `LinkedTransactionId` (`Guid?`).
- Añadir `LinkedTransactionDescription` (`string?`).

### 4.2 Nuevos Endpoints
- `POST /api/transactions/{id}/link`:
  - Request: `{ targetTransactionId: Guid }`
  - Response: `200 OK` con `TransactionDto` actualizado.
- `PATCH /api/transactions/{id}/nature`:
  - Request: `{ nature: TransactionNature }`
  - Response: `200 OK`.

---

## 5. Capa de Frontend (`frontend/`)

### 5.1 Tipados TypeScript
En `frontend/src/types/transaction.ts`:
```typescript
export type TransactionNature = 'StandardExpense' | 'ExpenseReimbursement' | 'TrueIncome' | 'InternalTransfer';
```

### 5.2 Componentes UI y Visualización
- En la tabla de transacciones:
  - Abono `ExpenseReimbursement`: Mostrar el importe en verde suave con badge explicativo: `Reembolso / Contrapartida` y categoría asociada.
  - Abono `TrueIncome`: Mostrar con badge `Ingreso`.
  - Movimiento `InternalTransfer`: Mostrar con badge gris neutro `Traspaso`.
- Acción "Vincular a compra/traspaso" en el menú contextual de la transacción.

---

## 6. Estrategia de Pruebas (`Nexus.UnitTests`)
1. Pruebas unitarias de `BizumClassifier`:
   - `Classify_IncomingBizumWithDinnerConcept_AssignsExpenseReimbursement`.
   - `Classify_IncomingBizumWithoutExpenseKeywords_AssignsTrueIncome`.
2. Pruebas de `InternalTransferMatcher`:
   - `Match_IdenticalAmountAndCloseDates_MarksBothAsInternalTransfer`.
3. Pruebas de Handlers:
   - `LinkTransaction_ValidRefund_LinksAndSetsReimbursement`.
