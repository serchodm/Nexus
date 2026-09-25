import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../lib/api-client';
import type { HealthResponse } from '../types/api';

export function useHealthCheck() {
  return useQuery({
    queryKey: ['health'],
    queryFn: () => apiClient<HealthResponse>('/api/health'),
    refetchInterval: 10000, // Revalida cada 10 segundos
  });
}
