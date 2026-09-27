import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../lib/api-client';
import type { 
  CategorizationRule, 
  CreateRuleRequest, 
  ApplyRetroactiveRequest, 
  ApplyRetroactiveResult 
} from '../types/rule';

export function useRules() {
  const queryClient = useQueryClient();

  const rulesQuery = useQuery({
    queryKey: ['rules'],
    queryFn: async () => {
      return apiClient<CategorizationRule[]>('/api/rules');
    },
  });

  const createRuleMutation = useMutation({
    mutationFn: async (req: CreateRuleRequest) => {
      return apiClient<CategorizationRule>('/api/rules', {
        method: 'POST',
        body: JSON.stringify(req),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rules'] });
    },
  });

  const deleteRuleMutation = useMutation({
    mutationFn: async (ruleId: string) => {
      return apiClient<void>(`/api/rules/${ruleId}`, {
        method: 'DELETE',
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rules'] });
    },
  });

  const applyRetroactiveMutation = useMutation({
    mutationFn: async (req: ApplyRetroactiveRequest = {}) => {
      return apiClient<ApplyRetroactiveResult>('/api/rules/apply-retroactive', {
        method: 'POST',
        body: JSON.stringify(req),
      });
    },
    onSuccess: () => {
      // Invalida transacciones porque pueden haber cambiado sus categorías
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      queryClient.invalidateQueries({ queryKey: ['rules'] });
    },
  });

  return {
    rules: rulesQuery.data ?? [],
    isLoading: rulesQuery.isLoading,
    isError: rulesQuery.isError,
    refetch: rulesQuery.refetch,
    createRule: createRuleMutation.mutateAsync,
    isCreating: createRuleMutation.isPending,
    deleteRule: deleteRuleMutation.mutateAsync,
    isDeleting: deleteRuleMutation.isPending,
    applyRetroactive: applyRetroactiveMutation.mutateAsync,
    isApplyingRetroactive: applyRetroactiveMutation.isPending,
  };
}
