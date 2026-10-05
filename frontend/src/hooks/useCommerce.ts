import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../lib/api-client';
import type {
  ReconciliationCandidate,
  ImportCommerceReportResult,
  AcceptReconciliationResult,
  CommerceOrder,
} from '../types/commerce';

export function useCommerceOrders(provider?: number, page = 1, pageSize = 50) {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  });
  if (provider !== undefined) {
    params.append('provider', provider.toString());
  }

  return useQuery({
    queryKey: ['commerce-orders', provider, page, pageSize],
    queryFn: () => apiClient<CommerceOrder[]>(`/api/commerce/orders?${params.toString()}`),
  });
}

export function useCommerce() {
  const queryClient = useQueryClient();

  const reconciliationsQuery = useQuery({
    queryKey: ['commerce-reconciliations'],
    queryFn: () => apiClient<ReconciliationCandidate[]>('/api/commerce/reconciliations'),
  });

  const importAmazonMutation = useMutation({
    mutationFn: (file: File) => {
      const formData = new FormData();
      formData.append('file', file);
      return apiClient<ImportCommerceReportResult>('/api/commerce/amazon/import-report', {
        method: 'POST',
        body: formData,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['commerce-reconciliations'] });
      queryClient.invalidateQueries({ queryKey: ['commerce-orders'] });
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });

  const importPayPalMutation = useMutation({
    mutationFn: (file: File) => {
      const formData = new FormData();
      formData.append('file', file);
      return apiClient<ImportCommerceReportResult>('/api/commerce/paypal/import-activity', {
        method: 'POST',
        body: formData,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['commerce-reconciliations'] });
      queryClient.invalidateQueries({ queryKey: ['commerce-orders'] });
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });

  const connectPayPalMutation = useMutation({
    mutationFn: (data: { accountEmail: string; accessToken: string; refreshToken?: string; expiresInSeconds?: number }) =>
      apiClient<{ success: boolean; connectionId: string; accountEmail: string; message?: string }>('/api/commerce/paypal/connect', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
  });

  const syncPayPalMutation = useMutation({
    mutationFn: (dates?: { startDate?: string; endDate?: string }) =>
      apiClient<ImportCommerceReportResult>('/api/commerce/paypal/sync', {
        method: 'POST',
        body: JSON.stringify(dates || {}),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['commerce-reconciliations'] });
      queryClient.invalidateQueries({ queryKey: ['commerce-orders'] });
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });

  const acceptReconciliationMutation = useMutation({
    mutationFn: (reconciliationId: string) =>
      apiClient<AcceptReconciliationResult>(`/api/commerce/reconciliations/${reconciliationId}/accept`, {
        method: 'POST',
        body: JSON.stringify({}),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['commerce-reconciliations'] });
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
  });

  const rejectReconciliationMutation = useMutation({
    mutationFn: ({ reconciliationId, reason }: { reconciliationId: string; reason?: string }) =>
      apiClient<{ success: boolean; reconciliationId: string; message?: string }>(`/api/commerce/reconciliations/${reconciliationId}/reject`, {
        method: 'POST',
        body: JSON.stringify({ reason }),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['commerce-reconciliations'] });
    },
  });

  return {
    reconciliations: reconciliationsQuery.data ?? [],
    isLoadingReconciliations: reconciliationsQuery.isLoading,
    refetchReconciliations: reconciliationsQuery.refetch,
    importAmazon: importAmazonMutation,
    importPayPal: importPayPalMutation,
    connectPayPal: connectPayPalMutation,
    syncPayPal: syncPayPalMutation,
    acceptReconciliation: acceptReconciliationMutation,
    rejectReconciliation: rejectReconciliationMutation,
  };
}
