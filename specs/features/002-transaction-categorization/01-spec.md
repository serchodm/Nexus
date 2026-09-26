# Especificación Funcional: Motor de Categorización Inteligente, Merchant Intelligence y Desglose de Gastos

- **ID de la Feature**: `002-transaction-categorization`
- **Estado**: Aprobado (Gate 1)
- **Fecha de Creación**: 2026-09-27
- **Última Actualización**: 2026-09-27
- **Dependencias**: `001-raw-statements-storage`

---

## 1. Resumen y Contexto

Tras la ingesta e inmutabilidad de los extractos bancarios en la Feature 001, las transacciones quedan almacenadas con sus descripciones brutas (ej. `"TPV 49201 BCN MERCADONA S.A. 25/09"`, `"PAYPAL *NETFLIX 9238"`, `"BIZUM CENA CUMPLEAÑOS MARTA"`, `"EL CORTE INGLES S.A."`).

Nexus va un paso más allá de las herramientas tradicionales (Fintonic, YNAB, Toshl) eliminando la categorización ciega y proporcionando un **Motor de Inteligencia Financiera** compuesto por:
1. **Taxonomía Jerárquica Híbrida**: Categorías y subcategorías del sistema con colores/iconos para dashboards + categorías personalizadas del usuario.
2. **Merchant Intelligence Pipeline**: Extracción y normalización del comercio limpio (`CleanMerchantName`), limpiando prefijos de terminales TPV y pasarelas de pago (`TPV*`, `SUMUP*`, `PAYPAL*`, `STRIPE*`, `REDYS*`).
3. **Split Transactions (Desglose de Gasto Mixto)**: Posibilidad de dividir una transacción en múltiples partidas independientes con importes y categorías distintas (ej. 130 € = 90 € Supermercado + 40 € Hogar) garantizando la invariante contable.
4. **Bizum & Transfer Concept Matching**: Categorización semántica de Bizums y transferencias basada en el texto del concepto.
5. **Detección de Recurrencia (Gastos Fijos vs. Variables / Suscripciones)**: Distinción entre el coste de vida basal comprometido del mes y el gasto discrecional variable.
6. **Pre-marcado Compartido vs. Individual**: Conexión nativa con el modelo dual de gasto, pre-asignando si una partida es personal o de grupo (Splitwise).
7. **Metadatos de Plataformas Online**: Preparación de campos para la reconciliación con pasarelas de pago (PayPal, Amazon, etc.).

---

## 2. Historias de Usuario y Casos de Uso

### Historia 1: Normalización de Comercio Limpio y Auto-categorización Inmediata
> **Como** usuario que importa un extracto con descripciones bancarias crudas y ruidosas (`TPV 9812 BCN MERCADONA`),  
> **Quiero** que el sistema limpie el nombre a `"Mercadona"` y le asigne automáticamente la categoría `Alimentación / Supermercados`,  
> **Para que** mis informes muestren comercios legibles con iconos claros sin intervención manual.

### Historia 2: Desglose de Gastos Mixtos (Split Transactions)
> **Como** usuario que realiza una compra de 120 € en *Carrefour* donde compró comida (80 €) y sartenes/menaje (40 €),  
> **Quiero** desglosar esa transacción en 2 partidas con categorías distintas,  
> **Para que** mis estadísticas reflejen la realidad de mi consumo sin romper la conciliación del saldo del banco.

### Historia 3: Inteligencia en Conceptos de Bizum y Transferencias
> **Como** usuario que recibe o emite Bizums frecuentes entre amigos,  
> **Quiero** que un Bizum con concepto `"Pádel martes"` o `"Cena cumpleaños"` se categorice automáticamente en *Deportes* o *Restaurantes*,  
> **Para que** las transferencias entre particulares no queden en una bolsa negra de "Varios".

### Historia 4: Clasificación de Gastos Fijos vs. Variables y Suscripciones
> **Como** usuario que quiere conocer su presupuesto mensual real,  
> **Quiero** que el sistema marque automáticamente qué gastos son suscripciones o costes fijos periódicos (Netflix, alquiler, luz, gimnasio) y cuáles son variables,  
> **Para que** pueda ver de un vistazo mi "coste basal de vida comprometido" frente a mi "gasto prescindible".

### Historia 5: Aprendizaje Interactivo al Recategorizar
> **Como** usuario que cambia manualmente la categoría de un movimiento,  
> **Quiero** que el sistema me ofrezca crear una regla automática para ese comercio con aplicación retroactiva en 1 clic,  
> **Para que** la plataforma aprenda de mis hábitos y sea cada vez más autónoma.

### Historia 6: Enlace con el Modelo Dual (Gasto Compartido vs. Individual)
> **Como** usuario que gestiona gastos conjuntos con su pareja y gastos personales,  
> **Quiero** que los gastos de ciertas categorías o cuentas queden pre-marcados como "Compartidos" (ej. 50/50),  
> **Para que** el módulo de gastos compartidos (Splitwise) se alimente de forma transparente y sin doble entrada de datos.

---

## 3. Alcance (Scope)

### ✅ Dentro del Alcance (In-Scope)
- **Modelo de Taxonomía (`Category`)**:
  - Jerarquía de 2 niveles: Categoría Padre $\rightarrow$ Subcategorías Hijas.
  - Categorías del Sistema (seed global inmutable con iconos y colores HSL).
  - Categorías Personalizadas del Usuario (`UserId != null`).
  - Tipo de Flujo Financiero: `Expense` (Gasto), `Income` (Ingreso), `Transfer` (Transferencia/Ajuste).
  - Naturaleza Predeterminada: `DefaultExpenseNature` (`Individual`, `Shared`).
- **Pipeline de Merchant Intelligence**:
  - Normalizador de texto que elimina prefijos bancarios comunes (`TPV*`, `SUMUP*`, `PAYPAL*`, `STRIPE*`, `REDYS*`, fechas y números de terminal).
  - Extracción del `CleanMerchantName`.
- **Desglose de Transacciones (`TransactionSplit`)**:
  - Toda transacción nace como categoría única (100% del importe).
  - Entidad `TransactionSplitItem`: `Id`, `BankTransactionId`, `CategoryId`, `SubcategoryId`, `Amount`, `Description`, `ExpenseNature`.
  - Invariante contable estricta: $\sum \text{SplitItems.Amount} == \text{BankTransaction.Amount}$.
- **Motor de Reglas de Categorización (`CategorizationRule`)**:
  - `Pattern`: Cadena de búsqueda normalizada (sin acentos, insensible a mayúsculas/minúsculas).
  - `MatchType`: `Contains`, `StartsWith`, `Exact`, `Regex`.
  - `TargetCategoryId` y `TargetSubcategoryId`.
  - `Priority`: Entero para orden de ejecución.
  - `SetExpenseNature`: Posibilidad de forzar `Individual` o `Shared`.
  - `SetIsRecurring`: Posibilidad de marcar como suscripción/fijo.
- **Motor Semántico de Bizums**:
  - Diccionario de palabras clave en conceptos de Bizum (`"cena"`, `"comida"`, `"padel"`, `"futbol"`, `"regalo"`, `"gasolina"`, `"alquiler"`).
- **Detección de Recurrencia**:
  - Campos en `BankTransaction`: `IsRecurring` (booleano), `RecurrencePeriod` (`Monthly`, `Yearly`, `None`), `ExpenseFlexibility` (`Fixed`, `Discretionary`).
- **Metadatos Preparatorios para Pasarelas (PayPal / Amazon)**:
  - `ExternalPlatform`: Enum (`None`, `PayPal`, `Amazon`, `Apple`, `Klarna`).
  - `PendingPlatformEnrichment`: Booleano (para transacciones opacas `PAYPAL *[hex]` que requerirán cotejo en el Slice 003).

### 🚫 Fuera del Alcance (Out-of-Scope para este Slice)
- ❌ **Conexión API REST directa con PayPal / Amazon** (corresponde al Vertical Slice `003-platform-connectors-paypal-amazon`).
- ❌ **Escaneo OCR de fotos de tickets de caja** (corresponde a un slice futuro de digitalización documental).
- ❌ **Liquidación de balances entre personas (Splitwise)** (corresponde a `004-shared-expenses`).

---

## 4. Reglas de Negocio e Invariantes

1. **RN-01 [Prevalencia Estricta de Categorización]**:
   $$\text{Manual (100\%)} > \text{Regla de Usuario} > \text{Categoría Nativa de Banco (ING)} > \text{Regla Global de Sistema} > \text{Sin Categoría}$$
   Las transacciones marcadas como `Manual` están protegidas contra cualquier sobreescritura automática.
2. **RN-02 [Invariante Contable del Desglose (Splits)]**:
   Si una transacción tiene desgloses (`SplitItems`), la suma de los importes de todas sus líneas debe coincidir con precisión de céntimo ($0.01 €$) con el `Amount` de la transacción bancaria. Si la transacción no tiene desgloses, su `Amount` computa íntegro a su categoría única.
3. **RN-03 [Normalización de Merchant]**:
   El extractor debe suprimir prefijos de TPV (`TPV`, `SUMUP`, `PAYPAL *`, `STRIPE *`, códigos postales o sufijos de sociedad `S.A.`, `S.L.`, `SL`, `SA`) para obtener un nombre limpio reproducible.
4. **RN-04 [Categorización Semántica de Bizums]**:
   Si una transacción es detectada como Bizum (por canal bancario o concepto `BIZUM`), el motor evalúa el texto del comentario con el diccionario semántico. Si no coincide ninguna regla, se clasifica como `Transferencias / Bizum` y no como comercio genérico.
5. **RN-05 [Reglas de Usuario Aisladas y Retroactivas]**:
   Un usuario solo puede crear o editar sus propias reglas. Si selecciona `ApplyToExisting = true`, la regla se ejecuta sobre su histórico para actualizar transacciones que no hayan sido editadas manualmente.
6. **RN-06 [Inmutabilidad de Transacciones de Origen]**:
   La categorización, el desglose o la normalización de comercio no modifican los campos contables de auditoría de la Feature 001 (`Amount`, `OperationDate`, `ValueDate`, `BalanceAfter`, `RawDataJson`).

---

## 5. Taxonomía Inicial del Sistema (Catálogo Predeterminado)

| Categoría Padre | Color Token | Naturaleza Defecto | Subcategorías Predeterminadas |
| :--- | :--- | :--- | :--- |
| **Alimentación** | `#10B981` | Compartido (en cuentas conjuntas) | Supermercados, Restaurantes y Bares, Cafeterías, Comida a Domicilio |
| **Hogar y Vivienda** | `#3B82F6` | Compartido | Alquiler / Hipoteca, Electricidad y Gas, Agua, Internet y Telefonía, Menaje y Mantenimiento |
| **Transporte** | `#F59E0B` | Individual / Compartido | Combustible, Transporte Público, Taxi / VTC, Parking y Peajes, Mantenimiento Vehículo |
| **Suscripciones y Servicios** | `#8B5CF6` | Individual | Streaming (Netflix/Spotify), Software y Cloud, Gimnasio, Membresías |
| **Ocio y Estilo de Vida** | `#EC4899` | Individual | Viajes y Vacaciones, Cine y Espectáculos, Ropa y Calzado, Hobbies y Deporte |
| **Salud y Bienestar** | `#06B6D4` | Individual | Farmacia, Médicos y Clínicas, Cuidado Personal |
| **Finanzas e Ingresos** | `#22C55E` | Individual | Nómina / Salario, Dividendos e Intereses, Transferencias Recibidas |
| **Impuestos y Seguros** | `#64748B` | Individual | IRPF, IBI, Tasas Municipales, Seguros Generales |

---

## 6. Criterios de Aceptación (BDD / Given-When-Then)

### Escenario 1: Pipeline de Merchant Intelligence y Auto-categorización
- **Dado que** se importa una transacción con descripción `"TPV 49201 BCN MERCADONA S.A. 25/09"` por importe `-45,20 €`,
- **Cuando** pasa por el pipeline de categorización,
- **Entonces** `CleanMerchantName` se resuelve como `"Mercadona"`,
- **Y** se asigna automáticamente la categoría `Alimentación / Supermercados` con `CategorizationSource = RuleEngine`.

### Escenario 2: Desglose de Transacción Mixta (Split Transaction)
- **Dado que** el usuario tiene una transacción en `"Carrefour"` por `-120,00 €`,
- **Cuando** solicita desglosar la transacción en:
  - Partida 1: `-80,00 €` en `Alimentación / Supermercados`
  - Partida 2: `-40,00 €` en `Hogar / Menaje y Mantenimiento`
- **Entonces** el sistema valida que $-80 + (-40) == -120$,
- **Y** crea los dos registros `TransactionSplitItem`, manteniendo el saldo contable intacto.

### Escenario 3: Concepto semántico en Bizum
- **Dado que** se importa un movimiento `"BIZUM DE CARLOS CENA EL BODEGON"` por `-25,00 €`,
- **Cuando** el motor semántico de Bizums evalúa el concepto,
- **Entonces** detecta el término `"CENA"` y clasifica la transacción en `Alimentación / Restaurantes y Bares`.

### Escenario 4: Detección de Recurrencia y Suscripciones
- **Dado que** entra un cargo `"PAYPAL *NETFLIX"` por `-17,99 €`,
- **Cuando** se categoriza en `Suscripciones / Streaming`,
- **Entonces** el sistema marca `IsRecurring = true`, `RecurrencePeriod = Monthly` y `ExpenseFlexibility = Fixed`.

### Escenario 5: Aprendizaje Interactivo con Creación de Regla
- **Dado que** el usuario recategoriza manualmente una transacción `"CABIFY MADRID"` a `Transporte / Taxi y VTC`,
- **Cuando** confirma el modal interactivo de *"Crear regla para comercios que contengan 'CABIFY'"*,
- **Entonces** se crea una nueva `CategorizationRule` y todas las transacciones previas con `"CABIFY"` que no fueran manuales se actualizan automáticamente.

---

## 7. Preguntas Abiertas y Decisiones

1. **¿Qué comportamiento prefieres en la UI para transacciones desglosadas (splits)?**
   - **Opción A (Recomendada)**: La tabla de transacciones muestra la fila principal con una pequeña etiqueta `[2 partidas]` que al hacer clic se despliega como un acordeón mostrando las 2 subpartidas.
   - **Opción B**: Mostrar directamente cada subpartida como una fila independiente con una marca visual que indica que provienen de la misma transacción bancaria.
