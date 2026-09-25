import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 minutos de validez de caché para alto rendimiento
      gcTime: 1000 * 60 * 30, // 30 minutos de persistencia en memoria
      retry: (failureCount, error) => {
        // No reintentar ante errores 401/403/404
        if (error instanceof Error && error.message.includes('401')) return false;
        return failureCount < 2;
      },
      refetchOnWindowFocus: false,
    },
  },
});
