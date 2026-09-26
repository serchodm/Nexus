import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../lib/api-client';
import type { BankAccount, CreateBankAccountRequest, GetBankAccountsResult } from '../types/account';

export function useAccounts() {
  const queryClient = useQueryClient();

  const accountsQuery = useQuery({
    queryKey: ['accounts'],
    queryFn: () => apiClient<GetBankAccountsResult>('/api/accounts'),
    staleTime: 1000 * 60 * 2, // 2 minutos de frescura
  });

  const createAccountMutation = useMutation({
    mutationFn: (data: CreateBankAccountRequest) =>
      apiClient<BankAccount>('/api/accounts', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
  });

  return {
    accounts: accountsQuery.data?.accounts ?? [],
    totalNetWorthShare: accountsQuery.data?.totalNetWorthShare ?? 0,
    isLoading: accountsQuery.isLoading,
    isError: accountsQuery.isError,
    error: accountsQuery.error,
    refetch: accountsQuery.refetch,
    createAccount: createAccountMutation.mutateAsync,
    isCreating: createAccountMutation.isPending,
  };
}
