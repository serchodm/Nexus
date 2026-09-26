# Especificación Funcional: Ingesta y Almacenamiento Inmutable de Extractos Multi-Banco

- **ID de la Feature**: `001-raw-statements-storage`
- **Estado**: Aprobado (Gate 1 - Con Salvaguardas, Roles y Refinamiento Crítico)
- **Fecha de Creación**: 2026-09-26
- **Última Actualización**: 2026-09-27
- **Documento Macro de Referencia**: [SYSTEM_VISION.md](../../SYSTEM_VISION.md)

---

## 1. Resumen y Contexto
Esta funcionalidad constituye el primer ladrillo fundacional (*Vertical Slice 1*) de Nexus. Introduce un **Modelo Canónico Financiero Multi-Banco** que permite a los usuarios gestionar múltiples cuentas bancarias (incluso dentro de una misma entidad, como cuentas de gastos comunes, ahorros personales o fondos familiares) e importar extractos de movimientos (con soporte inicial verificado para **BBVA** e **ING Direct**).

Garantías del sistema:
1. **Roles de Titularidad Precisos**: Diferenciación legal y patrimonial entre **Titular**, **Cotitular**, **Apoderado/Autorizado** y **Representante Legal (Custodia de Menores)**, asegurando que solo el dinero que realmente pertenece al usuario compute en su patrimonio neto.
2. **Previsualización antes de persistir (con Token Efímero)**: Resumen en memoria de saldo inicial, saldo final y fechas antes de confirmar la carga definitiva, evitando tener que re-subir el fichero en la confirmación.
3. **Chequeo de coherencia y continuidad de saldo**: Detección de discontinuidades entre el saldo histórico de la cuenta y los saldos del extracto.
4. **Comprobación de tarjetas vinculadas**: Verificación cruzada entre las tarjetas detectadas en las observaciones del extracto y las tarjetas asociadas a la cuenta.
5. **Deshacer importación (Rollback en 1 Clic)**: Capacidad de revertir completamente un lote importado por error sin dejar rastro ni corromper el saldo.
6. **Robustez de Codificación y Limpieza de Datos**: Soporte transparente de codificaciones bancarias españolas (`Windows-1252`, `Latin1`, `UTF-8`) y normalización de espacios en blanco en descripciones.

---

## 2. Historias de Usuario y Casos de Uso

### Historia 1: Gestión de Cuentas Diferenciadas por Propósito y Titularidad
> **Como** usuario con múltiples cuentas en una misma entidad (e.g. cuenta de gastos comunes con mi pareja, cuenta personal, cuenta de mi negocio donde soy apoderado o cuenta de ahorro de mi hija),  
> **Quiero** dar de alta cada cuenta indicando mi rol de titularidad (**Titular**, **Cotitular**, **Apoderado** o **Representante Legal**),  
> **Para que** mis movimientos queden aislados y el sistema compute adecuadamente qué parte del saldo corresponde realmente a mi patrimonio neto.

### Historia 2: Autodetección de Entidad por Código de 4 Dígitos
> **Como** usuario que registra una cuenta bancaria con su IBAN (`ESxx 0182...` o `ESxx 1465...`),  
> **Quiero** que el sistema reconozca automáticamente la entidad financiera (BBVA, ING, Santander, CaixaBank, etc.),  
> **Para que** no tenga que seleccionar manualmente el banco en listas desplegables.

### Historia 3: Previsualización Eficiente y Confirmación de Carga
> **Como** usuario que sube un fichero de extracto bancario,  
> **Quiero** ver una vista previa con el saldo inicial real, saldo final real, número de movimientos y las primeras transacciones antes de que se guarden en la base de datos, y confirmar con 1 solo clic sin volver a subir el archivo,  
> **Para que** el proceso sea ultra-rápido en móviles y pueda verificar visualmente que estoy cargando el extracto en la cuenta correcta.

### Historia 4: Reversibilidad / Deshacer Importación en 1 Clic
> **Como** usuario que por error confirma la carga de un extracto en la cuenta equivocada,  
> **Quiero** disponer de una opción para "Deshacer la importación",  
> **Para que** el sistema elimine en una sola operación todas las transacciones de ese lote sin alterar el resto de mi histórico.

### Historia 5: Detección y Enmascaramiento de Datos Sensibles
> **Como** usuario consciente de la privacidad,  
> **Quiero** que los números de tarjeta de 16 dígitos que aparecen en las observaciones bancarias se enmascaren automáticamente (`**** **** **** 6589`),  
> **Para que** la plataforma cumpla con altos estándares de seguridad y protección de datos (GDPR / PCI).

---

## 3. Alcance (Scope)

### ✅ Dentro del Alcance (In-Scope)
- **Modelo de Cuentas Financieras (`BankAccount`)**:
  - `UserId`: Identificador del usuario propietario del registro en Nexus.
  - `Name`: Alias de la cuenta (ej. "Gastos Comunes", "Ahorro Personal", "Ahorro Hija").
  - `BankName` y `BankCode`: Nombre y código oficial de 4 dígitos (`0182` = BBVA, `1465` = ING, etc.).
  - `IbanMasked` y `Last4Digits`.
  - `AccountType`: `Checking` (Corriente), `Savings` (Ahorro), `Investment` (Inversión).
  - `CurrentBalance`: Saldo actual consolidado de la cuenta (actualizado automáticamente a la fecha más reciente).
  - `BalanceLastUpdatedAt`: Fecha/hora del último movimiento registrado.
  - **`HolderRole` (Rol de Titularidad)**:
    - `Owner` (**Titular único**): Pleno dominio del saldo (100% de cómputo patrimonial).
    - `JointOwner` (**Cotitular**): Titularidad compartida (porcentaje definido en `OwnershipPercentage`, por defecto 50%).
    - `Authorized` (**Apoderado / Autorizado**): Puede operar la cuenta y gestionar gastos, pero **0% de titularidad patrimonial** (los fondos no son suyos).
    - `LegalGuardian` (**Representante Legal / Menor**): Cuenta a nombre de un hijo o tutelado, gestionada como fondo en custodia familiar.
  - `OwnershipPercentage`: Porcentaje de propiedad sobre el saldo (ej. 100%, 50%, 0%).
  - `LinkedCardsLast4`: Lista opcional de últimos 4 dígitos de tarjetas vinculadas a la cuenta.
- **Flujo de Previsualización con Token Efímero (`StatementPreview`)**:
  - Endpoint de parsing en memoria que no persiste en base de datos.
  - Genera un `previewToken` con expiración temporal (15 minutos).
  - Devuelve: banco detectado, rango de fechas (`FromDate`, `ToDate`), saldo inicial cronológico, saldo final cronológico, total movimientos, desglose de ingresos/gastos y alertas de coherencia.
- **Registro Inmutable de Importaciones (`StatementImport`)**:
  - Identificador único de lote `Id`, `BankAccountId`, `FileName`, `FileHashSha256`, `ImportedAt`, `TotalRecords`, `ImportedRecords`, `SkippedDuplicates`.
- **Persistencia de Transacciones Canónicas (`BankTransaction`)**:
  - `StatementImportId`: Clave foránea al lote para posibilitar el rollback atómico.
  - `OperationDate`: Fecha de la operación.
  - `ValueDate`: Fecha valor contable.
  - `Description`: Comercio o concepto unificado (espacios múltiples colapsados y sanitizado de PANs).
  - `Amount`: Importe en decimal con signo (negativo = cargo, positivo = abono).
  - `Currency`: Código ISO 4217 (`EUR`).
  - `BalanceAfter`: Saldo resultante disponible tras la operación.
  - `BankCategory` y `BankSubcategory`: Categorías nativas cuando el banco las provea (e.g. ING Direct).
  - `BankMovementType`: Canal o naturaleza reportada por el banco.
  - `Observations`: Observaciones sanitizadas.
  - `RawData`: JSON inmutable con la fila exacta original para auditoría.
  - `Fingerprint`: Hash SHA-256 de unicidad para deduplicación.
- **Acción de Rollback (Deshacer Lote)**:
  - Endpoint para eliminar un `StatementImport` y todas sus `BankTransaction` asociadas en cascada transaccional, recalculando el `CurrentBalance` de la cuenta.

### 🚫 Fuera del Alcance (Out-of-Scope para este Slice)
- ❌ **Motor de Categorización Personalizada de Nexus**: Reglas automáticas definidas por el usuario (se abordará en el slice `002-categorization`).
- ❌ **Módulo de Liquidación de Gastos Compartidos (Splitwise)**: Se construirá en el slice específico de gastos compartidos.
- ❌ **Conexión Open Banking PSD2 en vivo**: Ingesta exclusiva vía ficheros exportados por el usuario.

---

## 4. Reglas de Negocio e Invariantes

1. **RN-01 [Aislamiento de Cuentas por Usuario]**: Un usuario solo puede crear, consultar, previsualizar, importar o revertir extractos en cuentas de las que sea titular en la plataforma.
2. **RN-02 [Autodetección de Entidad por IBAN]**: Si el usuario introduce un IBAN español (`ESxx EEEE ...`), el sistema resuelve la entidad a partir de los 4 dígitos `EEEE` consultando la tabla oficial del Banco de España (`0182` = BBVA, `1465` = ING, `0049` = Santander, `2100` = CaixaBank, etc.).
3. **RN-03 [Anti-Reimportación de Fichero Idéntico]**: Si el hash SHA-256 del fichero ya existe en la misma cuenta, la carga definitiva se rechaza con error `409 Conflict`.
4. **RN-04 [Inmutabilidad Contable]**: Las transacciones persistidas no pueden modificarse en sus campos financieros (`Amount`, `OperationDate`, `ValueDate`, `BalanceAfter`, `RawData`).
5. **RN-05 [Enmascaramiento de Números de Tarjeta (PAN)]**: Toda secuencia de 16 dígitos numéricos detectada en cualquier campo de texto o payload JSON debe reemplazarse por `**** **** **** <últimos-4-dígitos>`.
6. **RN-06 [Deduplicación Canónica por Fingerprint]**: Cada transacción calculará un hash determinista:
   $$\text{Fingerprint} = \text{SHA256}(\text{BankAccountId} + \text{OperationDate} + \text{Amount} + \text{NormalizedDescription} + \text{BalanceAfter})$$
   Donde `NormalizedDescription` tiene espacios múltiples colapsados y está en mayúsculas sin espacios sobrantes. Si el fingerprint ya existe en la cuenta, se ignora en la inserción y se suma al contador `SkippedDuplicates`.
7. **RN-07 [Firma de Cabeceras para Selección de Adaptador]**:
   - `F. VALOR`, `CATEGORÍA`, `SUBCATEGORÍA`, `IMPORTE (€)` $\rightarrow$ **IngDirectStatementParser**.
   - `F.Valor`, `Fecha`, `Concepto`, `Movimiento`, `Disponible` $\rightarrow$ **BbvaStatementParser**.
8. **RN-08 [Orden Cronológico Estricto de Saldos]**: Dado que los bancos suelen exportar en orden descendente (más reciente arriba), el sistema debe ordenar internamente las filas por fecha ascendente para determinar correctamente el `StartingBalance` (saldo del movimiento más antiguo del fichero) y `EndingBalance` (saldo del movimiento más reciente del fichero).
9. **RN-09 [Alerta de Discontinuidad de Saldo]**: Si la cuenta ya tiene transacciones registradas y el saldo inicial del nuevo extracto difiere en más de un margen configurable del saldo final previamente registrado, la previsualización debe retornar una advertencia `warning: "BALANCE_DISCONTINUITY"`.
10. **RN-10 [Alerta de Tarjeta No Vinculada]**: Si un extracto contiene pagos con una tarjeta cuyos últimos 4 dígitos no coinciden con las tarjetas vinculadas a la cuenta seleccionada, la previsualización retorna `warning: "CARD_MISMATCH"`.
11. **RN-11 [Reversibilidad Atómica de Lotes]**: Al invocar la reversión de un lote (`DELETE /api/statements/{importId}`), el sistema elimina en una transacción unitaria el registro `StatementImport` y todas sus transacciones asociadas, actualizando el `CurrentBalance` de la cuenta.
12. **RN-12 [Cómputo Patrimonial según Rol de Titularidad]**:
    - Si `HolderRole == Owner`, el patrimonio neto propio computa el `100% * Balance`.
    - Si `HolderRole == JointOwner`, computa `OwnershipPercentage * Balance` (por defecto 50%).
    - Si `HolderRole == Authorized`, computa `0%` del saldo en el patrimonio neto del usuario (cuenta operativa externa).
    - Si `HolderRole == LegalGuardian`, computa en una sección aislada de "Fondos Familiares / Custodia de Menores", no sumándose a la liquidez personal disponible del usuario.
13. **RN-13 [Normalización de Textos y Codificación]**: El parser debe colapsar espacios en blanco consecutivos en descripciones y soportar codificaciones `Windows-1252`, `ISO-8859-1` y `UTF-8` con o sin BOM.

---

## 5. Criterios de Aceptación (Given-When-Then)

### Escenario 1: Distinción de rol patrimonial en la cuenta
- **Dado que** el usuario registra una cuenta "Empresa Familiar" donde su rol es `Authorized` (Apoderado),
- **Cuando** importa un extracto con saldo final de 50.000 €,
- **Entonces** las transacciones se almacenan y son consultables para control de gastos,
- **Y** el cálculo de patrimonio personal del usuario no suma los 50.000 € (`OwnershipPercentage = 0%`).

### Escenario 2: Flujo en dos pasos con Token Efímero (Previsualizar y Confirmar)
- **Dado que** el usuario tiene seleccionada su cuenta "BBVA Gastos Comunes",
- **Cuando** sube un extracto de BBVA al endpoint de previsualización,
- **Entonces** recibe en respuesta HTTP 200 el `previewToken`, resumen con saldo inicial (1.129,20 €), saldo final (523,66 €) calculados cronológicamente y las 3 primeras transacciones parseadas,
- **Y** la base de datos permanece intacta (0 registros insertados),
- **Cuando** el usuario confirma la importación definitiva enviando únicamente el `{ previewToken }`,
- **Entonces** se crea el lote `StatementImport`, las transacciones quedan persistidas con éxito y el `CurrentBalance` de la cuenta pasa a ser 523,66 €.

### Escenario 3: Alerta de discrepancia de tarjeta vinculada
- **Dado que** la cuenta tiene configurada la tarjeta `9999`,
- **Cuando** se previsualiza un extracto de BBVA cuyas observaciones contienen pagos con la tarjeta `6589`,
- **Entonces** el sistema incluye en el resumen la alerta: `warning: "CARD_MISMATCH: Se detectaron movimientos de la tarjeta **** 6589 no vinculada a esta cuenta"`.

### Escenario 4: Deshacer importación (Rollback en 1 clic)
- **Dado que** un usuario importó por error un extracto que generó 40 transacciones en la cuenta `acc-1`,
- **Cuando** el usuario solicita deshacer la importación indicando el ID del lote,
- **Entonces** el sistema elimina el registro del lote y las 40 transacciones en una sola transacción SQL,
- **Y** el `CurrentBalance` de la cuenta se reajusta automáticamente al estado previo.

### Escenario 5: Deduplicación por solapamiento de fechas con espacios colapsados
- **Dado que** la cuenta ya contiene movimientos del 20 al 25 de septiembre,
- **Cuando** se confirma un extracto que contiene 15 movimientos nuevos y 5 idénticos al período solapado (incluso si en un extracto había espacios múltiples y en otro no),
- **Entonces** se insertan exactamente 15 registros canónicos,
- **Y** se omiten los 5 duplicados gracias a la normalización de la descripción en el fingerprint, retornando `skippedDuplicates: 5`.

### Escenario 6: Preservación de categorías nativas de ING Direct
- **Dado que** se procesa un extracto de ING Direct con categorías nativas ("Ocio y viajes", "Alimentación"),
- **Cuando** se persisten las transacciones canónicas,
- **Entonces** las columnas `BankCategory` y `BankSubcategory` almacenan dichos valores originales sin alteración.
