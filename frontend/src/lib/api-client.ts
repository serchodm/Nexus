import { authStore } from './auth-store';
import type { AuthSuccessResponse } from '../types/auth';

let isRefreshing = false;
let refreshQueue: Array<(token: string | null) => void> = [];

function processQueue(token: string | null) {
  refreshQueue.forEach((callback) => callback(token));
  refreshQueue = [];
}

/**
 * Cliente HTTP ligero y tipado sobre fetch nativo.
 * Cumple con:
 * - Nivel 1 de dependencias (Web APIs nativas)
 * - credentials: 'include' para Cookies HttpOnly
 * - Silent Refresh seguro con cola de peticiones concurrentes
 */
export async function apiClient<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const url = endpoint.startsWith('http') ? endpoint : endpoint;
  const headers = new Headers(options.headers || {});

  if (!headers.has('Content-Type') && !(options.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json');
  }

  const token = authStore.getAccessToken();
  if (token && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  const config: RequestInit = {
    ...options,
    headers,
    credentials: 'include', // Requerido para envío de cookies HttpOnly de Refresh Token
  };

  const response = await fetch(url, config);

  // Manejo de expiración de token y Silent Refresh
  if (response.status === 401 && !endpoint.includes('/api/auth/refresh') && !endpoint.includes('/api/auth/google')) {
    if (isRefreshing) {
      // Si ya hay un refresco en curso, encolamos esta petición para evitar llamadas concurrentes a /refresh
      return new Promise<T>((resolve, reject) => {
        refreshQueue.push((newToken) => {
          if (newToken) {
            headers.set('Authorization', `Bearer ${newToken}`);
            resolve(apiClient<T>(endpoint, { ...options, headers }));
          } else {
            reject(new Error('Sesión expirada'));
          }
        });
      });
    }

    isRefreshing = true;

    try {
      const refreshResponse = await fetch('/api/auth/refresh', {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
      });

      if (!refreshResponse.ok) {
        authStore.clearSession();
        processQueue(null);
        throw new Error('No se pudo renovar la sesión');
      }

      const data: AuthSuccessResponse = await refreshResponse.json();
      authStore.setSession(data.accessToken, data.user);
      processQueue(data.accessToken);

      // Reintentar la petición original con el nuevo token
      headers.set('Authorization', `Bearer ${data.accessToken}`);
      return apiClient<T>(endpoint, { ...options, headers });
    } catch (error) {
      authStore.clearSession();
      processQueue(null);
      throw error;
    } finally {
      isRefreshing = false;
    }
  }

  if (!response.ok) {
    let errorMessage = `Error HTTP ${response.status}`;
    try {
      const errorJson = await response.json();
      errorMessage = errorJson.message || errorJson.title || errorMessage;
    } catch {
      // Cuerpo no JSON
    }
    throw new Error(errorMessage);
  }

  if (response.status === 204) {
    return {} as T;
  }

  return response.json();
}
