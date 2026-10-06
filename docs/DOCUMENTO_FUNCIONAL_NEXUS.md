# Nexus — Documento Funcional Integral (Master Functional Specification)

- **Producto**: **Nexus — Financial Operating System & Shared Ledger**
- **Nivel de Gobernanza**: Especificación Maestra de Producto y Dominio
- **Autor y Especialista de Dominio**: **`@warren`** (Oráculo Financiero de Nexus)
- **Coordinador Técnico**: **`@jarvis`**
- **Estado**: Aprobado / Versión Definitiva de Referencia
- **Fecha**: 2026-10-06

---

## Tabla de Contenidos
1. [Objetivos Estratégicos del Proyecto](#1-objetivos-estratégicos-del-proyecto)
   - 1.1. Misión y Definición de Producto
   - 1.2. La Estrategia del "Océano Azul" (Nexus vs. El Mercado Tradicional)
   - 1.3. Principios Inviolables de Dominio
2. [Alcance Global del Sistema](#2-alcance-global-del-sistema)
   - 2.1. Dentro del Alcance (In-Scope)
   - 2.2. Fuera del Alcance (Out-of-Scope)
3. [Pilares y Funcionalidades con Diagramas de Flujo](#3-pilares-y-funcionalidades-con-diagramas-de-flujo)
   - 3.1. Ingesta Universal y Open Banking (Enable Banking)
   - 3.2. Motor de Transacciones, Normalización y Clasificación de Bizum
   - 3.3. Reconciliación de E-Commerce (Amazon y PayPal)
   - 3.4. Espacio Compartido, Reparto Fair-Share y Muro de Privacidad
   - 3.5. Consolidación Patrimonial Multiactivo (Net Worth Real)
   - 3.6. Nexus Aura, Índice de Salud Financiera y Simulador "What-If"
4. [Mocks de Pantallas y Experiencia de Usuario (UI/UX)](#4-mocks-de-pantallas-y-experiencia-de-usuario-uiux)
   - 4.1. Mock 1: Dashboard Principal (Visión Consolidada)
   - 4.2. Mock 2: Transacciones, Filtros y Drawer de Split Contable
   - 4.3. Mock 3: Espacio Compartido (Pareja/Hogar), Fair-Share y Settle Up Bizum
   - 4.4. Mock 4: Hub de Conexión Bancaria (Open Banking & Extractos)
   - 4.5. Mock 5: Simulador de Vida "What-If" y Nexus Health Score
5. [Invariantes Matemáticas y Modelo de Datos](#5-invariantes-matemáticas-y-modelo-de-datos)
6. [Estrategia Multiplataforma (Web y Móvil con Capacitor)](#6-estrategia-multiplataforma-web-y-móvil-con-capacitor)

---

## 1. Objetivos Estratégicos del Proyecto

### 1.1. Misión y Definición de Producto
**Nexus** es un **Sistema Operativo Financiero (Financial OS)** integral diseñado para devolver al usuario la soberanía, visibilidad absoluta y gobierno de su patrimonio personal y sus finanzas compartidas.

A diferencia de los bancos tradicionales o neobancos transaccionales (que operan en silos cerrados) y de las plataformas PFM obsoletas (que monetizan vendiendo créditos o seguros de terceros), **Nexus no custodia dinero ni coloca productos comerciales**. Su propósito es constituirse como la **capa superior e imparcial de inteligencia patrimonial**, orquestando y unificando en una experiencia única:
1. El **seguimiento patrimonial multiactivo real** (cuentas, fondos, inmuebles, hipotecas, criptomonedas).
2. El **flujo de caja operativo** con categorización autónoma y desgloses contables exactos.
3. La **gestión compartida del hogar y pareja** con conciliación automática de deudas y aislamiento de privacidad.

```mermaid
flowchart TD
    subgraph Silos ["Ecosistema Financiero Fragmentado"]
        B1["Bancos Tradicionales<br/>(BBVA, ING, Santander)"]
        B2["Plataformas de Comercio<br/>(Amazon, PayPal)"]
        B3["Inversiones & Cripto<br/>(Indexados, Wallets)"]
        B4["Gastos del Hogar<br/>(Bizum, Splitwise)"]
    end

    B1 --> NexusOS["🏛️ NEXUS Financial OS<br/>(Inteligencia, Privacidad & Automatización)"]
    B2 --> NexusOS
    B3 --> NexusOS
    B4 --> NexusOS

    NexusOS --> Out1["📊 Patrimonio Neto Real"]
    NexusOS --> Out2["🤝 Finanzas en Pareja Fair-Share"]
    NexusOS --> Out3["🔮 Simulador de Vida What-If"]
```

---

### 1.2. La Estrategia del "Océano Azul" (Nexus vs. El Mercado Tradicional)

La industria fintech ha obligado históricamente al usuario a fragmentar su vida financiera en aplicaciones incompatibles:
- **PFM Tradicional (Fintonic, Copilot, Monarch)**: Tienen una perspectiva 100% individual y ciega. Si pagas una cena de $120\text{ \euro}$ entre amigos, registran un gasto de $-120\text{ \euro}$ que distorsiona tu presupuesto mensual. Cuando tus amigos te pagan por Bizum, la app lo computa erróneamente como un "nuevo ingreso", inflando artificialmente tu ahorro.
- **Gestores de Gastos en Grupo (Splitwise, Tricount)**: Son libretas de notas manuales desconectadas de los bancos. El usuario tiene que acordarse de teclear los gastos a mano, las deudas se olvidan y no hay conexión con la tesorería real.

#### La Solución Disruptiva de Nexus:
Nexus fusiona ambos universos creando un **Ledger Financiero Unificado**:
1. **Del movimiento bancario al Split en 1 tap**: Al pagar un gasto común, el importe correspondiente a la otra persona se reclasifica inmediatamente como un **Activo a Corto Plazo (Cuentas por Cobrar / Receivables)** en tu balance patrimonial, reflejando tu gasto neto real.
2. **Conciliación pasiva de Bizums**: Cuando recibes el Bizum de liquidación, Nexus lo casa contra la deuda pendiente, amortizándola a $0.00\text{ \euro}$ sin computarlo jamás como ingreso salarial.
3. **Muro de Privacidad Zero-Knowledge**: Permite compartir los gastos de convivencia de forma transparente sin que tu pareja o compañeros vean tu nómina, tus cuentas privadas ni tus inversiones personales.
4. **Independencia absoluta**: Sin publicidad, sin venta de datos y sin colocación sesgada de créditos.

---

### 1.3. Principios Inviolables de Dominio
- **RN-01 Prevalencia de la Soberanía del Usuario**: Cualquier edición manual realizada por el usuario (`IsManual = true`) tiene prioridad jerárquica absoluta sobre cualquier deducción automática, regla o sincronización posterior.
- **RN-02 Invariante de Balance Cero**: En cualquier división contable (*split*) o reparto compartido, la suma de las partidas hijas debe igualar con precisión de $0.00\text{ \euro}$ al importe original. Prohibidos los redondeos con céntimos huérfanos.
- **RN-03 Pureza Aritmética**: Todas las operaciones monetarias se calculan en tipo `decimal` mediante *Banker's Rounding* (`MidpointRounding.ToEven`).
- **RN-04 Cero Conocimiento de Credenciales**: Nexus nunca solicita, procesa ni almacena contraseñas o PINs de banca online.

---

## 2. Alcance Global del Sistema

```mermaid
quadrantChart
    title Matriz de Alcance Estratégico de Nexus
    x-axis "Baja Automatización" --> "Alta Automatización"
    y-axis "Bajo Valor Añadido" --> "Alto Valor Añadido Estratégico"
    quadrant-1 "Núcleo de Nexus (In-Scope)"
    quadrant-2 "Diferenciadores Océano Azul"
    quadrant-3 "Fuera de Alcance (Out-of-Scope)"
    quadrant-4 "Comodities Secundarias"
    "Open Banking Enable Banking": [0.85, 0.88]
    "Modelo Dual Fair-Share Pareja": [0.78, 0.95]
    "Conciliación Bizum & Amazon": [0.82, 0.90]
    "Simulador Vida What-If": [0.65, 0.85]
    "Patrimonio Neto Multiactivo": [0.70, 0.80]
    "Categorización & Splits": [0.75, 0.72]
    "Importación Manual de Ficheros": [0.25, 0.55]
    "Custodia Directa de Fondos": [0.10, 0.15]
    "Iniciación de Transferencias (PISP)": [0.45, 0.30]
    "Venta de Seguros/Préstamos": [0.20, 0.05]
    "Trading Bursátil Automático": [0.90, 0.25]
```

### 2.1. Dentro del Alcance (In-Scope)
1. **Ingesta Multicanal**:
   - Conexión desatendida mediante **Open Banking AISP (Enable Banking)** con consentimiento de 180 días.
   - Procesamiento de extractos tabulares históricos (CSV, XLSX) de bancos españoles (BBVA, ING, Santander...).
   - Conectores de compras detalladas de **Amazon** (Order Reports CSV) y pasarelas **PayPal**.
2. **Motor de Transacciones e Inteligencia**:
   - Deduplicación determinista basada en huella criptográfica SHA-256 e ID bancario externo.
   - Normalización de conceptos comerciales y limpieza de prefijos de TPV (`MerchantNormalizer`).
   - Clasificación inteligente de conceptos Bizum (`BizumConceptClassifier`).
   - Motor jerárquico de categorías y reglas definidas por el usuario.
   - *Split Engine* con desglose de tickets mixtos a balance cero.
3. **Espacios Compartidos y Finanzas en Pareja**:
   - Espacio Privado individual + Espacio Compartido colaborativo.
   - Algoritmo de reparto **Fair-Share** (50/50 o proporcional a ingresos declarados, ej. 55/45).
   - Cálculo dinámico del balance bilateral consolidado.
   - Liquidación en un tap vía Bizum con reseteo de deuda a $0.00\text{ \euro}$.
4. **Patrimonio Neto Multiactivo (Wealth Tracking)**:
   - Consolidación de cuentas corrientes, depósitos, inversiones y planes de pensiones.
   - Módulo inmobiliario y vehículos con amortización.
   - Criptoactivos con valoración spot y trazabilidad fiscal de plusvalías/minusvalías bajo criterio FIFO.
   - Pasivos: Hipotecas (capital, cuota, amortización) y préstamos personales.
   - Ponderación porcentual por cotitularidad ($0\%$ a $100\%$).
5. **Nexus Aura & Simulador "What-If"**:
   - Auditoría de ineficiencias (dinero ocioso sin remunerar, gastos vampiro, suscripciones duplicadas).
   - Índice de Salud Financiera (**Nexus Health Score** de 0 a 100) ponderando Runway, DTI, Ahorro y Diversificación.
   - Simulador de escenarios de vida: compra de vivienda, compra de coche o pérdida temporal de ingresos.
6. **Multiplataforma Web y Móvil**:
   - Aplicación web responsive (React 19 + TypeScript).
   - App móvil nativa para iOS y Android mediante Capacitor con soporte para Deep Links seguros.

### 2.2. Fuera del Alcance (Out-of-Scope)
- **Custodia o captación de depósitos**: Nexus nunca mantiene saldos ni actúa como banco custodio.
- **Iniciación de Pagos salientes (PISP)**: Nexus no emite transferencias ni mueve dinero desde las cuentas bancarias del usuario.
- **Ejecución automática de órdenes bursátiles o trading cripto**: El sistema realiza seguimiento y auditoría, pero no cursa órdenes de compra/venta en brokers.
- **Monetización publicitaria de créditos**: Cero colocación de préstamos comisionados o seguros de terceros.
- **Extensiones de navegador propietarias**: Queda excluida cualquier solución que no sea 100% portable a móviles.

---

## 3. Pilares y Funcionalidades con Diagramas de Flujo

### 3.1. Ingesta Universal y Open Banking (Enable Banking)

Permite la sincronización bancaria automática sin licencia PSD2 propia mediante la integración con la API de Enable Banking.

```mermaid
flowchart TD
    Inicio([Usuario pulsa 'Conectar Banco']) --> Select[Selecciona entidad: BBVA, ING, Santander...]
    Select --> CreateSession[Nexus genera sesión en Enable Banking]
    CreateSession --> RedirectBank[Redirección a Portal/App Oficial del Banco]
    RedirectBank --> BankSCA{Usuario aprueba 2FA en su Banco?}
    BankSCA -- No / Cancelado --> Abort[Cancelación de sesión]
    BankSCA -- Sí --> Callback[Retorno a Nexus Callback URL]
    Callback --> DiscoverAccounts[Enable Banking entrega Account IDs e IBANs]
    DiscoverAccounts --> SaveConn[Persistencia en DB: BankConnection + Consentimiento 180d]
    SaveConn --> WorkerLoop[[Worker Periódico cada 12h]]
    WorkerLoop --> FetchTx[Descarga transacciones recientes]
    FetchTx --> Deduplicate{¿Existe Fingerprint o External ID?}
    Deduplicate -- Sí --> Skip[Ignorar duplicado / Actualizar estado]
    Deduplicate -- No --> Pipeline[Canalizar a Categorización & Reglas]
```

---

### 3.2. Motor de Transacciones, Normalización y Clasificación de Bizum

Convierte extractos crudos e incomprensibles en registros contables limpios y comprensibles:

```mermaid
flowchart LR
    Raw["Texto Crudo de Banco:<br/>'COMPRA TPV 28001 MERCADONA 12,50'"] --> Sanitizer[TextSanitizer & Normalizer]
    Sanitizer --> CleanMerchant["Comercio: 'Mercadona'<br/>Ubicación: 'Madrid'<br/>Tarjeta: '**** 1234'"]
    CleanMerchant --> RulesCheck{¿Coincide con Regla de Usuario?}
    RulesCheck -- Sí --> UserCat["Categoría: Alimentación (Prioridad RN-01)"]
    RulesCheck -- No --> EngineCat["Motor Heurístico: Supermercado"]
    
    BizumRaw["'BIZUM CENA CUMPLE CARLOS +25,00'"] --> Classifier[BizumConceptClassifier]
    Classifier --> ClassifyType["Tipo: Abono Social (Posible Liquidación)"]
    ClassifyType --> DebtMatch{¿Existe Deuda Compartida Pendiente?}
    DebtMatch -- Sí --> SettlePrompt["Sugerir compensación de saldo a 0.00 €"]
    DebtMatch -- No --> IngresoPuntual["Clasificar como Ingreso / Reembolso"]
```

---

### 3.3. Reconciliación de E-Commerce (Amazon y PayPal)

Resuelve el problema de los cargos opacos en la cuenta corriente (ej. `AMZN Mktp ES 84,30 €`) cruzándolos con los extractos detallados de las tiendas:

```mermaid
sequenceDiagram
    participant Bank as Cuenta Bancaria
    participant Engine as Nexus Reconciliation Engine
    participant Amazon as Reporte de Pedidos Amazon (CSV)

    Bank->>Engine: Cargo bancario: 84.30 € (Fecha: 15/10)
    Amazon->>Engine: Pedido #123 (Total: 84.30 €)
    Note over Engine: Ventana de tolerancia: ±7 días
    Engine->>Engine: Matching 1:1 o Combinatorio (1:N)
    Note over Engine: Desglose en artículos:<br/>- 1x Libro Finanzas (24.30 €) -> Educación<br/>- 1x Cafetera Italiana (60.00 €) -> Hogar
    Engine->>Bank: Convierte el cargo en un Split de 2 partidas exactas
    Note over Engine: Invariante RN-02: 24.30 € + 60.00 € = 84.30 € (0.00 € residuo)
```

---

### 3.4. Espacio Compartido, Reparto Fair-Share y Muro de Privacidad

La joya del Océano Azul de Nexus. Permite la convivencia financiera sin invadir la intimidad de ninguna de las partes.

```mermaid
flowchart TD
    GastoBancario["Pago Bancario:<br/>'IKEA MOBILIARIO' 240,00 € (Pagado por Sergio)"] --> Drawer[Abrir Drawer de Imputación Compartida]
    Drawer --> ConfigRatio{Selección de Reparto}
    ConfigRatio -- 50/50 --> Split1["Sergio: 120,00 € | Elena: 120,00 €"]
    ConfigRatio -- "Fair-Share (55/45)" --> Split2["Sergio (55%): 132,00 €<br/>Elena (45%): 108,00 €"]
    
    Split2 --> Contabilidad["Contabilidad de Sergio:<br/>• Gasto Real Hogar: 132,00 €<br/>• Activo/Receivable (Elena me debe): 108,00 €"]
    Contabilidad --> Ledger["Shared Ledger:<br/>Balance Neto: 'Elena debe 108,00 € a Sergio'"]
    
    Ledger --> SettleEvent[Elena hace clic en 'Saldar vía Bizum']
    SettleEvent --> BizumTrigger[Se genera referencia Bizum: 'NEXUS-SETTLE-108']
    BizumTrigger --> OffsetDebt[Compensación automática: Balance extinto a 0.00 €]
```

---

### 3.5. Consolidación Patrimonial Multiactivo (Net Worth Real)

Calcula el patrimonio neto exacto del usuario integrando la fórmula contable:
$$\text{Patrimonio Neto Real} = \sum (\text{Activos} \times \text{Participación}) - \sum (\text{Pasivos} \times \text{Participación})$$

```mermaid
graph LR
    subgraph Activos ["Activos Ponderados"]
        A1["Cuentas y Depósitos (100%)"]
        A2["Fondos & Acciones (100%)"]
        A3["Inmueble Habitual (50% con pareja)"]
        A4["Criptomonedas (FIFO / Spot)"]
        A5["Deudas por Cobrar (Receivables Espacio Compartido)"]
    end

    subgraph Pasivos ["Pasivos Ponderados"]
        P1["Hipoteca Vivienda (50% con pareja)"]
        P2["Préstamo Personal (100%)"]
        P3["Tarjetas Crédito Aplazado"]
    end

    Activos --> SumA[Total Activos: 285.400 €]
    Pasivos --> SumP[Total Pasivos: 112.000 €]

    SumA --> NetWorth["🏛️ PATRIMONIO NETO REAL<br/>173.400 €"]
    SumP --> NetWorth
```

---

### 3.6. Nexus Aura, Índice de Salud Financiera y Simulador "What-If"

Aura calcula el **Nexus Health Score (0 a 100)** y simula decisiones vitales de alto impacto:

```mermaid
flowchart TD
    Data[Datos Financieros Verificados de Nexus] --> HealthEngine[Cálculo de Health Score]
    HealthEngine --> P1["1. Runway (Meses colchón >= 6 meses)"]
    HealthEngine --> P2["2. DTI (Endeudamiento < 30%)"]
    HealthEngine --> P3["3. Tasa de Ahorro (> 20% mensual)"]
    HealthEngine --> P4["4. Diversificación Multiactivo"]
    P1 & P2 & P3 & P4 --> ScoreFinal["Puntuación: 84 / 100 (Salud Excelente)"]

    Data --> WhatIf[Motor Simulador 'What-If']
    WhatIf --> SimChoice{Elegir Escenario}
    SimChoice -- Compra Vivienda --> HypoCalc["Calcula: Entrada, Nueva Hipoteca, Caída de Liquidez y DTI a 25 años"]
    SimChoice -- Compra Coche --> CarCalc["Calcula: Descapitalización vs. Financiación TAE"]
    SimChoice -- Pérdida de Ingresos --> StressCalc["Calcula: Tiempo exacto de supervivencia antes de agotar liquidez"]
```

---

## 4. Mocks de Pantallas y Experiencia de Usuario (UI/UX)

A continuación se presentan los esquemas visuales (*wireframes*) interactivos de las pantallas principales del sistema, detallando los elementos y comportamientos de cada una.

---

### 4.1. Mock 1: Dashboard Principal (Visión Consolidada)

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│  [NEXUS LOGO]   [Dashboard]  [Transacciones]  [Espacio Compartido]  [Patrimonio]   (EUR ▼) [👤 Sergio]│
├──────────────────────────────────────────────────────────────────────────────────────────────────┤
│  PANEL PATRIMONIAL CONSOLIDADO                                         [🔄 Sincronizar Bancos]    │
│                                                                                                  │
│  ┌────────────────────────┐ ┌────────────────────────┐ ┌───────────────────────┐ ┌───────────────┐ │
│  │ PATRIMONIO NETO REAL   │ │ FLUJO DE CAJA (OCT)    │ │ RUNWAY DE LIQUIDEZ    │ │ HEALTH SCORE  │ │
│  │ 173.450,20 €           │ │ +1.420,50 €            │ │ 8,4 Meses             │ │ 84 / 100      │ │
│  │ ▲ +2,4% vs mes anterior│ │ Ing: 3.800€ | Gasto:2.380│ │ Objetivo: > 6 meses   │ │ Excelente ⭐  │ │
│  └────────────────────────┘ └────────────────────────┘ └───────────────────────┘ └───────────────┘ │
│                                                                                                  │
│  ┌───────────────────────────────────────────────────┐ ┌────────────────────────────────────────┐ │
│  │ EVOLUCIÓN HISTÓRICA DEL PATRIMONIO (12 MESES)      │ │ NEXUS AURA — INSIGHTS PROACTIVOS       │ │
│  │                                                   │ │                                        │ │
│  │  200k ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─╭───────────│ │ 💡 Tienes 14.200 € al 0% en cuenta   │ │
│  │  150k ─ ─ ─ ─ ─ ─ ─ ─ ─ ─╭───────╭─────╯  Activos │ │    corriente. Supera tu runway de      │ │
│  │  100k ─ ─ ─ ─ ─ ─ ─╭─────╯───────╯                │ │    seguridad. Mover 8.000 € a cuenta   │ │
│  │   50k ─ ─ ─ ─ ─ ─ ─╰───────────────────── Pasivos │ │    remunerada generaría ~280 €/año.    │ │
│  │       Ene Feb Mar Abr May Jun Jul Ago Sep Oct     │ │                                        │ │
│  │       [Ver desglose detallado de activos ➔]       │ │ ⚠️ Suscripción Netflix subió de 12 a 17€│ │
│  └───────────────────────────────────────────────────┘ └────────────────────────────────────────┘ │
│                                                                                                  │
│  ┌─────────────────────────────────────────────────────────────────────────────────────────────┐ │
│  │ ESPACIO COMPARTIDO (SERGIO & ELENA)               [Reparto Fair-Share: 55/45]  [Saldar Bizum]│ │
│  │ Elena te debe actualmente:  170,61 €              Última liquidación: 30 Sep (0,00 €)        │ │
│  └─────────────────────────────────────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Explicación de Componentes del Dashboard:
1. **Top Bar con Balance de Pareja**: Muestra el selector de divisa base, el widget de estado bilateral rápido y el botón global de refresco.
2. **KPI Cards Superiores**: Cuatro indicadores objetivos inmutables: Patrimonio Neto Real, Flujo de Caja mensual, Runway de supervivencia ante imprevistos y el Nexus Health Score.
3. **Gráfico Evolutivo de Patrimonio**: Curvas desacopladas de Activos totales vs. Pasivos exigibles.
4. **Nexus Aura Feed**: Tarjetas de diagnóstico proactivo libres de publicidad (coste de oportunidad de efectivo ocioso, detección de incrementos de suscripción).
5. **Widget Rápido de Finanzas Compartidas**: Resumen del balance bilateral con acceso directo al modal de liquidación.

---

### 4.2. Mock 2: Transacciones, Filtros y Drawer de Split Contable

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│  TRANSACCIONES & MOVIMIENTOS                             [🔍 Buscar concepto...] [Filtrar ▼]     │
├──────────────────────────────────────────────────────────────────────────────────────────────────┤
│  [Todos los bancos ▼]  [Todas las categorías ▼]  [Estado: Todos ▼]         [+ Importar Extracto] │
├──────────────────────────────────────────────────────────────────────────────────────────────────┤
│  FECHA      CONCEPTO / COMERCIO          BANCO      CATEGORÍA          IMPORTE      ESTADO       │
│  ─────────  ───────────────────────────  ─────────  ─────────────────  ───────────  ───────────  │
│  06/10/2026 MERCADONA MADRID             BBVA       Supermercado       -68,40 €     [Booked]     │
│  05/10/2026 IBERDROLA SUMINISTROS        ING Direct Hogar / Luz        -110,61 €    [Shared 55%] │
│  04/10/2026 AMAZON EU (3 artículos)      BBVA       [Split Dividido]   -84,30 €     [Reconciled] │
│  03/10/2026 BIZUM DE ELENA               BBVA       Settle Up Pareja   +140,00 €    [Bizum Match]│
│  02/10/2026 NÓMINA EMPRESA               BBVA       Ingreso Salarial   +2.850,00 €  [Verified]   │
└──────────────────────────────────────────────────────────────────────────────────────────────────┘

                       ┌──────────────────────────────────────────────────┐
                       │ DRAWER: SPLIT DE TRANSACCIÓN COMPUESTA            │
                       │ Original: AMAZON EU — Total: -84,30 €            │
                       ├──────────────────────────────────────────────────┤
                       │ Invariante Contable: 84,30 € = 84,30 € (Dif: 0€) │
                       │                                                  │
                       │ Partida 1:                                       │
                       │ Concepto: Libro Clean Code Arquitectura         │
                       │ Importe:  24,30 €   Categoría: [Formación ▼]     │
                       │                                                  │
                       │ Partida 2:                                       │
                       │ Concepto: Filtro Purificador de Aire Hogar       │
                       │ Importe:  60,00 €   Categoría: [Hogar ▼]         │
                       │ [x] Imputar al Espacio Compartido (Elena)        │
                       │                                                  │
                       │ [ + Añadir otra partida ]                        │
                       │                                                  │
                       │ [Cancelar]                [Guardar Split Exacto] │
                       └──────────────────────────────────────────────────┘
```

#### Explicación de Componentes de Transacciones:
1. **Badges de Inteligencia Transaccional**:
   - `[Booked]`: Transacción bancaria firme y confirmada.
   - `[Shared 55%]`: Transacción imputada al espacio colaborativo con reparto Fair-Share.
   - `[Reconciled]`: Transacción bancaria cruzada con éxito con reporte de pedido de Amazon o PayPal.
   - `[Bizum Match]`: Abono reconocido como amortización de deuda, no como ingreso inflado.
2. **Drawer Lateral de Split Contable**: Permite desglosar un ticket compuesto en múltiples categorías e imputar partidas individuales al espacio de pareja, garantizando la **Invariante RN-02 de Balance Cero ($0.00\text{ \euro}$ de descuadre)**.

---

### 4.3. Mock 3: Espacio Compartido (Pareja/Hogar), Fair-Share y Settle Up Bizum

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│  ESPACIO COMPARTIDO: HOGAR SERGIO & ELENA                 [⚙️ Configuración] [👤 Sergio / 👤 Elena]│
├──────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                  │
│   ┌───────────────────────────────────────────────────────────────────────────────────────────┐  │
│   │  BALANCE NETO CONSOLIDADO BILATERAL                                                       │  │
│   │                                                                                           │  │
│   │         Elena te debe:   170,61 €                                                         │  │
│   │         (Cálculo proporcional Fair-Share: Sergio 55% / Elena 45%)                         │  │
│   │                                                                                           │  │
│   │         [ 📲 Saldar Deuda vía Bizum ]           [ 📝 Registrar Gasto Manual ]             │  │
│   └───────────────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                                  │
│  MURO DE PRIVACIDAD ZERO-KNOWLEDGE:                                                              │
│  🔒 Los miembros solo ven los gastos imputados a este espacio común. Cuentas personales,        │
│     nóminas y carteras de inversión individuales son 100% invisibles y confidenciales.           │
│                                                                                                  │
│  HISTORIAL DE PARTIDAS COMPARTIDAS EN EL MES:                                                    │
│  ──────────────────────────────────────────────────────────────────────────────────────────────  │
│  FECHA       DESCRIPCIÓN              PAGADO POR     TOTAL      TU CUOTA (55%)   CUOTA ELENA (45%)│
│  06/10/2026  Mercadona Compra Semanal Sergio         120,00 €   66,00 €          54,00 €          │
│  05/10/2026  Factura Luz Iberdrola    Sergio         110,61 €   60,84 €          49,77 €          │
│  02/10/2026  Cena Restaurante         Elena           60,00 €   33,00 €          27,00 €          │
│  ──────────────────────────────────────────────────────────────────────────────────────────────  │
│                                              TOTALES: 290,61 €  159,84 €         130,77 €         │
│                                              BALANCE RESULTANTE A FAVOR DE SERGIO: 170,61 €       │
└──────────────────────────────────────────────────────────────────────────────────────────────────┘

                       ┌──────────────────────────────────────────────────┐
                       │ MODAL: LIQUIDAR BALANCE (SETTLE UP)              │
                       ├──────────────────────────────────────────────────┤
                       │ Miembro deudor: Elena                            │
                       │ Importe exacto de liquidación: 170,61 €          │
                       │ Método de pago: Bizum / Transferencia Inmediata  │
                       │ Concepto generado: NEXUS-HOGAR-OCT               │
                       │                                                  │
                       │ [ Copiar importe y concepto para Bizum ]         │
                       │                                                  │
                       │ Una vez recibido el Bizum, pulsa confirmar:      │
                       │ [Confirmar Liquidación y Resetear a 0,00 €]      │
                       └────────────────────────────────────────────────┘
```

#### Explicación de Componentes del Espacio Compartido:
1. **Regla de Reparto Fair-Share Dinámica**: Pondera el esfuerzo de cada persona en función de su capacidad económica declarada (ej. 55% Sergio, 45% Elena) sin revelar los importes exactos de las nóminas privadas.
2. **Consolidación Bilateral Neta**: Compensa automáticamente quién pagó qué, evitando micro-transferencias absurdas; al final del ciclo solo existe una cifra neta única.
3. **Muro de Privacidad Zero-Knowledge**: Garantiza que convivir financieramente no implique perder la soberanía o privacidad personal.
4. **Flujo Settle Up**: Proporciona los datos exactos para emitir el Bizum y extingue contablemente la deuda a $0.00\text{ \euro}$.

---

### 4.4. Mock 4: Hub de Conexión Bancaria (Open Banking & Extractos)

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│  HUB DE CONEXIONES BANCARIAS & FUENTES DE DATOS                            [+ Conectar Nuevo Banco]│
├──────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                  │
│  ENTIDADES CONECTADAS MEDIANTE OPEN BANKING (ENABLE BANKING AISP)                                │
│                                                                                                  │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐  │
│  │ [LOGO BBVA]  BBVA España                                                    [🟢 ACTIVA]     │  │
│  │ Cuentas: Cuenta Online (ES76 **** **** 4912)                                               │  │
│  │ Saldo sincronizado: 4.820,15 €                                                             │  │
│  │ Última sincronización automática: Hoy, 06:00 UTC (hace 4 horas)                           │  │
│  │ Consentimiento PSD2: Válido hasta 28/03/2027 (Quedan 172 días)                             │  │
│  │ [🔄 Sincronizar Ahora]           [Renovar SCA]           [Desconectar]                     │  │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                                  │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐  │
│  │ [LOGO ING]   ING Direct España                                              [🟡 POR RENOVAR]│  │
│  │ Cuentas: Cuenta Nómina + Cuenta Naranja (2 cuentas)                                        │  │
│  │ Saldo sincronizado: 12.340,80 €                                                            │  │
│  │ Consentimiento PSD2: Expira en 6 días (Obligatorio renovar SCA cada 180 días)              │  │
│  │ [ ⚠️ Renovar Consentimiento en 1 Tap ]                   [🔄 Sincronizar Ahora]             │  │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                                  │
│  IMPORTACIÓN MANUAL DE RESPALDO (EXTRACTOS CSV / XLSX)                                           │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐  │
│  │ Arrastra y suelta extractos bancarios de BBVA o ING para cargas históricas de años pasados  │  │
│  │ [📂 Seleccionar archivo .xlsx o .csv]           (Detección automática de entidad bancaria) │  │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Explicación de Componentes de Conexión Bancaria:
1. **Gestión Transparente de Sesiones PSD2**: Muestra el reloj de caducidad del consentimiento regulatorio de 180 días con alertas visuales preventivas a partir de los 15 días restantes.
2. **Sincronización Híbrida**: Permite la actualización desatendida mediante el worker en segundo plano cada 12 horas, junto con un botón de refresco bajo demanda (*cooldown* de 15 minutos).
3. **Respaldo de Extractos Tradicionales**: Mantiene accesible la zona de carga de ficheros tabulares para rescatar históricos de años anteriores no cubiertos por la ventana de Open Banking.

---

### 4.5. Mock 5: Simulador de Vida "What-If" y Nexus Health Score

```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│  SIMULADOR DE VIDA PATRIMONIAL ("WHAT-IF ENGINE") & SALUD FINANCIERA                             │
├──────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                  │
│  ┌────────────────────────────────────────────────────────────────────────────────────────────┐  │
│  │ NEXUS HEALTH SCORE:  84 / 100   (Salud Financiera Excelente ⭐)                            │  │
│  │ • Runway Liquidez:  8,4 meses (Óptimo > 6m)    • Ratio Endeudamiento (DTI): 22% (< 30%)    │  │
│  │ • Tasa Ahorro Mensual: 32% (Óptimo > 20%)      • Diversificación: Liquidez + Indexados + RE │  │
│  └────────────────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                                  │
│  SIMULADOR DE ESCENARIOS VITALES                                                                 │
│  Selecciona una decisión futura para proyectar su impacto en tu balance patrimonial:             │
│  [🏡 Compra de Vivienda e Hipoteca]   [🚗 Compra de Vehículo]   [💼 Parón Laboral / Excedencia]  │
│                                                                                                  │
│  PARÁMETROS DEL ESCENARIO: COMPRA DE VIVIENDA HABITUAL                                           │
│  Precio de compra:        [ 280.000 € ] ────────●────────────────────                            │
│  Ahorro aportado entrada: [  70.000 € ] ──────────────●────────────── (Gastos ITP/Notaría: 28k€) │
│  Tipo de interés hipoteca:   Fijo [x]   Variable [ ]   Interés: [ 2,75 % ]                       │
│  Plazo amortización:      [ 30 Años ]                                                            │
│                                                                                                  │
│  PROYECCIÓN DE IMPACTO INMEDIATO:                                                                │
│  • Liquidez posterior: 18.200 € (Runway resultante: 5,2 meses ➔ Sigue en rango seguro)           │
│  • Nueva cuota mensual: 858,40 € / mes                                                           │
│  • Nuevo DTI (Endeudamiento): 28,5% (Cerca del límite recomendado del 30%)                       │
│                                                                                                  │
│  PROYECCIÓN PATRIMONIAL A 10 AÑOS (PATRIMONIO NETO CON COMPRA VS. ALQUILER + INVERSIÓN):          │
│                                                                                                  │
│  350k ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─╭─────────── Escenario Compra Vivienda      │
│  280k ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─╭───────────╯                                           │
│  210k ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─╭───────╯─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─  Escenario Alquiler Continuado  │
│       Hoy     Año 2     Año 4     Año 6     Año 8     Año 10                                     │
│                                                                                                  │
│  Dictamen de Nexus Aura: "La compra es patrimonialmente viable y preserva tu fondo de emergencia.│
│  Sin embargo, tu cuota hipotecaria consumirá el 28,5% de tus ingresos, reduciendo tu capacidad   │
│  de ahorro mensual para fondos indexados en 420 €/mes."                                          │
└──────────────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Explicación de Componentes del Simulador:
1. **Desglose Multidimensional del Health Score**: No se limita a una nota abstracta; audita los cuatro pilares objetivos de la estabilidad económica personal.
2. **Controles Paramétricos Interactivos**: Sliders reactivos que recalculan en tiempo real impuestos (ITP, notaría, tasación), cuotas de amortización y descapitalización.
3. **Proyección Comparativa a Largo Plazo**: Gráficas predictivas que comparan la evolución patrimonial real entre ejecutar la decisión o mantener la estrategia actual.
4. **Dictamen Imparcial de Nexus Aura**: Conclusiones matemáticas sin sesgo comercial ni recomendaciones patrocinadas.

---

## 5. Invariantes Matemáticas y Modelo de Datos

Nexus implementa un modelo de datos relacional estricto sobre **PostgreSQL 17** con Entity Framework Core 10:

```mermaid
erDiagram
    USERS ||--o{ BANK_CONNECTIONS : owns
    USERS ||--o{ ACCOUNTS : owns
    USERS ||--o{ SHARED_SPACE_MEMBERS : participates
    BANK_CONNECTIONS ||--o{ CONNECTED_BANK_ACCOUNTS : discovers
    ACCOUNTS ||--o{ BANK_TRANSACTIONS : records
    BANK_TRANSACTIONS ||--o{ TRANSACTION_SPLIT_ITEMS : splits
    BANK_TRANSACTIONS ||--o{ SHARED_EXPENSE_ENTRIES : imputes
    SHARED_SPACES ||--o{ SHARED_SPACE_MEMBERS : includes
    SHARED_SPACES ||--o{ SHARED_EXPENSE_ENTRIES : contains
    SHARED_SPACES ||--o{ SHARED_SETTLEMENTS : resolves

    BANK_TRANSACTIONS {
        uuid id PK
        uuid account_id FK
        date operation_date
        date value_date
        varchar description
        decimal amount
        decimal balance_after
        varchar external_reference
        varchar fingerprint_sha256
        boolean is_manual
    }

    TRANSACTION_SPLIT_ITEMS {
        uuid id PK
        uuid transaction_id FK
        varchar concept
        decimal amount
        uuid category_id FK
    }

    SHARED_EXPENSE_ENTRIES {
        uuid id PK
        uuid shared_space_id FK
        uuid transaction_id FK
        uuid paid_by_member_id FK
        decimal total_amount
        decimal payer_share
        decimal debtor_share
        varchar status
    }
```

### Invariantes Contables Obligatorias:
1. **Invariante de Continuidad de Saldo (`BALANCE_DISCONTINUITY`)**:
   $$\text{Saldo}_{t} = \text{Saldo}_{t-1} + \text{Movimiento}_{t}$$
2. **Invariante de Split Exacto ($0.00\text{ \euro}$)**:
   $$\text{Importe Original} = \sum_{i=1}^{n} \text{Importe Partida}_i$$
3. **Invariante de Reparto Compartido**:
   $$\text{Total Gasto} = \text{Cuota Pagador} + \text{Cuota Deudor}$$
4. **Invariante de Extinción Bilateral**:
   $$\text{Balance Neto Resuelto} + \text{Bizum de Liquidación} = 0.00\text{ \euro}$$

---

## 6. Estrategia Multiplataforma (Web y Móvil con Capacitor)

Nexus opera bajo una arquitectura de **Núcleo Universal Único**:
- **Frontend desacoplado**: Diseñado en React 19 con Tailwind CSS y componentes modulares tipo Glassmorphism.
- **Shell Nativo con Capacitor**: El mismo código se empaqueta sin bifurcaciones para la web de escritorio, navegadores móviles y aplicaciones nativas en **Google Play (Android)** y **Apple App Store (iOS)**.
- **Deep Linking Nativo**: Redirecciones seguras OAuth y Open Banking mediante esquemas universales `nexus://banking/callback`.
- **Almacenamiento Criptográfico Adaptativo**: Tokens de sesión en memoria y cookies `HttpOnly` en web; persistencia en *Android Keystore* e *iOS Keychain* en aplicaciones nativas móviles.

---

*Documento aprobado y sellado por `@warren` y `@jarvis` para gobernar el desarrollo y ciclo de vida de **Nexus**.*
