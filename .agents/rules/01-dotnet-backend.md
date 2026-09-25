# Directrices Backend: .NET Core (Clean Architecture + CQRS + FastEndpoints)

## 1. Arquitectura del Sistema
El backend se estructura siguiendo **Clean Architecture**, **CQRS** y el patrón **REPR** (*Request-Endpoint-Response*) mediante **FastEndpoints**:

- **Domain**: Entidades, Value Objects, eventos de dominio, excepciones de negocio e interfaces de repositorios para mutaciones. Totalmente aislado de EF Core y paquetes de infraestructura.
- **Application**: 
  - **Commands (Escritura)**: Orquestan lógica de dominio a través de repositorios y transacciones unitarias.
  - **Queries (Lectura)**: Consultas optimizadas para alto volumen de lectura. Acceden directamente al contexto de lectura proyectando a DTOs con `.AsNoTracking()`.
  - **Validaciones**: Validadores automáticos por petición usando `FluentValidation`.
- **Infrastructure**: Implementación de repositorios de escritura, `ApplicationDbContext`, configuración de Npgsql/PostgreSQL, servicio de tokens JWT/Cookies, proveedor de Google OAuth y validador TOTP.
- **Presentation (FastEndpoints)**: Endpoints verticales estructurados con el patrón REPR (`Endpoint<TRequest, TResponse>`). Un archivo por endpoint.

---

## 2. Regla Anti-"Fat Endpoint" en FastEndpoints
Para preservar la separación de responsabilidades y la pureza arquitectónica:
- **Prohibición de Lógica de Negocio en `ExecuteAsync`**: Queda expresamente prohibido inyectar `DbContext`, escribir queries LINQ o implementar lógica de negocio dentro del método `ExecuteAsync` del endpoint.
- **Responsabilidad Estricta del Endpoint**:
  1. Validar la entrada (ejecutada automáticamente por el pipeline de FastEndpoints mediante `Validator<TRequest>`).
  2. Mapear el `TRequest` a un Command o Query de la capa Application.
  3. Despachar el Command/Query al Handler correspondiente.
  4. Mapear el `Result<T>` del handler a la respuesta HTTP adecuada (`SendOkAsync`, `SendNotFoundAsync`, etc.).

---

## 3. Optimización para Alto Volumen de Lectura
- **Bypass de Repositorio en Consultas**: Las Queries consultan directamente el contexto de persistencia proyectando a DTOs (`.Select(x => new MyDto { ... })`) con `.AsNoTracking()`. Nunca devolver entidades de dominio en endpoints de lectura.
- **Hot-Paths Críticos**: Para endpoints de altísima concurrencia (miles de req/s), se autoriza el uso de **Compiled Queries** (`EF.CompileAsyncQuery`) o micro-consultas directas con **Dapper** para evitar el coste de compilación de árboles de expresiones LINQ.
- **Paginación Obligatoria**: Toda colección debe implementar paginación mediante cursor o `Take/Skip` con límite máximo de registros.
- **Caché de Consultas**: Para lecturas frecuentes y de baja volatilidad, usar `IMemoryCache` (o `IDistributedCache` con Redis) aplicando el patrón *Cache-Aside*.

---

## 4. Principios SOLID y Patrones de Diseño
- **Single Responsibility (SRP)**: Cada endpoint gestiona exclusivamente una ruta HTTP y una operación de negocio.
- **Open/Closed (OCP)**: Extensiones mediante middlewares y decoradores de pipeline sin modificar clases base.
- **Liskov Substitution (LSP)**: Respeto estricto a los contratos de las interfaces del dominio.
- **Interface Segregation (ISP)**: Interfaces pequeñas y de propósito específico. Prohibido crear repositorios genéricos con decenas de métodos no utilizados.
- **Dependency Inversion (DIP)**: Inyección de dependencias exclusivamente por constructor con modificador `readonly`. Prohibido el uso de `new` para servicios o dependencias.
- **Result Pattern**: Control de errores previsibles mediante objetos resultado (`Result<T>` o `OneOf`) sin penalizar el rendimiento con excepciones de control de flujo.

---

## 5. Ciclo de Vida de Inyección de Dependencias (DI)
- **Scoped**: `ApplicationDbContext`, repositorios de comandos, servicios de dominio y handlers de casos de uso.
- **Transient**: Validadores `FluentValidation` y calculadores puros sin estado.
- **Singleton**: Exclusivo para componentes thread-safe (algoritmo TOTP, configuración tipada `IOptions<T>`, clientes HTTP reutilizables, `IHostedService`).
  - **Regla Crítica contra Captive Dependencies**: NUNCA inyectar un servicio `Scoped` dentro de un `Singleton`. Si un `IHostedService` o Singleton requiere base de datos, debe crear un ámbito explícito mediante `IServiceScopeFactory`.

---

## 6. Autenticación Google OAuth + MFA (TOTP)
- **Vinculación Inmutable de Google**:
  - Validar criptográficamente el token con `GoogleJsonWebSignature.ValidateAsync()`.
  - La clave foránea externa obligatoria es el claim **`sub`** (identificador único, permanente e inmutable de Google), NO el correo electrónico.
- **Seguridad de Tokens y Protección contra XSS**:
  - **Access Token**: JWT de vida corta (10-15 min) devuelto en el cuerpo de la respuesta para almacenamiento exclusivo en memoria por la aplicación cliente.
  - **Refresh Token**: Emitido obligatoriamente en una **Cookie `HttpOnly`, `Secure`, `SameSite=Strict`** con expiración a 7-30 días y rotación estricta en cada uso. JavaScript jamás debe tener acceso al Refresh Token.
- **Flujo TOTP Seguro (RFC 6238)**:
  - **Enrolamiento sin Bloqueo (Anti-Lockout)**: Al activar MFA, la clave secreta en Base32 se guarda en estado provisional (`is_mfa_enabled = false`). Solo se pasa a `true` tras recibir y verificar con éxito un primer código de 6 dígitos emitido por el autenticador del usuario.
  - **Protección contra Replay Attacks**: Registrar el último código o marca de tiempo de paso utilizado por el usuario para impedir la reutilización de un mismo código dentro de la ventana de validez de $\pm 1$ paso (30 segundos).
  - **Flujo de Acceso en 2 Fases**:
    1. Si `is_mfa_enabled == true`: el endpoint `/api/auth/google` devuelve `mfa_required` y un ticket/token temporal de 5 minutos (`scope: "mfa_pending"`).
    2. El endpoint `/api/auth/mfa/verify` valida el código de 6 dígitos.
    3. Si es válido, emite el Access Token en JSON y el Refresh Token en la Cookie `HttpOnly`.
  - **Códigos de Recuperación**: Generación de códigos de emergencia de un solo uso almacenados con hash criptográfico (Argon2id o BCrypt).

---

## 7. Jerarquía Estricta de Dependencias e Implementación de Código
Antes de implementar cualquier funcionalidad, se debe aplicar rigurosamente este orden de prelación:
1. **Nivel 1 (Nativo / BCL primero)**: Verificar si la necesidad está cubierta por la Base Class Library (BCL) de .NET (ej. `TimeProvider`, `System.Text.Json`, `System.Security.Cryptography`, `Channels`, `System.Threading.RateLimiting`). Si la solución nativa es suficiente, queda terminantemente prohibido instalar paquetes NuGet de terceros.
2. **Nivel 2 (Open Source de mercado consolidado y mantenido)**: Si la solución nativa no existe o es insuficiente para estándares de producción, recurrir a librerías open-source consolidadas, activamente mantenidas y con amplia adopción en la comunidad (ej. `FastEndpoints`, `Npgsql`, `FluentValidation`). Prohibido añadir paquetes abandonados o de baja calidad.
3. **Nivel 3 (Implementación Ad-Hoc Desacoplada)**: Si no existe solución nativa ni librería de mercado que encaje, implementar la funcionalidad a medida. **Regla de desacople obligatorio**: Toda funcionalidad custom transversal o de infraestructura debe diseñarse y aislarse en un módulo/proyecto separado (ej. librería de utilidades o abstracción desacoplada), protegiendo la pureza del núcleo de negocio y facilitando su reemplazo o testeo unitario.
