# Nexus — Backlog de Producto y Mejoras Pendientes

- **Nivel de Gobernanza**: Registro Central de Necesidades y Mejoras (Nivel 0 / SDD Gate 0)
- **Coordinadores**: `@warren` (Dominio Financiero) y `@jarvis` (Arquitectura Técnica)
- **Última Actualización**: 2026-10-10

---

## 📌 Items de Backlog Priorizados

### 1. `BKLOG-001`: Edición de Titularidad y Participación en Cuentas Bancarias (`UpdateBankAccount`)
- **Origen**: Refinamiento previo a Feature 006 / Integración Open Banking (Feature 005).
- **Problema**:
  - Actualmente las cuentas bancarias se crean mediante `CreateBankAccountCommand` (manualmente) o mediante `CompleteBankAuthCallbackCommand` (vía Open Banking / Enable Banking).
  - Al sincronizar vía Open Banking, las cuentas descubiertas se inicializan por defecto con `HolderRole.Owner` (100% de titularidad).
  - No existe actualmente un endpoint ni comando CQRS para que el usuario pueda reclasificar una cuenta existente a **Cotitular con Pareja** (`HolderRole.JointOwner` con % custom) o **Cuenta Supervisada / Menor** (`HolderRole.LegalGuardian` al 0%).
- **Solución Propuesta**:
  - **Backend**:
    - Comando CQRS `UpdateBankAccountCommand(Guid AccountId, Guid UserId, string? Name, HolderRole HolderRole, decimal? OwnershipPercentage, List<string>? LinkedCardsLast4)`.
    - Endpoint FastEndpoints: `PATCH /api/bank-accounts/{id}` con validación FluentValidation (invariante: si `JointOwner`, % entre 1 y 99; si `LegalGuardian` o `Authorized`, % forzado a 0; si `Owner`, % forzado a 100).
    - Actualización atómica en base de datos y recálculo automático de `totalNetWorthShare`.
  - **Frontend**:
    - Botón de edición en tarjeta de cuenta dentro de `AccountSelector.tsx` o modal de ajustes de cuenta.
    - Corrección en payload frontend (`AccountSelector.tsx:59`) para asegurar que `LegalGuardian` envíe explícitamente `0` en `ownershipPercentage`.
- **Prioridad**: Alta (siguiente iteración tras Feature 006 o soporte complementario a Pilar 2).

---

### 2. `BKLOG-002`: Aislamiento Contable y Analítico de Cuentas Supervisadas de Menores en Nexus Aura
- **Origen**: Principios de soberanía patrimonial y separación de esferas jurídicas de `@warren`.
- **Problema**:
  - Un padre o tutor legal puede registrar la cuenta de un hijo menor bajo `HolderRole.LegalGuardian` con el fin de **observar movimientos, cargar extractos y categorizar gastos** sin que el saldo compute en su patrimonio neto (0.00 € de imputación).
  - Sin embargo, los motores analíticos de **Nexus Aura (Pilar 6)** y **What-If Engine (Pilar 7)** podrían interpretar erróneamente los saldos líquidos o movimientos de la cuenta del menor como parte de la tesorería del tutor (p.ej. sugiriendo invertir un "dinero ocioso" que pertenece al menor o alterando el fondo de emergencia del adulto).
- **Solución Propuesta**:
  - **Regla Inviolable de Aura (RA-09)**: *"Aislamiento Estricto de Cuentas Supervisadas y Tutelares"*.
  - El motor de contexto (`Financial Context Engine`) debe segregar las cuentas con `HolderRole.LegalGuardian` y `HolderRole.Authorized` en una dimensión separada denominada **"Custodia Familiar / Cuentas Supervisadas"**.
  - Aura jamás debe consolidar estos saldos en el cálculo de *Runway*, *DTI*, colchón de seguridad de 6 meses ni proponer movimientos de inversión sobre fondos del menor.
  - Generación de auditorías específicas para cuentas de menores (p.ej. seguimiento de cuotas escolares, propinas o huchas de ahorro infantil), aisladas de la planificación patrimonial del tutor.
- **Prioridad**: Media-Alta (alineado con la implementación del Pilar 6: Nexus Aura).

---

## 🗺️ Estado del Roadmap Macro

1. **Features 001 - 005**: 100% Completadas (Extractos, Categorización, Amazon/PayPal, Gastos Bilaterales, Open Banking).
2. **Feature 006**: Espacios Compartidos Multigrupo, N-Miembros y Simplificación de Deudas (En ejecución).
3. **Mejoras BKLOG-001 y BKLOG-002**: Agendadas para iteraciones inmediatas / Pilar 2 y Pilar 6.
4. **Pilar 2 Avanzado**: Consolidación Patrimonial Multiactivo Real (Inmuebles, Hipotecas, Inversiones, Cripto).
5. **Pilar 6 & 7**: Nexus Aura & What-If Engine (Copiloto financiero e índice de salud financiera).
