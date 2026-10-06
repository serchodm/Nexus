import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { bankingService } from '../lib/services/bankingService';
import type {
  BankInstitution,
  BankConnection,
  BankAuthSessionResult,
  SyncResult,
} from '../types/banking';

export function useBanking(country = 'ES') {
  const queryClient = useQueryClient();

  const institutionsQuery = useQuery<BankInstitution[]>({
    queryKey: ['banking-institutions', country],
    queryFn: () => bankingService.getSupportedInstitutions(country),
    staleTime: 1000 * 60 * 60, // 1 hora
  });

  const connectionsQuery = useQuery<BankConnection[]>({
    queryKey: ['bank-connections'],
    queryFn: () => bankingService.getBankConnections(),
    staleTime: 1000 * 30, // 30 segundos
  });

  const startAuthMutation = useMutation<BankAuthSessionResult, Error, { institutionCode: string; redirectUri?: string }>({
    mutationFn: ({ institutionCode, redirectUri }) =>
      bankingService.startAuthSession(institutionCode, redirectUri),
  });

  const completeCallbackMutation = useMutation<BankConnection, Error, { code: string; sessionId: string }>({
    mutationFn: ({ code, sessionId }) => bankingService.completeCallback(code, sessionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bank-connections'] });
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });

  const syncMutation = useMutation<SyncResult, Error, { connectionId: string; force?: boolean }>({
    mutationFn: ({ connectionId, force }) => bankingService.syncConnection(connectionId, force),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bank-connections'] });
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
    },
  });

  const revokeMutation = useMutation<void, Error, string>({
    mutationFn: (connectionId: string) => bankingService.revokeConnection(connectionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bank-connections'] });
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
  });

  return {
    institutions: institutionsQuery.data ?? [],
    isLoadingInstitutions: institutionsQuery.isLoading,
    connections: connectionsQuery.data ?? [],
    isLoadingConnections: connectionsQuery.isLoading,
    refetchConnections: connectionsQuery.refetch,
    startAuth: startAuthMutation.mutateAsync,
    isStartingAuth: startAuthMutation.isPending,
    completeCallback: completeCallbackMutation.mutateAsync,
    isCompletingCallback: completeCallbackMutation.isPending,
    syncConnection: syncMutation.mutateAsync,
    isSyncing: syncMutation.isPending,
    revokeConnection: revokeMutation.mutateAsync,
    isRevoking: revokeMutation.isPending,
  };
}
