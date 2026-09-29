# Nexus — Visión General del Sistema (System Vision & Context)

- **Documento**: Documento de Visión Macro y Contexto de Negocio
- **Nivel de Gobernanza**: Nivel 0 (Marco General de Referencia — No genera código directo)
- **Estado**: Aprobado
- **Última Actualización**: 2026-09-29

---

## 1. Visión y Definición del Producto

**Nexus** es una plataforma integral de **gestión financiera personal** y **orquestación de gastos compartidos**. El sistema converge en una única solución dos necesidades fundamentales del usuario: el seguimiento consolidado de sus finanzas personales (modelo PFM) y la administración colaborativa de partidas compartidas (modelo de balance de gastos entre personas).

---

## 2. Propósito Core

El objetivo central de Nexus es proporcionar al usuario **visibilidad total y centralizada de su salud financiera de un solo vistazo**, permitiéndole monitorear simultáneamente:
1. Su **patrimonio global** (ingresos, ahorros, inversiones y criptomonedas).
2. Su **flujo de gasto diario y mensual**.

### 2.1. Principio Arquitectónico Fundamental: Universal Core & Mobile-Ready
Nexus se diseña bajo un paradigma **100% API-First y agnóstico del cliente**:
- **Prohibición de Extensiones Propietarias**: Queda expresamente descartada cualquier solución técnica que dependa de extensiones de navegador web, scripts de inyección en el DOM o capacidades no portables a dispositivos móviles.
- **Paridad Multiplataforma Total**: Cualquier capacidad del sistema (subida de extractos bancarios, ingesta de pedidos de Amazon o PayPal, categorización interactiva y reconciliación contable) debe operar con idéntica fidelidad tanto en la aplicación Web de escritorio como en la futura aplicación móvil nativa (iOS / Android / PWA).
- **Formatos Abiertos y Conectores Universales**: Todas las integraciones con terceros se basan en APIs estándar abiertas o en la ingesta directa de reportes y extractos en formatos universales (CSV, XLSX, PDF, JSON).

---

## 3. Pilares Funcionales del Sistema

### Pilar 1: Ingesta de Datos y Extractos
- Capacidad de **subir e importar extractos periódicos** (mensuales) multiformato (XLSX, CSV, PDF) de las principales entidades bancarias (BBVA, Santander, ING, etc.).
- Detección canónica de duplicados mediante fingerprint criptográfico (`SHA256`) y soporte de transacciones de previsualización atómicas sin persistencia previa.
- Capacidad de deshacer importaciones erróneas (*batch rollback*) con restauración de saldos.

### Pilar 2: Consolidación Patrimonial Multiactivo
Visualización centralizada de los distintos componentes patrimoniales del usuario:
- **Ingresos y Gastos**: Flujo de caja corriente y operativa diaria.
- **Ahorros**: Fondos de reserva o cuentas de ahorro remuneradas.
- **Inversiones**: Posiciones y cartera de inversión tradicional (fondos indexados, acciones).
- **Criptomonedas**: Activos digitales en posesión del usuario.
- **Ponderación de Cotitularidad**: Cálculo del patrimonio neto ponderado según el porcentaje de participación del usuario en cada cuenta o depósito.

### Pilar 3: Modelo Dual del Gasto (Individual vs. Compartido)
- Cada movimiento o gasto registrado podrá especificarse y gestionarse bajo dos modalidades:
  - **Gasto Individual**: Imputable únicamente al usuario para su control personal.
  - **Gasto Compartido**: Imputable a un grupo o conjunto de personas para el cálculo y liquidación de saldos compartidos (modelo *Fair-Share* y balances de compensación).

### Pilar 4: Categorización Autónoma e Inteligente de Movimientos
- Clasificación autónoma en dos niveles (Categoría Raíz y Subcategoría) mediante motor determinista de reglas, normalización de comercios (*Merchant Intelligence*) y detección de recurrencia/suscripciones.
- Soporte nativo de desgloses de tickets y compras compuestas (**Split Transactions**) respetando la integridad contable.
- Creación de reglas de usuario automáticas tras corrección manual con capacidad de re-evaluación retroactiva.

### Pilar 5: Conectores de Comercio y Pasarelas de Pago Online (Amazon, PayPal, etc.)
- Reconciliación cruzada entre cargos bancarios genéricos u opacos (ej. `AMZN Mktp ES`, `PAYPAL *[código]`) y el detalle real de pedidos de plataformas externas.
- Ingesta de reportes de pedidos oficiales de Amazon (CSV/ZIP) y extractos o APIs de PayPal.
- Motor de reconciliación temporal ($\pm 7$ días) y combinatoria de artículos (1:1 y 1:N), convirtiendo automáticamente compras complejas en *splits* clasificados con exactitud matemática.

---

## 4. Invariantes de Negocio y Seguridad

### 4.1. Invariante de Balance Cero (RN-02)
- Toda división o desglose contable (*Split Transaction* o reconciliación de compras) debe satisfacer una **discrepancia matemática estricta de $0.00\ €$**:
  $$\sum \text{Partidas} = \text{Importe Total de la Transacción}$$
- Queda terminantemente prohibido cualquier redondeo permisivo o tolerancia de descuadre en persistencia.

### 4.2. Prevalencia de la Soberanía del Usuario (RN-01)
- Cualquier transacción, categoría, desglose o ajuste verificado o editado manualmente por el usuario (`IsManual = true`) es **inviolable e inmutable** ante automatismos del sistema, motores heurísticos o ejecuciones retroactivas de reglas.
- Ningún algoritmo puede sobreescribir la voluntad explícita del usuario sin su confirmación directa.

### 4.3. Cifrado en Reposo y Soberanía de Datos
- **Cifrado Simétrico Fuerte (AES-256-GCM)**: Obligatorio para cualquier credencial, clave secreta o token de pasarelas externas almacenado en base de datos.
- **Enmascaramiento de Datos Financieros**: Los números de tarjeta de crédito/débito (PAN) deben enmascararse irreversiblemente en la ingesta (`**** **** **** 1234`).
- **Sanitización de Privacidad**: Eliminación de datos postales personales y domicilios que no aportan valor financiero.

### 4.4. Explicabilidad y Determinismo de la Inteligencia
- Toda deducción realizada por el sistema (clasificación de comercios, detección de gastos fijos o propuestas de reconciliación) debe ser **explicable**, informando al usuario el nivel de confianza (`ConfidenceScore`) y la razón del emparejamiento (`MatchReason`).

---

## 5. Límites y Filosofía de Evolución (SDD)

Este documento actúa como el **Norte Estratégico** del producto. 
A partir de estos cinco pilares e invariantes, el desarrollo técnico se ejecutará mediante **especificaciones incrementales por Vertical Slice** (carpeta `specs/features/`), abordando cada capacidad de forma aislada, autocontenida y verificable antes de proceder a la siguiente.
