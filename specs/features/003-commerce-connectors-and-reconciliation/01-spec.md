# Especificación Funcional: Conectores de Comercio & Reconciliación Externa (Amazon & PayPal Sync)

- **ID de la Feature**: `003-commerce-connectors-and-reconciliation`
- **Estado**: Aprobado (Gate 1)
- **Fecha de Creación**: 2026-09-29
- **Última Actualización**: 2026-09-29
- **Dependencias**: `001-raw-statements-storage`, `002-transaction-categorization`
- **Issue en GitHub**: [#15](https://github.com/serchodm/Nexus/issues/15)

---

## 1. Resumen y Contexto

En los extractos bancarios tradicionales (Slices 001 y 002), un volumen muy significativo de compras aparece enmascarado bajo nombres genéricos de pasarelas de pago y marketplaces:
- `AMZN Mktp ES*9X1234`
- `PAYPAL *REVENUEST` o `PAYPAL *[código alfanumérico]`
- `SUMUP *COMERCIO`

Esto provoca una ceguera analítica: el usuario sabe que gastó 89,90 € en Amazon, pero no sabe qué artículos compró ni cómo clasificarlos contablemente (¿fue un libro de trabajo, ropa deportiva o comida?). Igualmente, en PayPal se desconoce el comercio subyacente real (Spotify, Steam, Decathlon, Uber).

**Nexus 003** implementa los **Conectores de Comercio y el Motor de Reconciliación Cruzada**:
1. **Amazon Order Reports Parser**: Ingesta oficial de reportes de pedidos en CSV/ZIP generados por Amazon, extrayendo pedidos, paquetes, artículos individuales, precios, IVA y categorías estimadas.
2. **PayPal Connector**: Conexión directa mediante **OAuth2 / API REST oficial de transacciones** (`/v1/reporting/transactions`) y soporte alternativo para **importación de CSV oficial de actividad**.
3. **Motor de Reconciliación Cruzada**: Algoritmo determinista y heurístico que empareja cargos bancarios con pedidos externos por importe exacto, moneda y ventana temporal de liquidación ($\pm 7$ días).
4. **Auto-Generación de Splits**: Cuando un cargo bancario reconciliado incluye múltiples productos de diferente índole, el sistema genera automáticamente un **Split Transaction** respetando estrictamente el Invariante Contable RN-02 y la Prevalencia Heurística RN-01.

---

## 2. Historias de Usuario

### Historia 1: Ingesta de Reporte Oficial de Pedidos de Amazon (CSV)
> **Como** usuario que compra habitualmente en Amazon Prime,  
> **Quiero** arrastrar el archivo CSV oficial descargado de mi área de pedidos de Amazon,  
> **Para que** Nexus almacene todos mis pedidos y los artículos individuales comprados sin necesidad de introducir contraseñas ni usar extensiones.

### Historia 2: Sincronización Automática con PayPal (OAuth2 & API)
> **Como** usuario que paga compras online recurrentes a través de PayPal,  
> **Quiero** conectar mi cuenta de PayPal en 1 clic autorizando permisos de solo lectura,  
> **Para que** Nexus sincronice periódicamente mis transacciones y descubra el comercio real y los contratos de pago a plazos.

### Historia 3: Reconciliación de Cargos de Amazon con Desglose Automático (Splits)
> **Como** usuario con un cargo en el banco de `AMZN Mktp ES* 45,90 €`,  
> **Quiero** que el sistema detecte que corresponde al pedido de Amazon enviado el día anterior que contenía un libro (20,00 €) y café (25,90 €),  
> **Para que** mi movimiento bancario se desglose automáticamente en 2 partidas con categorías *Libros / Formación* y *Supermercado*.

### Historia 4: Desenmascaramiento de Comercios de PayPal
> **Como** usuario que tiene en su extracto bancario `PAYPAL *REVENUEST 14,99 €`,  
> **Quiero** que el sistema reemplace el comercio enmascarado por el comercio real (*"Deliveroo"* o *"Spotify"*),  
> **Para que** el panel de movimientos sea 100% legible y el gasto se clasifique con precisión.

### Historia 5: Control y Aprobación Visual de Reconciliaciones
> **Como** usuario prudente con mis finanzas,  
> **Quiero** ver una bandeja de sugerencias de reconciliación con puntuación de confianza (ej. 99% Coincidencia Exacta) antes de aplicar cambios si hay ambigüedad,  
> **Para** tener siempre el control final sobre mis cuentas.

---

## 3. Alcance (Scope)

### ✅ Dentro de Alcance
1. **Parser de Reportes de Amazon (CSV)**:
   - Soporte para el formato oficial de Amazon España (`Order History Reports` / `Datos de cuenta GDPR`).
   - Extracción de: `OrderId`, `OrderDate`, `ShipmentDate`, `Title`, `Category`, `ASIN`, `ItemTotal`, `ShippingFee`, `Tax`, `PaymentMethodLast4`.
2. **Conector PayPal (OAuth2 & CSV)**:
   - Flujo de autorización OAuth2 de PayPal (Client Id, Client Secret, Tokens cifrados).
   - Invocación a la API de reporting de transacciones (`v1/reporting/transactions`).
   - Parser de archivo CSV oficial de actividad de PayPal (para usuarios sin app developer).
   - Extracción de: `TransactionId`, `PayerEmail`, `RecipientName`, `GrossAmount`, `FeeAmount`, `ItemSubject`, `PaymentStatus`.
3. **Motor de Reconciliación Cruzada**:
   - Matching 1:1 (1 cargo bancario = 1 pedido de importe idéntico en fecha cercana).
   - Matching 1:N (1 pedido de Amazon dividido en varios cargos bancarios por expediciones separadas).
   - Matching N:1 (varios pedidos agrupados en un solo cobro bancario de PayPal).
   - Ventana temporal configurable ($\pm 7$ días entre pedido y liquidación bancaria).
4. **Auto-Generación de Splits**:
   - Generación de `TransactionSplitItem` cuando un cargo bancario corresponde a un pedido con varios artículos.
   - Respeto estricto del Invariante Contable $\sum \text{Splits} = \text{Monto Transacción}$ (tolerancia $0.00\ €$).
5. **Prevalencia y Seguridad**:
   - Inmutabilidad de movimientos manuales (`IsManual = true`).
   - Cifrado en reposo para tokens y credenciales de PayPal.

### 🚫 Fuera de Alcance (Out of Scope)
- Extensiones de navegador (Chrome/Firefox extensions) — descartadas explícitamente por el usuario para mantener compatibilidad universal.
- Scraping directo de Amazon con credenciales (usuario/contraseña).
- Ejecución de pagos o transferencias directas hacia Amazon/PayPal.

---

## 4. Reglas de Negocio e Invariantes

### RN-01: Prevalencia de Inmutabilidad Manual
Si una transacción bancaria o sus splits fueron categorizados o desglosados manualmente por el usuario (`IsManual = true`), el motor de reconciliación **no sobreescribe** automáticamente el movimiento. Puede registrar la coincidencia en estado `CandidateReview` para que el usuario la acepte o descarte voluntariamente.

### RN-02: Invariante Contable del Desglose Reconciliado
Cualquier desglose generado a partir de un pedido externo debe cumplir:
$$\sum_{i=1}^{n} \text{SplitItem.Amount}_i = |\text{BankTransaction.Amount}|$$
Si los artículos suman un importe diferente al cargo bancario debido a gastos de envío o descuentos globales del carrito:
- Los descuentos se prorratean o se crea una partida de ajuste `Descuentos / Cupones`.
- Los gastos de envío se imputan como partida independiente `Envíos & Logística` o se reparten proporcionalmente.
- La discrepancia final debe ser **estrictamente 0.00 €**.

### RN-03: Ventana de Coincidencia Temporal
La fecha de cobro bancario de compras online suele retrasarse entre 1 y 5 días respecto a la fecha del pedido:
$$\text{OrderDate} - 1\text{ día} \le \text{TransactionDate} \le \text{OrderDate} + 7\text{ días}$$
Las coincidencias fuera de esta ventana no se marcan automáticamente con alta confianza (`HighConfidence`), sino que pasan a revisión manual (`MediumConfidence`).

### RN-04: Exclusividad de Reconciliación de Ítems
Un mismo artículo de un pedido de Amazon (`OrderItemId` o tupla `OrderId + Sku`) **sólo puede vincularse a un único cargo bancario**. Una vez reconciliado, no puede volver a consumirse en otra transacción.

### RN-05: Deduplicación Canónica de Pedidos
Al subir múltiples reportes de Amazon o sincronizar PayPal repetidamente:
- Pedidos de Amazon con el mismo `OrderId` se consolidan sin duplicar registros.
- Transacciones de PayPal con el mismo `PayPalTransactionId` se consideran idempotentes.

### RN-06: Desenmascaramiento de Comercio Limpio
Cuando un cargo bancario como `PAYPAL *REVENUEST` o `AMZN Mktp ES*9X` se reconcilia exitosamente:
- El campo `CleanMerchantName` de la transacción bancaria se actualiza con el nombre del comercio real (ej. `"Deliveroo"`, `"Spotify"`, `"Amazon"`).
- Se conserva la descripción bruta original en `RawDescription` para auditoría contable.

### RN-07: Cifrado en Reposo de Credenciales PayPal
Cualquier secreto o token de acceso obtenido de PayPal (`ClientSecret`, `AccessToken`, `RefreshToken`) debe almacenarse cifrado en la base de datos PostgreSQL utilizando algoritmos estándar del sector (AES-256-GCM / DPAPI / Clave simétrica de entorno).

---

## 5. Criterios de Aceptación (DoD)

1. **Subida de Amazon CSV**:
   - El usuario puede subir un fichero CSV oficial de pedidos de Amazon.
   - El sistema almacena los pedidos e ítems sin errores y muestra el total de pedidos e importes procesados.
2. **Conexión PayPal API & CSV**:
   - El usuario puede importar un extracto CSV de actividad de PayPal o sincronizar vía API.
   - Los comercios subyacentes quedan normalizados y listos para reconciliar.
3. **Reconciliación 100% Precisa**:
   - Para un cargo `AMZN Mktp ES 45.90 €` que coincida con un pedido de 2 artículos (20 € y 25.90 €), el sistema sugiere o aplica el desglose en 2 partidas que suman exactamente 45.90 €.
   - Para un cargo `PAYPAL *STEAM 14.99 €`, el sistema asigna el comercio limpio `Steam Games` y la categoría `Videojuegos`.
4. **Cero Regresiones**:
   - Todas las pruebas unitarias y E2E de los Slices 001 y 002 siguen pasando al 100%.
