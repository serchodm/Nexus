import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../lib/api-client';
import type { 
  TransactionDetail, 
  TransactionsPagedResult, 
  TransactionsFilter, 
  CategorizeTransactionRequest, 
  SplitTransactionRequest,
  LinkTransactionRequest,
  UpdateTransactionNatureRequest
} from '../types/transaction';

export function useTransactions(filter: TransactionsFilter = {}) {
  const queryClient = useQueryClient();

  const queryParams = new URLSearchParams();
  if (filter.bankAccountId) queryParams.set('bankAccountId', filter.bankAccountId);
  if (filter.categoryId) queryParams.set('categoryId', filter.categoryId);
  if (filter.subcategoryId) queryParams.set('subcategoryId', filter.subcategoryId);
  if (filter.searchTerm) queryParams.set('searchTerm', filter.searchTerm);
  if (filter.fromDate) queryParams.set('fromDate', filter.fromDate);
  if (filter.toDate) queryParams.set('toDate', filter.toDate);
  if (filter.isRecurring !== undefined && filter.isRecurring !== null) {
    queryParams.set('isRecurring', String(filter.isRecurring));
  }
  if (filter.hasSplits !== undefined && filter.hasSplits !== null) {
    queryParams.set('hasSplits', String(filter.hasSplits));
  }
  if (filter.expenseNature) queryParams.set('expenseNature', String(filter.expenseNature));
  if (filter.page) queryParams.set('page', String(filter.page));
  if (filter.pageSize) queryParams.set('pageSize', String(filter.pageSize));

  const queryString = queryParams.toString();
  const endpoint = `/api/transactions${queryString ? `?${queryString}` : ''}`;

  const transactionsQuery = useQuery({
    queryKey: ['transactions', filter],
    queryFn: async () => {
      return apiClient<TransactionsPagedResult>(endpoint);
    },
    enabled: Boolean(filter.bankAccountId),
  });

  const categorizeMutation = useMutation({
    mutationFn: async ({ id, data }: { id: string; data: CategorizeTransactionRequest }) => {
      return apiClient<TransactionDetail>(`/api/transactions/${id}/categorize`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      queryClient.invalidateQueries({ queryKey: ['rules'] });
    },
  });

  const splitMutation = useMutation({
    mutationFn: async ({ id, data }: { id: string; data: SplitTransactionRequest }) => {
      return apiClient<TransactionDetail>(`/api/transactions/${id}/split`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });

  const resetSplitsMutation = useMutation({
    mutationFn: async (id: string) => {
      return apiClient<TransactionDetail>(`/api/transactions/${id}/splits`, {
        method: 'DELETE',
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });

  const linkMutation = useMutation({
    mutationFn: async ({ id, data }: { id: string; data: LinkTransactionRequest }) => {
      return apiClient<TransactionDetail>(`/api/transactions/${id}/link`, {
        method: 'POST',
        body: JSON.stringify(data),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });

  const updateNatureMutation = useMutation({
    mutationFn: async ({ id, data }: { id: string; data: UpdateTransactionNatureRequest }) => {
      return apiClient<TransactionDetail>(`/api/transactions/${id}/nature`, {
        method: 'PATCH',
        body: JSON.stringify(data),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });

  return {
    transactions: transactionsQuery.data?.items ?? [],
    totalCount: transactionsQuery.data?.totalCount ?? 0,
    page: transactionsQuery.data?.page ?? 1,
    pageSize: transactionsQuery.data?.pageSize ?? 50,
    isLoading: transactionsQuery.isLoading,
    isError: transactionsQuery.isError,
    refetch: transactionsQuery.refetch,
    categorizeTransaction: categorizeMutation.mutateAsync,
    isCategorizing: categorizeMutation.isPending,
    splitTransaction: splitMutation.mutateAsync,
    isSplitting: splitMutation.isPending,
    resetSplits: resetSplitsMutation.mutateAsync,
    isResettingSplits: resetSplitsMutation.isPending,
    linkTransaction: linkMutation.mutateAsync,
    isLinking: linkMutation.isPending,
    updateTransactionNature: updateNatureMutation.mutateAsync,
    isUpdatingNature: updateNatureMutation.isPending,
  };
}
