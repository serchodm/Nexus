# Nexus — Modern SaaS Boilerplate & Starter Template

Plantilla empresarial moderna, de alto rendimiento y lista para producción, diseñada con **Clean Architecture** en **.NET 10** y frontend desacoplado en **React 19 + TypeScript**, con soporte híbrido para **Web (Desktop/Mobile)** y aplicaciones móviles nativas para **Android** e **iOS** mediante **Capacitor**.

---

## 🏗️ Arquitectura del Sistema

El proyecto está diseñado bajo estrictos patrones de ingeniería y separación de responsabilidades:

```text
Nexus/
├── src/                                  # Backend (.NET 10 - Clean Architecture)
│   ├── Nexus.Domain/                     # Entidades, Contratos de Interfaz, Enums, Reglas de Dominio
│   ├── Nexus.Application/                # Comandos CQRS, DTOs, Casos de Uso, Validaciones Fluent
│   ├── Nexus.Infrastructure/             # EF Core 10, PostgreSQL 17, JWT, Totp.NET, Google OAuth
│   └── Nexus.Api/                        # FastEndpoints 8.3, Swagger OpenAPI, Endpoints REST
├── frontend/                             # Frontend & Mobile Shell (React 19 + TypeScript)
│   ├── src/
│   │   ├── components/                   # UI modular, Navbar, Footer, AuthSection, TOTP 6-digit input
│   │   ├── hooks/                        # Custom Hooks con TanStack React Query
│   │   ├── lib/                          # api-client (Silent Refresh & Capacitor), auth-store (In-memory)
│   │   ├── types/                        # Tipado estricto TypeScript
│   │   └── index.css                     # Sistema de diseño con variables HSL, Glassmorphism y Dark Theme
│   ├── capacitor.config.ts               # Configuración multiplataforma Android / iOS
│   └── vite.config.ts                    # Bundler ultra-rápido con proxy local
└── docker-compose.yml                    # PostgreSQL 17 con healthchecks automáticos
```

---

## 🔐 Seguridad y Autenticación Híbrida

Nexus implementa un modelo de autenticación dual adaptativo:

1. **Clientes Web (Navegador Desktop & Móvil):**
   - **Access Token:** Con vigencia corta (15 minutos), almacenado **estrictamente en memoria** en `authStore` (inmune a vectores de ataque XSS).
   - **Refresh Token:** Transmitido y gestionado mediante cookies seguras `HttpOnly`, `SameSite=Lax`, `Path=/api/auth` (inmune a robo por JavaScript).
   - **Silent Refresh:** Cola de peticiones concurrentes y rotación criptográfica estricta de tokens en cada refresco.

2. **Clientes Móviles Nativos (Android & iOS con Capacitor):**
   - Detección automática mediante cabecera `X-Client-Platform: android | ios`.
   - El backend emite el `RefreshToken` en el cuerpo de la respuesta JSON para que el cliente nativo pueda persistirlo de forma cifrada (Keystore / Keychain).
   - Endpoints de renovación (`/api/auth/refresh`) y logout (`/api/auth/logout`) aceptan el token en cuerpo JSON cuando no hay soporte de cookies entre dominios nativos.

3. **MFA / 2FA (Google Authenticator / RFC 6238 TOTP):**
   - Enrolamiento mediante URI estándar `otpauth://` y generación de código QR en frontend con `qrcode.react`.
   - Tokens temporales de desafío con scope limitado (`mfa_pending`) de 5 minutos de vigencia.
   - **Protección Anti-Replay:** Prevención de reutilización del mismo código dentro de la ventana temporal de tolerancia.

---

## 🚀 Inicio Rápido en Desarrollo Local

### Requisitos Previos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v20+)
- [Docker Desktop](https://www.docker.com/) (para PostgreSQL 17)

### 1. Iniciar Base de Datos
```bash
docker compose up -d
```
Verifica que el contenedor `nexus-postgres` esté corriendo en el puerto `5432`.

### 2. Ejecutar Migraciones y Backend
```bash
# Aplicar migraciones con dotnet-ef
dotnet ef database update --project src/Nexus.Infrastructure --startup-project src/Nexus.Api

# Iniciar la API
dotnet run --project src/Nexus.Api
```
La API estará disponible en `http://localhost:5276` y su documentación interactiva Swagger en `http://localhost:5276/swagger`.

### 3. Iniciar Frontend
```bash
cd frontend
npm install
npm run dev
```
La aplicación web se ejecutará en `http://localhost:5173` conectada transparentemente al backend local.

---

## 📱 Compilación para Android e iOS con Capacitor

Para empaquetar el frontend como aplicación nativa móvil:

```bash
cd frontend

# 1. Compilar los assets web de producción
npm run build

# 2. Agregar las plataformas nativas deseadas (ejecutar una única vez)
npx cap add android
npx cap add ios

# 3. Sincronizar cambios web hacia los proyectos nativos
npm run cap:sync

# 4. Abrir en Android Studio o Xcode
npm run cap:android   # Abre Android Studio
npm run cap:ios       # Abre Xcode (requiere macOS)
```

---

## 📋 Cómo Usar este Repositorio como Plantilla ("Template Repository")

1. Ve a la página del repositorio en GitHub: [https://github.com/serchodm/Nexus](https://github.com/serchodm/Nexus).
2. Entra en **Settings** -> sección **General**.
3. Marca la casilla **Template repository**.
4. ¡Listo! Ahora, cada vez que quieras crear un nuevo proyecto (de finanzas, salud, fitness, e-commerce, etc.), simplemente haz clic en el botón verde **Use this template** -> **Create a new repository**.
