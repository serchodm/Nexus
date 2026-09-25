import { useSyncExternalStore } from 'react';
import type { UserProfile } from '../types/auth';

interface AuthState {
  accessToken: string | null;
  user: UserProfile | null;
  isAuthenticated: boolean;
  isInitialLoading: boolean;
}

let state: AuthState = {
  accessToken: null,
  user: null,
  isAuthenticated: false,
  isInitialLoading: true,
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
   * Almacena el Access Token EXCLUSIVAMENTE en memoria.
   * Regla de seguridad: Cero persistencia en localStorage/sessionStorage.
   */
  setSession(accessToken: string, user: UserProfile) {
    state = {
      accessToken,
      user,
      isAuthenticated: true,
      isInitialLoading: false,
    };
    emitChange();
  },

  clearSession() {
    state = {
      accessToken: null,
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
};

export function useAuth(): AuthState {
  return useSyncExternalStore(authStore.subscribe, authStore.getSnapshot);
}
