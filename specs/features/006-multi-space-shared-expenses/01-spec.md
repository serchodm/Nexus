# Especificación Funcional: Espacios Compartidos Multigrupo, N-Miembros, Viajes y Simplificación de Deudas

- **ID de la Feature**: `006-multi-space-shared-expenses`
- **Estado**: Borrador para Revisión del Usuario (Gate 1 SDD — NO EJECUTAR AÚN)
- **Fecha de Creación**: 2026-10-07
- **Última Actualización**: 2026-10-07
- **Especialista de Dominio**: `@warren`
- **Orquestador Técnico**: `@jarvis`

---

## 1. Resumen y Justificación Estratégica

La especificación previa ([`004-shared-expenses`](file:///C:/Projects/Nexus/specs/features/004-shared-expenses/01-spec.md)) introdujo el libro contable compartido (*Shared Ledger*) enfocado en una relación bilateral estricta (pareja/hogar de dos miembros con reparto 50/50 o Fair-Share 55/45). 

Sin embargo, la realidad financiera de un individuo es **multidimensional y dinámica**: una misma persona participa simultáneamente en múltiples círculos sociales y contextos económicos que **jamás deben mezclarse en el mismo balance**:
1. **Espacio Permanente de Convivencia (Hogar / Pareja)**: Gastos corrientes de la vivienda (supermercado, luz, alquiler, reparaciones) con liquidaciones mensuales.
2. **Espacio de Viaje o Vacaciones en Pareja**: Partidas de ocio puntual (vuelos, hoteles, cenas) que deben aislarse para **no falsear el presupuesto operativo ordinario del hogar**.
3. **Grupos Multimiembro ($N \ge 3$) con Amigos**: Salidas, barbacoas, cenas de fin de semana o casas rurales donde uno o varios miembros adelantan pagos y el consumo se distribuye entre 4, 6 u 8 personas.
4. **Viajes Familiares / Proyectos de Ocio**: Grupos de gasto con participantes que tienen diferentes pesos o consumos asimétricos (ej. familias con hijos).
5. **Eventos Efímeros**: Un bote o cena puntual que se liquida a $0.00\text{ \euro}$ en 48 horas y debe poder **archivarse** para mantener la interfaz limpia.

Esta feature evoluciona el Pilar 3 de Nexus desde un modelo bilateral cerrado hacia una **plataforma multiespacio de $N$ grupos y $N$ miembros**, con asignación selectiva de transacciones bancarias, modalidades avanzadas de reparto ($1/N$, porcentajes, importes, partes/shares y exclusiones), y un **algoritmo de simplificación de deudas** que minimiza las transferencias bancarias o Bizums necesarios para saldar cuentas.

---

## 2. Historias de Usuario y Casos de Uso

### Historia 1: Creación y gestión de N espacios de distinta naturaleza
> **Como** usuario de Nexus,  
> **Quiero** crear múltiples espacios compartidos con nombres, iconos y categorías diferenciadas (ej. "Hogar", "Viaje a Roma", "Cena Amigos"),  
> **Para que** los gastos de mis vacaciones o salidas de ocio no contaminen los números del presupuesto mensual de mi casa.

### Historia 2: Reparto flexible en grupos de más de 2 personas
> **Como** miembro de un grupo de viaje con 5 amigos,  
> **Quiero** registrar un gasto de 250 € (pagado con mi tarjeta) y poder dividirlo entre todos por igual, o excluir a amigos que no participaron de ese gasto concreto,  
> **Para que** nadie pague por servicios o productos que no consumió.

### Historia 3: Imputación contextual desde transacciones bancarias
> **Como** usuario que revisa sus movimientos bancarios en Nexus,  
> **Quiero** pulsar "Compartir Gasto" en una compra y seleccionar en un menú desplegable a qué espacio pertenece (Pareja, Viaje o Amigos),  
> **Para que** el flujo de ingesta desde BBVA o ING alimente el libro compartido correspondiente en 2 clics.

### Historia 4: Gastos pagados por terceros y deudas a pagar (Payables)
> **Como** participante de un espacio cuando otra persona adelanta el dinero (ej. Carlos pagó el hotel o Elena pagó la compra),  
> **Quiero** registrar el gasto indicando que pagó esa persona y ver reflejada mi cuota exacta como saldo deudor ("Tú debes X € a Carlos"),  
> **Para que** mi balance patrimonial compute ese pasivo pendiente y pueda emitir el Bizum de liquidación con un solo clic sin duplicar gastos.

### Historia 5: Algoritmo de simplificación de deudas en grupos grandes
> **Como** participante de un grupo de 6 amigos donde varios miembros han pagado cosas distintas,  
> **Quiero** que Nexus calcule el balance neto de cada uno y me diga exactamente a quién y cuánto tengo que pagar yo con el número mínimo de transferencias,  
> **Para que** no tengamos que hacer 15 Bizums cruzados y se liquide todo en 2 o 3 pagos.

### Historia 6: Miembros sombra (sin cuenta obligatoria en Nexus)
> **Como** organizador de un viaje donde no todos mis amigos usan Nexus,  
> **Quiero** poder añadir miembros con solo un nombre/apodo y teléfono para Bizum,  
> **Para que** podamos llevar las cuentas del grupo sin obligar a nadie a descargarse o registrarse en la aplicación.

### Historia 7: Archivo y cierre de grupos efímeros
> **Como** usuario tras volver de un viaje o cena ya saldada,  
> **Quiero** marcar el espacio como "Archivado",  
> **Para que** deje de aparecer en el selector diario pero conserve su historial contable para consulta futura.

---

## 3. Alcance (Scope)

### ✅ Dentro del Alcance (In-Scope)
1. **Modelo de Espacios Dinámicos ($N$ Espacios)**:
   - Creación, edición, archivo y reactivación de espacios.
   - Metadatos del espacio: Nombre, icono, divisa base (`EUR`, `USD`, `GBP`), tipo (`Household`, `Trip`, `Event`, `Project`), descripción y fecha límite opcional.
2. **Miembros Multiusuario y Miembros Sombra ($N$ Miembros)**:
   - Espacios sin límite de participantes ($N \ge 2$).
   - Miembros vinculados a usuarios registrados de Nexus (`RegisteredUser`).
   - Miembros sombra (`GuestMember` / `ShadowMember`) definidos por nombre, avatar opcional y teléfono Bizum.
3. **Modalidades de Reparto Avanzadas**:
   - **Equitativo ($1/M$)**: División automática entre los $M$ miembros seleccionados.
   - **Porcentajes Personalizados**: Cada miembro asume un $\%$ explícito ($\sum \% = 100\%$).
   - **Importes Fijos Exactos**: Importe monetario concreto por persona ($\sum \text{Partes} = \text{Total}$).
   - **Pesos / Shares**: Asignación de multiplicadores (ej. Adulto = 1, Niño = 0.5, Pareja = 2).
   - **Fair-Share Bipersonal / Familiar**: Proporcional a ingresos declarados para espacios de convivencia.
   - **Exclusión puntual**: Casilla para marcar quién entra o no en cada partida.
4. **Matriz de Balances y Algoritmo de Simplificación de Deudas**:
   - Cálculo de posición neta individual: $\text{Balance Neto}(M) = \text{Total Pagado por } M - \text{Total Consumo de } M$.
   - Reducción del grafo de transferencias a un máximo de $N-1$ pagos.
   - Panel visual: *"Tú debes X € a [Persona]"* o *"[Persona] te debe Y €"*.
5. **Liquidación Granular (Settle Up)**:
   - Liquidación total del grupo o liquidación bilateral selectiva entre dos miembros vía Bizum/transferencia.
   - Generación de referencia de pago para Bizum.
6. **Muro de Privacidad Zero-Knowledge Multiespacio**:
   - Aislamiento estricto: los miembros de "Viaje Amigos" solo ven los gastos de ese viaje; no tienen acceso a tus cuentas personales, nóminas ni a tus otros espacios (Hogar, Pareja, etc.).
7. **Selector Global en UI**:
   - Dropdown de selección de espacio activo en TopBar y en el menú de navegación.
   - Visión consolidada: *"Total Cuentas por Cobrar en todos tus grupos: +245,60 €"*.

### 🚫 Fuera del Alcance (Out-of-Scope)
- **Monedero virtual o custodia de saldo en grupo**: Nexus no almacena dinero grupal ni es una pasarela bancaria.
- **Cobro automático por domiciliación o tarjeta**: Nexus genera la propuesta e instrucciones de Bizum/SEPA, pero el movimiento de fondos lo ejecuta el usuario en su banca habitual.

---

## 4. Reglas de Negocio e Invariantes

### RN-01 Invariante de Reparto Exacto a 0.00 € (Inviolable)
Cualquiera que sea la modalidad de reparto (porcentaje, partes, importes fijos o proporcional), la suma aritmética de las cuotas asignadas a todos los participantes debe igualar con precisión absoluta al importe total de la transacción original:
$$\text{Total Gasto} = \sum_{i=1}^{M} \text{Cuota}_i$$
Cualquier residuo generado por divisiones inexactas (ej. $10.00\text{ \euro} / 3 = 3.3333...$) debe ajustarse al céntimo mediante *Banker's Rounding* asignando el céntimo sobrante o faltante al pagador de la partida. Tolerancia de descuadre: **$0.00\text{ \euro}$**.

### RN-02 Invariante de Balance Cero Colectivo
En todo momento, dentro de cualquier espacio compartido cerrado:
$$\sum_{m \in \text{Miembros}} \text{Balance Neto}(m) \equiv 0.00\text{ \euro}$$
La suma de todos los saldos acreedores (a favor) debe equivaler exactamente a la suma de todos los saldos deudores (en contra).

### RN-03 Desacoplamiento Contable de Espacios
Los balances, deudas y liquidaciones de un espacio son **estrictamente independientes**. Un saldo a favor en "Viaje Amigos" nunca amortizará automáticamente una deuda en "Hogar Pareja" salvo que el usuario ejecute una acción explícita de traspaso.

### RN-04 Soberanía de Privacidad
Un usuario que participa en 4 espacios solo comparte con los integrantes de cada espacio las transacciones asignadas explícitamente a ese espacio. Ningún miembro de un grupo puede inferir ingresos, patrimonio neto o gastos privados de otro miembro.

### RN-05 Tratamiento Contable Dual de Deudas (Receivables vs. Payables)
- **Posición Acreedora (El usuario pagó)**: El gasto personal computa como consumo, mientras que las cuotas de los demás se reconocen como un **Activo a Corto Plazo (Cuentas por Cobrar / Receivables)** en el patrimonio neto.
- **Posición Deudora (Un tercero pagó)**: La cuota de consumo del usuario computa como gasto real en su categoría analítica, generándose simultáneamente un **Pasivo a Corto Plazo (Cuentas por Pagar / Payables)** en su balance.
- **Neutralidad de la Liquidación**: El posterior pago o cobro de Bizum extingue dicho Activo o Pasivo sin distorsionar ingresos salariales ni duplicar partidas de gasto.

---

## 5. Criterios de Aceptación (BDD Gherkin)

### Escenario 1: Creación de un nuevo espacio de viaje con 4 amigos
- **Dado que** Sergio está autenticado en Nexus,
- **Cuando** crea un nuevo espacio llamado "Viaje a Asturias", selecciona tipo "Trip" e introduce 3 amigos invitados (Carlos, Elena, Marcos),
- **Entonces** el espacio se crea con 4 miembros, balance inicial de 0.00 € para todos y queda disponible en el selector de espacios.

### Escenario 2: Imputación de compra con exclusión de un miembro
- **Dado que** Sergio realiza un pago de 120.00 € en un restaurante en "Viaje a Asturias",
- **Y** Marcos no asistió a esa cena,
- **Cuando** Sergio imputa el gasto y desmarca a Marcos del reparto,
- **Entonces** el sistema divide los 120.00 € equitativamente entre los 3 participantes restantes (40.00 € cada uno),
- **Y** el balance de Marcos permanece en 0.00 €, mientras Sergio queda con +80.00 € a favor y Carlos y Elena con -40.00 € cada uno.

### Escenario 3: Simplificación de deudas tras múltiples gastos
- **Dado que** en un grupo de 3 miembros:
  - Sergio pagó 60 € para los tres (20 € cada uno),
  - Carlos pagó 30 € para los tres (10 € cada uno),
  - Elena no ha pagado nada (consumió 30 € en total),
  - **Cuando** se consulta el informe de liquidación simplificada,
  - **Entonces** el algoritmo indica que Elena debe pagar 30 € directamente a Sergio, y Carlos no debe pagar ni recibir nada (balance neto 0 €),
  - **Y** se evita que Elena pague a Carlos y Carlos a Sergio.

### Escenario 4: Gasto pagado por un tercero y liquidación de deuda propia (Payable)
- **Dado que** en el grupo "Viaje a Asturias", Carlos paga un alojamiento rural de 120.00 € con su tarjeta para los 4 miembros (30.00 € cada uno),
- **Cuando** se registra el gasto con `PaidBy = Carlos`,
- **Entonces** la posición neta de Sergio pasa a -30.00 € ("Tú debes 30,00 € a Carlos"),
- **Y** en el balance patrimonial de Sergio se computa un Pasivo a Corto Plazo de 30.00 €,
- **Y** la interfaz proporciona el botón "Pagar con Bizum a Carlos" con importe de 30.00 € y concepto predefinido,
- **Y** cuando Sergio realiza el Bizum, Nexus clasifica el movimiento bancario como amortización de pasivo, dejando su deuda con Carlos en 0.00 € sin duplicar el gasto.

---

## 6. Estado y Siguientes Pasos
Este documento define la base conceptual del nuevo modelo multiespacio de Nexus. Queda pendiente de revisión y validación por parte del usuario antes de proceder con el diseño técnico y el desglose de tareas de `@jarvis`.
