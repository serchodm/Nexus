# Nexus — Visión General del Sistema (System Vision & Context)

- **Documento**: Documento de Visión Macro y Contexto de Negocio
- **Nivel de Gobernanza**: Nivel 0 (Marco General de Referencia — No genera código directo)
- **Estado**: Aprobado
- **Última Actualización**: 2026-09-27

---

## 1. Visión y Definición del Producto

**Nexus** es una plataforma integral de **gestión financiera personal** y **orquestación de gastos compartidos**. El sistema converge en una única solución dos necesidades fundamentales del usuario: el seguimiento consolidado de sus finanzas personales (modelo PFM) y la administración colaborativa de partidas compartidas (modelo de balance de gastos entre personas).

---

## 2. Propósito Core

El objetivo central de Nexus es proporcionar al usuario **visibilidad total y centralizada de su salud financiera de un solo vistazo**, permitiéndole monitorear simultáneamente:
1. Su **patrimonio global** (ingresos, ahorros, inversiones y criptomonedas).
2. Su **flujo de gasto diario y mensual**.

---

## 3. Pilares Funcionales del Sistema

### Pilar 1: Ingesta de Datos y Extractos
- Capacidad de **subir e importar extractos periódicos** (mensuales) que contengan los movimientos y transacciones del usuario.
- Los movimientos se incorporan a la plataforma para servir de base a los balances y análisis financieros.

### Pilar 2: Consolidación Patrimonial Multiactivo
Visualización centralizada de los distintos componentes patrimoniales del usuario:
- **Ingresos y Gastos**: Flujo de caja corriente y operativa diaria.
- **Ahorros**: Fondos de reserva o cuentas de ahorro.
- **Inversiones**: Posiciones y cartera de inversión tradicional.
- **Criptomonedas**: Activos digitales en posesión del usuario.

### Pilar 3: Modelo Dual del Gasto (Individual vs. Compartido)
- Cada movimiento o gasto registrado podrá especificarse y gestionarse bajo dos modalidades:
  - **Gasto Individual**: Imputable únicamente al usuario para su control personal.
  - **Gasto Compartido**: Imputable a un grupo o conjunto de personas para el cálculo y liquidación de saldos compartidos.

### Pilar 4: Categorización Autónoma e Inteligente de Movimientos
- Asignación de categorías estructuradas a cada movimiento para permitir el desglose analítico del gasto.
- Clasificación autónoma mediante motor de reglas determinista, merchant intelligence y aprendizaje interactivo.
- Detección de naturaleza del gasto (Fijo / Recurrente vs. Variable / Discrecional) y soporte de desgloses de tickets (Split Transactions).

### Pilar 5: Conectores de Comercio y Pasarelas de Pago Online (Amazon, PayPal, etc.)
- Reconciliación cruzada entre cargos bancarios genéricos u opacos (ej. `AMZN MKTP ES`, `PAYPAL *[código]`) y el detalle real de pedidos de plataformas externas.
- Extracción de artículos específicos, cuotas de pagos a plazos (Buy Now Pay Later) y comercios subyacentes para una categorización precisa y enriquecida.

---

## 4. Límites y Filosofía de Evolución (SDD)

Este documento actúa como el **Norte Estratégico** del producto. 
A partir de estos cinco pilares, el desarrollo técnico se ejecutará mediante **especificaciones incrementales por Vertical Slice** (carpeta `specs/features/`), abordando cada capacidad de forma aislada, autocontenida y verificable antes de proceder a la siguiente.
