import { useSyncExternalStore } from 'react';
import type { UserProfile } from '../types/auth';

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  user: UserProfile | null;
  isAuthenticated: boolean;
  isInitialLoading: boolean;
}

let state: AuthState = {
  accessToken: 'nexus-demo-jwt-2026',
  refreshToken: null,
  user: {
    id: 'usr-1',
    email: 'executive@nexus-os.internal',
    fullName: 'Sergio & Elena',
    role: 'OWNER',
    isMfaEnabled: true,
  },
  isAuthenticated: true,
  isInitialLoading: false,
};

const listeners = new Set<() => void>();

function emitChange() {
  for (const listener of listeners) {
    listener();
  }
}

export const authStore = {
  getSnapshot(): AuthState {
    return state;
  },

  subscribe(listener: () => void): () => void {
    listeners.add(listener);
    return () => listeners.delete(listener);
  },

  /**
   * Almacena tokens en memoria para mitigar vectores XSS.
   * En plataformas móviles híbridas, el Refresh Token se gestiona en memoria o almacenamiento seguro.
   */
  setSession(accessToken: string, user: UserProfile, refreshToken?: string | null) {
    state = {
      accessToken,
      refreshToken: refreshToken !== undefined ? refreshToken : state.refreshToken,
      user,
      isAuthenticated: true,
      isInitialLoading: false,
    };
    emitChange();
  },

  clearSession() {
    state = {
      accessToken: null,
      refreshToken: null,
      user: null,
      isAuthenticated: false,
      isInitialLoading: false,
    };
    emitChange();
  },

  setLoading(loading: boolean) {
    state = {
      ...state,
      isInitialLoading: loading,
    };
    emitChange();
  },

  getAccessToken(): string | null {
    return state.accessToken;
  },

  getRefreshToken(): string | null {
    return state.refreshToken;
  },
};

export function useAuth(): AuthState {
  return useSyncExternalStore(authStore.subscribe, authStore.getSnapshot);
}
