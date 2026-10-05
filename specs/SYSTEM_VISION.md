# Nexus — Visión General del Sistema (System Vision & Context)

- **Documento**: Documento de Visión Macro y Contexto de Negocio
- **Nivel de Gobernanza**: Nivel 0 (Marco General de Referencia — No genera código directo)
- **Estado**: Aprobado
- **Última Actualización**: 2026-10-03

---

## 1. Visión y Definición del Producto: El "Financial OS"

**Nexus** es un **Sistema Operativo Financiero (Financial OS)** integral diseñado para la gestión del patrimonio personal y la orquestación colaborativa de finanzas compartidas.

A diferencia de los neobancos transaccionales (Revolut), los brokers de inversión (Trade Republic, MyInvestor) o los agregadores obsoletos viciados por la colocación publicitaria de créditos y seguros (Fintonic), **Nexus no busca custodiar el dinero ni vender productos financieros**. Su propósito es situarse como la **capa superior e imparcial de inteligencia, gobernanza y orquestación** de todo el ecosistema patrimonial del usuario.

El sistema converge en una única plataforma dos realidades cotidianas que el software tradicional mantiene fragmentadas:
1. **El Seguimiento y Optimización Patrimonial Consolidado (PFM & Wealth Tracking)**.
2. **La Administración de Partidas Compartidas (Hogar, Pareja y Grupos)** con aislamiento estricto de privacidad.

---

## 2. Propósito Core y Principios de Diseño

El objetivo central de Nexus es otorgar al usuario **visibilidad y gobierno total de su salud financiera de un solo vistazo**, permitiéndole monitorear de forma simultánea:
- Su **Patrimonio Neto Real** ($\text{Activos} - \text{Pasivos}$).
- Su **Flujo de Caja Operativo** (ingresos, gastos recurrentes y discrecionales).
- La **Salud Financiera Cuantificada** mediante indicadores objetivos.
- Las **Decisiones Futuras** mediante simulación de escenarios de vida.

### 2.1. Principio Arquitectónico: Universal Core & Mobile-Ready
Nexus opera bajo un paradigma **100% API-First y agnóstico del cliente**:
- **Prohibición de Extensiones Propietarias**: Queda descartada cualquier dependencia de extensiones de navegador web, scripts DOM o tecnologías no portables a dispositivos móviles.
- **Paridad Multiplataforma Total**: Todas las capacidades (ingesta de extractos, sincronización de comercio, categorización y analítica patrimonial) operan con idéntica fidelidad en Web de escritorio y en apps móviles nativas (iOS / Android mediante Capacitor / PWA).
- **Formatos Abiertos y Conectores Universales**: Ingesta universal mediante APIs estándar abiertas o procesamiento directo de archivos estándar (CSV, XLSX, PDF, JSON).

---

## 3. Pilares Funcionales del Sistema

```text
                                  ┌────────────────────────────────────────┐
                                  │               NEXUS                    │
                                  │      (Financial Operating System)      │
                                  └──────────────────┬─────────────────────┘
                                                     │
         ┌───────────────────┬───────────────────────┼───────────────────────┬───────────────────┐
         ▼                   ▼                       ▼                       ▼                   ▼
   [PILAR 1 & 5]         [PILAR 2]               [PILAR 3]               [PILAR 4]           [PILAR 6 & 7]
 Ingesta Universal    Patrimonio Neto        Modelo Dual del Gasto    Categorización     Nexus Aura & What-If
 & Conectores         Multiactivo Real       & Espacio Compartido     & Splits Contables     Simulador Vida
 (Bancos, Amazon,     (Activos - Pasivos)    (Muro de Privacidad      (Merchant Normalizer, (Copiloto IA Imparcial,
  PayPal)                                     Zero-Knowledge)          Invariante 0.00 €)    Salud Financiera)
```

---

### Pilar 1: Ingesta Universal de Datos y Extractos
- Ingesta multiformato periódica (XLSX, CSV, PDF) de entidades bancarias (BBVA, Santander, ING, CaixaBank, Openbank, etc.).
- Detección determinista de duplicados mediante fingerprint criptográfico (`SHA256`).
- Previsualización atómica en memoria (`previewToken`) con detección previa de saltos cronológicos de saldo (`BALANCE_DISCONTINUITY`) o discrepancias de tarjetas (`CARD_MISMATCH`).
- Capacidad de deshacer importaciones erróneas (*batch rollback*) con restauración atómica de saldos.

---

### Pilar 2: Consolidación Patrimonial Multiactivo Real (Activos y Pasivos)
Nexus calcula y proyecta el **Patrimonio Neto Real** del usuario satisfaciendo la ecuación contable:
$$\text{Patrimonio Neto} = \sum \text{Activos Ponderados} - \sum \text{Pasivos Ponderados}$$

#### 2.1. Activos
- **Líquidos y Ahorro**: Cuentas corrientes operativas, depósitos y cuentas de ahorro remuneradas.
- **Inversión Financiera**: Fondos indexados, carteras gestionadas, ETFs, acciones y planes de pensiones.
- **Criptoactivos**: Activos digitales en custodia propia o exchanges.
- **Bienes Físicos / Ilíquidos**: Valoración de inmuebles (vivienda habitual, segundas residencias) y vehículos, con estimación de amortización o plusvalía.
- **Metas y Huchas de Ahorro**: Asignación de liquidez a objetivos concretos (*Fondo de Emergencia*, *Entrada Piso*, *Vacaciones*) con estimación automática de fecha de consecución según ritmo de ahorro real.

#### 2.2. Pasivos (Deudas)
- **Hipotecas**: Capital pendiente, tipo de interés (fijo/variable), cuota mensual y fecha de vencimiento.
- **Préstamos Personales / Auto**: Compromiso de deuda y amortización restante.
- **Deuda de Consumo**: Tarjetas de crédito revolving y líneas aplazadas.

#### 2.3. Ponderación por Cotitularidad
- Soporte de roles de titularidad (`SoleOwner`, `PrimaryJoint`, `SecondaryJoint`) con porcentaje explícito de participación ($0\%$ a $100\%$) para no distorsionar el patrimonio neto real en activos o deudas compartidas.

---

### Pilar 3: Modelo Dual del Gasto y Espacios Compartidos (Privacidad Asimétrica)
Nexus resuelve la fricción de las finanzas en pareja o de convivencia mediante un modelo de **doble espacio con muro de privacidad estricto**:

- **Espacio Individual (100% Privado / Soberanía Personal)**:
  - Salario, cuentas personales, inversiones, compras privadas y patrimonio particular son **totalmente invisibles** para otros miembros del grupo.
- **Espacio Compartido (Hogar, Pareja, Grupos de Gasto)**:
  - Fondo común de gastos y partidas imputadas explícitamente al espacio común.
  - Modelo de liquidación inteligente (*Fair-Share*): Reparto equitativo (50/50) o ponderado proporcionalmente a los ingresos de cada parte.
  - Balance de compensación dinámico (quién debe a quién) y registro de transferencias de liquidación sin necesidad de obligar a abrir cuentas bancarias conjuntas.

---

### Pilar 4: Categorización Autónoma, Merchant Intelligence y Desglose de Gastos
- Clasificación autónoma en árbol jerárquico de 2 niveles (Categoría Raíz y Subcategoría).
- **Merchant Intelligence**: Normalización de conceptos de TPV eliminando ruidos, códigos postales, fechas y prefijos de pasarelas (`SUMUP*`, `PAYPAL*`, `TPV`, `REDYS`).
- **Invariante Contable de Desglose (Split Transactions)**: Capacidad de dividir compras compuestas en múltiples partidas respetando estrictamente el balance cero (RN-02).
- Prevalencia absoluta de reglas de usuario sobre deducciones automáticas del sistema (RN-01).

---

### Pilar 5: Conectores de Comercio y Pasarelas de Pago Online (Amazon, PayPal)
- Reconciliación cruzada entre cargos bancarios opacos (ej. `AMZN Mktp ES`, `PAYPAL *EBAY`) y el detalle real de pedidos de plataformas externas.
- Ingesta de reportes oficiales de pedidos de Amazon (CSV) y extractos o APIs de PayPal.
- Motor de reconciliación temporal ($\pm 7$ días) y combinatoria de artículos (1:1 y 1:N), convirtiendo automáticamente compras complejas en *splits* clasificados con exactitud matemática.

---

### Pilar 6: Nexus Aura — Copiloto Financiero Proactivo e Inteligencia Patrimonial
**Nexus Aura** es el motor de inteligencia y asistente proactivo integrado en Nexus. A diferencia de chatbots genéricos, Aura opera exclusivamente sobre los datos verificados del usuario con explicabilidad algorítmica:

- **Auditoría de Ineficiencias Patrimoniales**:
  - Detección de dinero ocioso al 0% en cuentas corrientes que supera el fondo de emergencia óptimo.
  - Cálculo del coste de oportunidad frente a la inflación y opciones de rentabilidad pasiva.
- **Vigilancia de Suscripciones y Gastos Vampiro**:
  - Alerta de suscripciones duplicadas, recurrentes no utilizadas o incrementos repentinos de cuota.
- **Consultas Analíticas en Lenguaje Natural (NLQ)**:
  - Capacidad de responder preguntas complejas cruzando todas las entidades: *"¿Cuánto he gastado en suministros del hogar este año sumando mis pagos directos y los del espacio compartido?"*.
- **Independencia e Imparcialidad Absoluta (RN-08)**: Aura nunca recomienda productos de terceros a cambio de comisiones; responde con fidelidad matemática exclusiva al usuario.

---

### Pilar 7: Índice de Salud Financiera y Simulador de Vida ("What-If Engine")

#### 7.1. Índice de Salud Financiera (Nexus Health Score)
Indicador numérico unificado (0 a 100) compuesto por 4 métricas financieras objetivas:
1. **Cojín de Emergencia (Runway)**: Meses de gastos esenciales cubiertos con liquidez inmediata (objetivo saludable: 3–6 meses).
2. **Ratio de Endeudamiento (Debt-to-Income / DTI)**: Porcentaje del ingreso mensual neto absorbido por el pago de deudas (objetivo saludable $< 30\%$).
3. **Tasa de Ahorro / Inversión Mensual**: Porcentaje del flujo mensual retenido para acumulación patrimonial (objetivo saludable $> 20\%$).
4. **Diversificación Patrimonial**: Ausencia de concentración excesiva en un único activo o plataforma.

#### 7.2. Simulador de Escenarios de Vida (Motor "What-If")
Herramienta de proyección que simula el impacto patrimonial de decisiones vitales antes de ejecutarlas:
- **Compra de Vehículo**: Comparativa de descapitalización al contado vs. coste total de financiación.
- **Adquisición de Vivienda / Hipoteca**: Impacto en la liquidez disponible, ratio de endeudamiento y proyección de patrimonio neto a 5, 10 y 25 años.
- **Escenario de Estrés (Pérdida de Ingresos / Cambio Profesional)**: Cálculo del tiempo exacto de supervivencia financiera según los gastos fijos actuales y el fondo de emergencia.

---

## 4. Invariantes de Negocio y Seguridad

### 4.1. Invariante de Balance Cero (RN-02)
Toda división o desglose contable (*Split Transaction* o reconciliación de compras) debe satisfacer una **discrepancia matemática estricta de $0.00\ €$**:
$$\sum \text{Partidas} = \text{Importe Total de la Transacción}$$
Queda terminantemente prohibido cualquier redondeo permisivo o tolerancia de descuadre en persistencia.

### 4.2. Prevalencia de la Soberanía del Usuario (RN-01)
Cualquier transacción, categoría, desglose o ajuste verificado o editado manualmente por el usuario (`IsManual = true`) es **inviolable e inmutable** ante automatismos del sistema, motores heurísticos o ejecuciones retroactivas de reglas.

### 4.3. Cifrado en Reposo y Soberanía de Datos
- **Cifrado Simétrico Fuerte (AES-256-GCM)**: Obligatorio para cualquier credencial, clave secreta o token de pasarelas externas almacenado en base de datos.
- **Enmascaramiento de Datos Financieros**: Los números de tarjeta de crédito/débito (PAN) deben enmascararse irreversiblemente en la ingesta (`**** **** **** 1234`).
- **Sanitización de Privacidad**: Eliminación de datos postales personales y domicilios que no aportan valor financiero.

### 4.4. Explicabilidad y Determinismo de la Inteligencia
Toda deducción realizada por el sistema (clasificación de comercios, detección de gastos fijos o propuestas de reconciliación) debe ser **explicable**, informando al usuario el nivel de confianza (`ConfidenceScore`) y la razón del emparejamiento (`MatchReason`).

### 4.5. Muro de Privacidad en Espacios Compartidos (RN-07)
En el modelo colaborativo, el espacio individual de un usuario es hermético: ninguna transacción, cuenta, saldo o inversión personal es accesible para los demás integrantes del grupo sin una asignación explícita e inequívoca al espacio compartido.

### 4.6. Imparcialidad Algorítmica de Nexus Aura (RN-08)
El copiloto Nexus Aura se rige por una política de cero conflictos de interés. Queda prohibida la recomendación sesgada de productos bancarios, créditos o seguros patrocinados por terceros.

---

## 5. Límites y Filosofía de Evolución (SDD)

Este documento actúa como el **Norte Estratégico** del producto. 
A partir de estos siete pilares e invariantes, el desarrollo técnico se ejecutará mediante **especificaciones incrementales por Vertical Slice** (carpeta `specs/features/`), abordando cada capacidad de forma aislada, autocontenida y verificable antes de proceder a la siguiente.
