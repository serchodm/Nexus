import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../lib/api-client';
import type {
  GetAccountTransactionsResult,
  StatementPreview,
  ImportSummary,
} from '../types/statement';

export function useStatements(accountId?: string) {
  const queryClient = useQueryClient();

  const useTransactions = (page = 1, pageSize = 50, fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);

    return useQuery({
      queryKey: ['transactions', accountId, page, pageSize, fromDate, toDate],
      queryFn: () =>
        apiClient<GetAccountTransactionsResult>(
          `/api/accounts/${accountId}/transactions?${params.toString()}`
        ),
      enabled: !!accountId,
    });
  };

  const previewMutation = useMutation({
    mutationFn: ({ accountId, file }: { accountId: string; file: File }) => {
      const formData = new FormData();
      formData.append('file', file);

      return apiClient<StatementPreview>(`/api/accounts/${accountId}/statements/preview`, {
        method: 'POST',
        body: formData,
      });
    },
  });

  const confirmMutation = useMutation({
    mutationFn: ({ accountId, previewToken }: { accountId: string; previewToken: string }) =>
      apiClient<ImportSummary>(`/api/accounts/${accountId}/statements/confirm`, {
        method: 'POST',
        body: JSON.stringify({ previewToken }),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
      if (accountId) {
        queryClient.invalidateQueries({ queryKey: ['transactions', accountId] });
      }
    },
  });

  const rollbackMutation = useMutation({
    mutationFn: (importId: string) =>
      apiClient<void>(`/api/statements/${importId}`, {
        method: 'DELETE',
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });

  return {
    useTransactions,
    previewStatement: previewMutation.mutateAsync,
    isPreviewing: previewMutation.isPending,
    previewError: previewMutation.error,
    confirmImport: confirmMutation.mutateAsync,
    isConfirming: confirmMutation.isPending,
    confirmError: confirmMutation.error,
    rollbackImport: rollbackMutation.mutateAsync,
    isRollingBack: rollbackMutation.isPending,
  };
}
