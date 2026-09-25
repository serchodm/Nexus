# Directrices Frontend: React 19 + Vite + TypeScript

## 1. Estándares y Paradigmas de React 19
- **Abandono de `forwardRef`**: Los componentes funcionales reciben la propiedad `ref` directamente en sus props.
- **Hook `use()`**: Usar el hook `use()` para resolver promesas o contextos de forma concisa cuando aplique.
- **Metadatos Nativos en Componentes**: Usar etiquetas estándar `<title>`, `<meta>` y `<link rel="canonical">` directamente dentro de los componentes funcionales; React 19 las eleva automáticamente al `<head>`.

---

## 2. Delimitación Estricta: React 19 Actions vs. TanStack Query
Para evitar conflictos de sincronización de caché entre las acciones nativas de React 19 y el estado del servidor:

- **TanStack Query (`useQuery` / `useMutation`)**:
  - **Obligatorio para todo el ciclo de datos CRUD del servidor**: Cualquier operación que cree, modifique o elimine datos que se muestran en listas o vistas compartidas debe usar `useMutation`.
  - Permite invalidación precisa de caché (`queryClient.invalidateQueries`), *Optimistic Updates* coherentes y control global de reintentos y estados de error.
  - Configurar políticas de `staleTime` para amortiguar peticiones al backend en alto volumen de lectura.

- **React 19 Actions (`useActionState` / `useFormStatus` / `useOptimistic`)**:
  - **Reservado para flujos locales y formularios desacoplados de la caché compartida**:
    - Pantalla de inicio de sesión y formulario de verificación del código TOTP de 6 dígitos.
    - Asistentes/Wizards por pasos sin persistencia intermedia en el servidor.
    - Acciones puramente de interfaz local.
  - Usar `useFormStatus` en botones de submit secundarios para deshabilitación automática y estados de carga.

---

## 3. Tipado Estricto con TypeScript
- **Cero tolerancia a `any`**:
  - Emplear interfaces/types explícitos sincronizados con los DTOs de C#.
  - Usar `unknown` con type guards para datos externos o respuestas dinámicas.
- **Estructura de Componentes**:
  - Separar componentes de presentación puros de los custom hooks que consumen la API.
  - Tipar explícitamente props mediante interfaces dedicadas (`interface Props { ... }`).

---

## 4. Flujo de Autenticación Seguro: Google OAuth + MFA (TOTP)
- **Google Identity Services**:
  - Obtener el `credential` emitido por Google y enviarlo al endpoint `/api/auth/google`.
- **Manejo del Estado MFA**:
  - Si el backend responde con estado `mfa_required` y token temporal de 5 minutos, la UI redirige de inmediato a la pantalla de verificación TOTP.
  - Componente de entrada para código de 6 dígitos con foco automático, soporte para pegar código completo y reintento con mensaje claro de error.
  - Envío al endpoint `/api/auth/mfa/verify` para completar la autenticación.
- **Gestión Segura de Tokens (Protección contra XSS)**:
  - **Access Token**: Almacenado exclusivamente **en memoria** (estado reactivo en Zustand, React Context o variable de módulo). Prohibido guardarlo en `localStorage` o `sessionStorage`.
  - **Refresh Token**: Transparente e invisible para JavaScript. El navegador lo envía y recibe automáticamente a través de la Cookie `HttpOnly`.
  - **Cliente HTTP con Credenciales**:
    - Configurar el cliente HTTP (Axios / Fetch) con `withCredentials: true` (o `credentials: 'include'`) para enviar la cookie en las llamadas de refresco.
    - Interceptor de respuesta ante `401 Unauthorized`: invocar `/api/auth/refresh` de forma transparente (*Silent Refresh*), actualizar el Access Token en memoria y reintentar la petición original fallida.

---

## 5. Jerarquía Estricta de Dependencias e Implementación en Frontend
1. **Nivel 1 (Estándar Web y React 19 nativo)**: Priorizar siempre APIs web estándar (`fetch`, `FormData`, `URLSearchParams`, `Intl`, `crypto.randomUUID()`) y hooks nativos de React 19 antes de instalar paquetes de npm.
2. **Nivel 2 (Open Source de mercado consolidado y mantenido)**: Utilizar paquetes consolidados, con mantenimiento activo y tipado TypeScript de primer nivel (ej. `TanStack Query`, `lucide-react`, `qrcode.react`, `clsx`/`tailwind-merge`). Prohibido instalar micro-librerías o dependencias con vulnerabilidades/abandono.
3. **Nivel 3 (Implementación Ad-Hoc Desacoplada)**: Toda utilidad o lógica custom debe aislarse en módulos puros en `src/lib/` o `src/utils/`, completamente independientes del árbol de componentes de UI para garantizar su testabilidad y desacoplamiento.
