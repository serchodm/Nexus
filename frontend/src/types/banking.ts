export interface BankInstitution {
  code: string;
  name: string;
  logoUrl: string | null;
  country: string;
}

export interface ConnectedBankAccount {
  id: string;
  bankConnectionId: string;
  externalAccountId: string;
  iban: string | null;
  accountName: string;
  currency: string;
  currentBalance: number | null;
  availableBalance: number | null;
  balanceUpdatedAt: string | null;
  internalBankAccountId: string | null;
}

export type BankConnectionStatus =
  | 'PendingAuthorization'
  | 'Active'
  | 'ExpiringSoon'
  | 'Expired'
  | 'Revoked'
  | 'Error';

export interface BankConnection {
  id: string;
  institutionCode: string;
  institutionName: string;
  logoUrl: string | null;
  externalSessionId: string | null;
  consentExpiresAt: string | null;
  lastSyncedAt: string | null;
  status: BankConnectionStatus;
  daysRemaining: number;
  accounts: ConnectedBankAccount[];
}

export interface BankAuthSessionResult {
  connectionId: string;
  authorizationUrl: string;
  sessionId: string;
}

export interface SyncResult {
  connectionId: string;
  totalAccounts: number;
  newTransactionsImported: number;
  syncedAt: string;
}
