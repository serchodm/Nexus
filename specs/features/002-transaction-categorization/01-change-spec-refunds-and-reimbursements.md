# Especificación Funcional (Spec de Cambios): Contra-partidas de Gasto, Devoluciones y Reembolsos Bizum (Estrategia Océano Azul)

- **ID de la Feature**: `002-transaction-categorization`
- **Tipo de Documento**: Spec de Cambios / Evolución de Dominio (Change Spec)
- **Referencia Base**: [01-spec.md](./01-spec.md)
- **Autor & Asesor de Dominio**: `@warren` (Financial Oracle & Product Strategist)
- **Estado**: Propuesta Oficial (Gate 1 — Pendiente de Aprobación por Usuario)
- **Fecha de Creación**: 2026-10-05
- **Documento Macro de Referencia**: [SYSTEM_VISION.md](../../SYSTEM_VISION.md) (Pilar 4: Categorización Autónoma & Splits, Pilar 2: Consolidación Patrimonial y Pilar 7: Salud Financiera)

---

## 1. Resumen y Visión Estratégica (El Océano Azul)

En todas las aplicaciones de finanzas personales tradicionales (Fintonic, Copilot, Toshl, YNAB), existe un **fallo conceptual grave** en el tratamiento de los movimientos positivos:
> *Cualquier dinero que entra en la cuenta bancaria es tratado automáticamente como un "Ingreso" (Income) o genera distorsiones contables.*

### El Problema de la Realidad Cotidiana:
1. **El Bizum de la cena**: Pagas 120 € con tu tarjeta en un restaurante con 3 amigos. Tu banco registra `-120 €`. Al día siguiente, tus 3 amigos te envían 30 € cada uno por Bizum (`+90 €`).
   - *En Fintonic*: Se registra un gasto desorbitado de 120 € en ocio y un ingreso ficticio de 90 € en tu nómina/ahorro. Tus estadísticas mensuales de ahorro y gasto quedan totalmente falseadas.
2. **La Devolución en comercio (Refund)**: Compras ropa por 80 € en Zara y devuelves una prenda de 30 € recibiendo un abono bancario de `+30 €`.
   - *En Fintonic*: Te computa 80 € de gasto y 30 € de "nuevos ingresos".
3. **El Traspaso entre cuentas propias**: Mueves 1.000 € de tu cuenta corriente a tu cuenta remunerada de ahorro.
   - *En Fintonic*: Si no se configura con extrema cautela, computa como 1.000 € de gasto en una y 1.000 € de ingreso en otra.

### La Solución Nexus:
Nexus introduce la distinción contable entre **Ingreso Real (True Income)**, **Contra-partida de Gasto (Expense Reimbursement / Netting)** y **Traspaso Neutro (Internal Transfer)**:
* Un Bizum de una cena **no es un ingreso**: es un abono que **minora el gasto de la categoría** (`-120 € + 90 € = -30 €` de gasto real).
* Una devolución de tienda **minora el gasto del comercio y la categoría**, dejando el presupuesto impecable.
* Un traspaso propio es un evento **neutro en Cash Flow** que solo reubica activos en el balance patrimonial.

```text
       ┌────────────────────────────────────────────────────────┐
       │             FLUJO DE CAJA REAL EN NEXUS                │
       └──────────────────────────┬─────────────────────────────┘
                                  │
         ┌────────────────────────┼────────────────────────┐
         ▼                        ▼                        ▼
  [INGRESO REAL]          [CONTRA-PARTIDA]         [TRASPASO PROPIO]
  • Nómina / Salario      • Bizum de cena (-gasto) • De Corriente a Ahorro
  • Dividendos / Rentas   • Devolución Zara        • De Banco a Broker
  • Facturación autónomo  • Reembolso de billete   • Cero impacto en P&L
  ─────────────────────   ──────────────────────   ─────────────────────
  Suma a TotalIncome      Minora TotalExpense      Movimiento Neutro
```

---

## 2. Historias de Usuario y Casos de Uso

### Historia 1: Bizum de Vuelta como Contra-partida de Gasto
> **Como** usuario que paga periódicamente cuentas conjuntas (cenas, taxis, gasolina, compras de grupo) con su tarjeta,  
> **Quiero** que cuando mis amigos me hagan un Bizum con el concepto de la actividad (*"Cena cumpleaños"*, *"Pádel"*, *"Gasolina viaje"*), ese abono se compute como una contra-partida que minora el gasto de esa categoría,  
> **Para que** mi presupuesto refleje exclusivamente lo que realmente me costó a mí la actividad, sin inflar artificialmente mis ingresos del mes.

### Historia 2: Devolución de Compra en Comercio (Refund Netting)
> **Como** usuario que devuelve un artículo en una tienda física u online (Zara, Amazon, El Corte Inglés),  
> **Quiero** que el abono recibido en la tarjeta o cuenta se asigne a la categoría original de la compra con signo positivo de gasto (neteo),  
> **Para que** el gasto acumulado mensual de esa categoría disminuya automáticamente por el importe devuelto.

### Historia 3: Detección Automática de Traspasos Propios (Cero Falso Gasto)
> **Como** usuario con cuentas en varias entidades bancarias (BBVA corriente e ING ahorro),  
> **Quiero** que cuando transfiera fondos entre mis propias cuentas, el sistema detecte que el cargo en un banco y el abono en el otro corresponden a la misma operación interna,  
> **Para que** no se compute como gasto en el origen ni como ingreso en el destino, manteniendo mi Cash Flow limpio.

### Historia 4: Transparencia Analítica (Gasto Bruto vs. Gasto Neto)
> **Como** usuario meticuloso con sus finanzas,  
> **Quiero** poder ver en mis informes tanto el **Gasto Bruto** (cuánto dinero adelanté inicialmente) como el **Gasto Neto Real** (cuánto asumí tras los reembolsos de mis amigos),  
> **Para** entender mi volumen real de liquidez adelantada frente a mi coste de vida efectivo.

---

## 3. Alcance (Scope)

### ✅ Dentro del Alcance (In-Scope)

1. **Nuevo Clasificador Contable de Movimientos (`TransactionNature`)**:
   - `StandardExpense`: Gasto convencional que suma al presupuesto de egresos.
   - `ExpenseReimbursement`: Abono o cobro que actúa como **contra-partida** reduciendo el gasto de una categoría.
   - `TrueIncome`: Ingreso genuino que incrementa la riqueza y computa en el Cash Flow de ingresos (nómina, pensión, dividendos, ventas).
   - `InternalTransfer`: Movimiento neutro entre cuentas del mismo usuario (o de las que sea titular).

2. **Ecuación Contable de Agregación de Categorías**:
   Para cualquier categoría $C$ en un período:
   $$\text{Gasto Neto}(C) = \sum \text{StandardExpenses}(C) - \sum \text{ExpenseReimbursements}(C)$$
   $$\text{Ingresos Reales} = \sum \text{TrueIncome}$$
   *Garantía*: Los `ExpenseReimbursements` quedan **terminantemente excluidos** de la métrica `TotalIncome` del usuario.

3. **Motor Heurístico de Detección de Contra-partidas para Bizums**:
   - Extensión del diccionario de conceptos de Bizum (`specs/features/002-transaction-categorization`):
     - Si un Bizum recibido tiene concepto de consumo (*"cena"*, *"comida"*, *"copas"*, *"hotel"*, *"gasolina"*, *"padel"*), el sistema lo categoriza en la categoría de gasto respectiva pero con naturaleza `ExpenseReimbursement`.

4. **Vinculación Opcional de Devolución a Transacción Origen (`OriginalTransactionId`)**:
   - Capacidad de emparejar un abono bancario de devolución con el cargo original que lo causó.
   - Al vincularse:
     - El abono hereda automáticamente el `CleanMerchantName` y la categoría del gasto original.
     - Se valida que $|\text{Importe Abono}| \le |\text{Importe Cargo Original}|$.

5. **Detección Automática de Traspasos Internos (Matching 1:1)**:
   - Heurística de coincidencia entre cuentas del mismo usuario:
     - $|\text{Importe Cargo}| == |\text{Importe Abono}|$.
     - Divisa idéntica.
     - Fechas de operación comprendidas en una ventana de $\pm 2$ días hábiles.
     - Ambas transacciones se marcan como `InternalTransfer` y se enlazan mutuamente (`LinkedTransferTransactionId`).

### 🚫 Fuera del Alcance (Out-of-Scope)
- ❌ Módulo de grupos compartidos y liquidación multi-usuario estilo Splitwise con deudas cruzadas (se implementará en `004-shared-expenses`). Esta spec resuelve la base contable en la contabilidad individual del usuario.
- ❌ Reclamación automática de pagos o generación de enlaces de pago Bizum / PSD2.

---

## 4. Reglas de Negocio e Invariantes

### RN-CHANGE-01: Exclusión Estricta de Reembolsos en Ingresos Mensuales
Bajo ninguna circunstancia un movimiento clasificado como `ExpenseReimbursement` computará en los totalizadores de `TotalIncome`, `MonthlySalary` ni métricas de ingresos para el cálculo del *Runway* o la *Tasa de Ahorro*.

### RN-CHANGE-02: Límite de Neteo por Categoría (No Gasto Negativo Global)
En un período mensual normal, el total neto de una categoría de gasto no debería ser inferior a $0.00\ €$. Si por devoluciones de meses anteriores el total neto resultara temporalmente negativo, se muestra como saldo a favor en esa partida sin contaminar el cálculo general de ingresos.

### RN-CHANGE-03: Detección y Marcado de Traspasos Propios
Si el sistema detecta que un cargo en la Cuenta A (ej. BBVA) y un abono en la Cuenta B (ej. ING) pertenecen al mismo usuario, tienen el mismo importe y ocurrieron con un desfase máximo de 48 horas laborables:
- Se propone al usuario o se auto-clasifica como `InternalTransfer`.
- Ambos movimientos quedan excluidos tanto de `TotalExpense` como de `TotalIncome`.

### RN-CHANGE-04: Invariante de Saldo Bancario Intacta
Las clasificaciones `TransactionNature` son metadatos lógicos y analíticos. **Jamás alteran el saldo bancario real (`BalanceAfter`) ni la inmutabilidad de la transacción importada en la Feature 001**. El saldo de la cuenta bancaria sigue siendo exactamente el reportado por el banco.

---

## 5. Criterios de Aceptación (BDD / Given-When-Then)

### Escenario 1: Bizum de reembolso tras cena compartida (Neteo de Gasto)
- **Dado que** el usuario tiene un cargo en su cuenta de `-120,00 €` en el comercio *"Restaurante El Asador"* categorizado en `Restaurantes`,
- **Cuando** al día siguiente recibe un Bizum de `+30,00 €` con el concepto `"Cena asador"` de su amigo Carlos,
- **Entonces** el sistema clasifica el Bizum en la categoría `Restaurantes` con `TransactionNature = ExpenseReimbursement`,
- **Y** el informe de gastos del mes en `Restaurantes` computa un gasto neto de `-90,00 €` (`-120,00 € + 30,00 €`),
- **Y** el total de ingresos del mes (`TotalIncome`) no aumenta en 30,00 €.

### Escenario 2: Devolución de compra en tienda de ropa (Refund)
- **Dado que** el usuario compró un abrigo de `-80,00 €` en *"Zara"* (`Ropa y Calzado`),
- **Cuando** realiza una devolución parcial y el extracto bancario registra un abono de `+35,00 €` de *"ZARA ESPAÑA"*,
- **Entonces** el sistema normaliza el comercio como `"Zara"`,
- **Y** asigna `TransactionNature = ExpenseReimbursement` en `Ropa y Calzado`,
- **Y** el gasto mensual de `Ropa y Calzado` se reduce a `-45,00 €`.

### Escenario 3: Traspaso interno entre cuentas del usuario
- **Dado que** el usuario posee la cuenta corriente `ES0182...` (BBVA) y la cuenta de ahorro `ES1465...` (ING),
- **Cuando** transfiere 500,00 € el 2 de octubre desde BBVA y el abono de 500,00 € entra en ING el 3 de octubre,
- **Entonces** el motor de categorización detecta la correlación y marca ambas transacciones como `InternalTransfer`,
- **Y** el gasto total del mes no computa los 500,00 € de BBVA,
- **Y** el ingreso total del mes no computa los 500,00 € de ING.

### Escenario 4: Vinculación explícita de abono a transacción origen
- **Dado que** el usuario recibe un abono genérico de `+50,00 €` que no se auto-vinculó,
- **Cuando** el usuario selecciona la opción "Vincular a compra previa" y escoge un cargo de `-150,00 €` en *"MediaMarkt"*,
- **Entonces** el abono adopta el comercio `"MediaMarkt"`, la categoría `Tecnología` y queda enlazado con `OriginalTransactionId`.
