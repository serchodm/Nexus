import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../lib/api-client';
import { inMemorySharedLedger } from '../lib/services/InMemorySharedLedgerService';
import type {
  SharedSpaceSummary,
  SharedExpenseEntry,
  ShareExpenseInput,
  SettleBalanceInput,
  UpdateSplitRatioInput,
} from '../types/shared-space';

export function useSharedSpace() {
  const queryClient = useQueryClient();

  // Query: Resumen reactivo del espacio compartido
  const spaceQuery = useQuery({
    queryKey: ['shared-space'],
    queryFn: async (): Promise<SharedSpaceSummary> => {
      try {
        return await apiClient<SharedSpaceSummary>('/api/shared-spaces/active');
      } catch {
        // Fallback a ledger reactivo en memoria (Executive Sandbox)
        return inMemorySharedLedger.getSummary();
      }
    },
    staleTime: 1000 * 30, // 30 segundos
  });

  // Mutación: Imputar transacción al espacio común
  const shareExpenseMutation = useMutation({
    mutationFn: async (input: ShareExpenseInput): Promise<SharedExpenseEntry> => {
      try {
        return await apiClient<SharedExpenseEntry>(`/api/shared-spaces/${input.sharedSpaceId}/share`, {
          method: 'POST',
          body: JSON.stringify(input),
        });
      } catch {
        return inMemorySharedLedger.shareExpense(input);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['shared-space'] });
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
  });

  // Mutación: Liquidación Bizum / SEPA
  const settleMutation = useMutation({
    mutationFn: async (input: SettleBalanceInput): Promise<SharedSpaceSummary> => {
      try {
        await apiClient(`/api/shared-spaces/${input.sharedSpaceId}/settle`, {
          method: 'POST',
          body: JSON.stringify(input),
        });
        return inMemorySharedLedger.settle(input);
      } catch {
        return inMemorySharedLedger.settle(input);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['shared-space'] });
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
  });

  // Mutación: Actualizar ratio de reparto
  const updateRatioMutation = useMutation({
    mutationFn: async (input: UpdateSplitRatioInput): Promise<void> => {
      const ratioStr = `${input.ownerPercentage}/${100 - input.ownerPercentage}`;
      try {
        await apiClient(`/api/shared-spaces/${input.sharedSpaceId}/ratios`, {
          method: 'PATCH',
          body: JSON.stringify(input),
        });
      } catch {
        inMemorySharedLedger.updateSplitRatio(ratioStr);
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['shared-space'] });
    },
  });

  const summary = spaceQuery.data ?? inMemorySharedLedger.getSummary();

  return {
    summary,
    isLoading: spaceQuery.isLoading,
    isError: spaceQuery.isError,
    refetch: spaceQuery.refetch,

    shareExpense: shareExpenseMutation.mutateAsync,
    isSharing: shareExpenseMutation.isPending,

    settleBalance: settleMutation.mutateAsync,
    isSettling: settleMutation.isPending,

    updateRatio: updateRatioMutation.mutateAsync,
    isUpdatingRatio: updateRatioMutation.isPending,
  };
}
