# Especificación Funcional: Sincronización Automática con Bancos mediante Enable Banking (Open Banking AISP)

- **ID de la Feature**: `005-open-banking-enable-banking`
- **Estado**: Aprobado para Planificación Técnica (Gate 1 SDD)
- **Fecha de Creación**: 2026-10-06
- **Última Actualización**: 2026-10-06
- **Especialista de Dominio**: `@warren`
- **Orquestador Técnico**: `@jarvis`

---

## 1. Resumen y Contexto

Actualmente, **Nexus** cuenta con motores de procesamiento e ingesta de extractos bancarios en formatos tabulares (CSV, XLSX) para entidades como BBVA e ING Direct ([`001-raw-statements-storage`](file:///C:/Projects/Nexus/specs/features/001-raw-statements-storage/01-spec.md)). Aunque este mecanismo es robusto y seguro para cargas históricas masivas, introduce una **fricción operativa crítica**: exige que el usuario inicie sesión periódicamente en cada portal bancario, descargue los ficheros y los suba manualmente a Nexus.

Para materializar la visión de Nexus como un **Financial OS autónomo**, es imperativo dotar a la plataforma de sincronización continua, transparente y desatendida. Tras analizar las restricciones regulatorias de la directiva europea **PSD2/PSD3**, se adopta **Enable Banking** como agregador y conector oficial:
1. **Sin licencia regulatoria propia**: Enable Banking actúa como entidad autorizada AISP (Account Information Service Provider), asumiendo el marco legal y la certificación eIDAS/ASPSP.
2. **Coste cero para desarrollo y uso personal**: Proporciona un modo restringido (*Restricted Mode*) gratuito para conectar cuentas personales.
3. **Privacidad "Pure-Pipe"**: No almacena los extractos ni credenciales en bases de datos intermedias de terceros; actúa como un túnel criptográfico seguro entre el banco del usuario y Nexus.
4. **Cero credenciales compartidas**: El usuario autoriza la conexión mediante la pasarela segura oficial de su propio banco mediante **SCA (Strong Customer Authentication)** con validez de 180 días.

---

## 2. Historias de Usuario y Casos de Uso

### Historia 1: Conexión de nueva entidad bancaria
> **Como** usuario de Nexus,  
> **Quiero** conectar mi cuenta bancaria (ej. BBVA, ING, Santander, CaixaBank) seleccionándola en una lista y autorizando el acceso en la app/web oficial de mi banco,  
> **Para que** Nexus importe automáticamente mis cuentas y saldos sin tener que proporcionarle mis contraseñas bancarias a nadie.

### Historia 2: Sincronización automática desatendida
> **Como** usuario activo de Nexus,  
> **Quiero** que el sistema consulte periódicamente (cada 6–12 horas) mis últimos movimientos bancarios de forma transparente en segundo plano,  
> **Para que** mi panel financiero, categorías, alertas de gastos y deudas compartidas estén siempre al día sin tener que descargar extractos manualmente.

### Historia 3: Deduplicación y preservación de reglas de usuario
> **Como** usuario que ya ha categorizado o dividido transacciones pasadas,  
> **Quiero** que la sincronización automática reconozca movimientos ya registrados e ignore duplicados,  
> **Para que** mis splits contables, notas personales y categorizaciones manuales no se sobrescriban ni se dupliquen.

### Historia 4: Ciclo de vida y renovación del consentimiento (180 días)
> **Como** usuario regulado bajo la normativa europea,  
> **Quiero** ver con claridad cuándo va a expirar la autorización de mi banco y poder renovarla con un solo clic (SCA),  
> **Para que** el flujo de sincronización no se interrumpa inesperadamente y mantenga el control sobre el acceso a mis datos.

### Historia 5: Desconexión y revocación
> **Como** usuario que cambia de banco o desea cesar la sincronización,  
> **Quiero** poder desvincular una entidad bancaria en cualquier momento,  
> **Para que** Nexus cese inmediatamente las llamadas de sincronización y revoque la sesión en Enable Banking.

---

## 3. Alcance (Scope)

### ✅ Dentro del Alcance (In-Scope)
1. **Catálogo de entidades bancarias**: Consulta y filtrado de bancos soportados en España y zona SEPA (`country = 'ES'`) con nombres comerciales y logotipos oficiales.
2. **Flujo de Consentimiento OAuth/SCA**:
   - Generación de sesión de autorización en Enable Banking (`POST /auth`).
   - Redirección del usuario al portal web o deep link móvil de la entidad bancaria.
   - Retorno mediante callback seguro tanto en Web (`/banking/callback`) como en app móvil nativa vía Capacitor (`nexus://banking/callback`).
3. **Descubrimiento e Importación de Cuentas**:
   - Registro de cuentas corrientes, cuentas de ahorro y tarjetas asociadas (IBAN, nombre, divisa, saldo actual y saldo disponible).
4. **Sincronización Periódica de Transacciones**:
   - Descarga programada de transacciones liquidadas (`booked`) y pendientes (`pending`).
   - Normalización de transacciones al modelo de dominio unificado de Nexus (`ParsedRawTransaction`).
   - Deduplicación determinista basada en el `external_transaction_id` del banco complementado con el fingerprint SHA-256 de Nexus.
   - Inyección en el pipeline de categorización automática y detección de Bizums.
5. **Monitor de Consentimiento y Renovación**:
   - Seguimiento del tiempo de expiración (180 días según EBA RTS).
   - Indicador visual del estado de la conexión (`Activa`, `Próxima a Caducar`, `Caducada`, `Revocada`).
   - Botón de renovación ágil de consentimiento.

### 🚫 Fuera del Alcance (Out-of-Scope)
- **Iniciación de Pagos (PIS / PISP)**: Nexus solo consulta información de cuentas (AIS); bajo ninguna circunstancia emite transferencias ni ordena pagos salientes desde las cuentas del usuario.
- **Bancos fuera de la Unión Europea / SEPA**: En esta versión el conector se acota a entidades con soporte PSD2 ASPSP en la UE (con foco prioritario en España).
- **Almacenamiento de contraseñas o PINs**: Queda expresamente prohibido solicitar o almacenar credenciales de banca online en la base de datos de Nexus.

---

## 4. Reglas de Negocio e Invariantes

### RN-01: Soberanía de Seguridad y Delegación SCA (Inviolable)
Nexus jamás solicitará, transmitirá ni persistirá el usuario, contraseña o código de acceso de la banca online. La autenticación se realiza de manera exclusiva e inalterable en los servidores y aplicaciones del banco emisor mediante redirección segura.

### RN-02: Cifrado en Reposo de Credenciales de Integración
Cualquier identificador de sesión, token de acceso o clave de API relacionada con la conexión de Enable Banking debe almacenarse cifrado en la base de datos mediante **AES-256-GCM** utilizando los servicios de cifrado simétrico de [`Nexus.Domain.Security`](file:///C:/Projects/Nexus/src/Nexus.Domain/Security).

### RN-03: Deduplicación Determinista Multinivel
Para evitar duplicar movimientos entre importaciones de ficheros manuales y sincronizaciones automáticas de Enable Banking:
1. **Nivel 1 (Identificador Externo)**: Si la transacción bancaria provee un `entryReference` o `transactionId` único persistente, se utiliza como clave primaria externa.
2. **Nivel 2 (Fingerprint Nexus)**: Se calcula la huella criptográfica SHA-256 sobre `(AccountId, OperationDate, Amount, ValueDate, SanitizedDescription)`.
Si un movimiento entrante coincide con un registro preexistente, se omite de la inserción de nuevos registros y solo se actualiza su estado (ej. de `Pending` a `Booked`).

### RN-04: Inmutabilidad de Decisiones del Usuario (`IsManual = true`)
Si una transacción importada previamente fue editada, categorizada, vinculada a una deuda compartida o dividida en un *split* por el usuario (`IsManual = true`), una sincronización posterior **nunca sobrescribirá** su categoría, partidas ni notas.

### RN-05: Alerta Temprana de Caducidad de Consentimiento
El consentimiento bancario otorgado bajo PSD2 vence a los 180 días.
- A los **165 días** (15 días antes de expirar), Nexus cambiará el estado de la conexión a `ExpiringSoon` y mostrará una notificación visual en la interfaz recomendando la renovación con 1 tap.
- A los **180 días**, si no ha sido renovado, la conexión pasa a `Expired` y el worker suspende las llamadas periódicas a esa entidad hasta que el usuario renueve la autorización.

### RN-06: Continuidad de Saldo e Invariante de Descuadre
Tras cada sincronización automática de una cuenta:
$$\text{Saldo Calculado} = \text{Saldo Inicial} + \sum \text{Movimientos}$$
Si existe una discrepancia no conciliable superior a $0.00\ €$ entre el saldo calculado y el saldo reportado por el banco (`availableBalance`), el sistema registrará una alerta de `BALANCE_DISCONTINUITY` para auditoría del usuario sin bloquear la cuenta.

---

## 5. Criterios de Aceptación (BDD Gherkin)

### Escenario 1: Vinculación exitosa de cuenta BBVA vía Enable Banking
- **Dado que** un usuario autenticado entra en la sección "Bancos Conectados" en Nexus,
- **Cuando** selecciona "BBVA" de la lista de entidades y pulsa "Conectar Banco",
- **Entonces** el sistema genera una sesión de autorización con Enable Banking y redirige al usuario a la URL oficial de SCA de BBVA,
- **Y** tras aprobar la autenticación en la app de BBVA y retornar a `/banking/callback`, Nexus registra la conexión, descubre las cuentas asociadas e importa los saldos en tiempo real.

### Escenario 2: Sincronización en segundo plano sin duplicados
- **Dado que** el usuario tiene una conexión activa con ING Direct con 15 transacciones ya registradas,
- **Cuando** el worker programado se ejecuta a las 06:00 y Enable Banking devuelve 18 transacciones (15 ya existentes y 3 nuevas),
- **Entonces** Nexus detecta las 15 existentes mediante fingerprint e inserta únicamente las 3 transacciones nuevas,
- **Y** las 3 nuevas transacciones pasan por el motor de categorización automática y normalización de comercios.

### Escenario 3: Aviso de caducidad del consentimiento
- **Dado que** una conexión bancaria lleva activa 166 días desde su última autorización SCA,
- **Cuando** el usuario consulta su dashboard o lista de cuentas,
- **Entonces** la tarjeta del banco muestra una insignia amarilla de "Consentimiento por expirar en 14 días",
- **Y** al hacer clic en "Renovar", se inicia el flujo SCA directo para extender el consentimiento por otros 180 días.

---

## 6. Decisiones de Producto y Preguntas Abiertas
- **Frecuencia del Worker**: Sincronización por defecto cada 12 horas (06:00 y 18:00 UTC) para optimizar límites de API y preservar la cuota del modo restringido. Se habilitará un botón "Sincronizar ahora" con un cooldown de 15 minutos por cuenta.
- **Soporte Móvil**: Uso de `Browser` de Capacitor con esquema `nexus://banking/callback` para retorno transparente tras la autenticación en navegadores del sistema operativo móvil.
