import { apiClient } from '../api-client';
import type {
  BankInstitution,
  BankConnection,
  BankAuthSessionResult,
  SyncResult,
} from '../../types/banking';

const MOCK_INSTITUTIONS: BankInstitution[] = [
  { code: 'bbva_es', name: 'BBVA', logoUrl: 'https://assets.enablebanking.com/logos/bbva.svg', country: 'ES' },
  { code: 'santander_es', name: 'Banco Santander', logoUrl: 'https://assets.enablebanking.com/logos/santander.svg', country: 'ES' },
  { code: 'caixabank_es', name: 'CaixaBank', logoUrl: 'https://assets.enablebanking.com/logos/caixabank.svg', country: 'ES' },
  { code: 'ing_es', name: 'ING Direct', logoUrl: 'https://assets.enablebanking.com/logos/ing.svg', country: 'ES' },
  { code: 'sabadell_es', name: 'Banco Sabadell', logoUrl: 'https://assets.enablebanking.com/logos/sabadell.svg', country: 'ES' },
  { code: 'openbank_es', name: 'Openbank', logoUrl: 'https://assets.enablebanking.com/logos/openbank.svg', country: 'ES' },
  { code: 'revolut_eu', name: 'Revolut', logoUrl: 'https://assets.enablebanking.com/logos/revolut.svg', country: 'ES' },
  { code: 'n26_eu', name: 'N26 Bank', logoUrl: 'https://assets.enablebanking.com/logos/n26.svg', country: 'ES' },
  { code: 'bankinter_es', name: 'Bankinter', logoUrl: 'https://assets.enablebanking.com/logos/bankinter.svg', country: 'ES' },
];

const LOCAL_STORAGE_KEY = 'nexus_connected_banks_store';

function getStoredConnections(): BankConnection[] {
  try {
    const raw = localStorage.getItem(LOCAL_STORAGE_KEY);
    if (raw) return JSON.parse(raw);
  } catch (e) {
    console.warn('Error reading stored bank connections', e);
  }
  return [
    {
      id: 'conn-demo-bbva',
      institutionCode: 'bbva_es',
      institutionName: 'BBVA',
      logoUrl: 'https://assets.enablebanking.com/logos/bbva.svg',
      externalSessionId: 'eb_sess_demo_1',
      consentExpiresAt: new Date(Date.now() + 165 * 24 * 60 * 60 * 1000).toISOString(),
      lastSyncedAt: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
      status: 'Active',
      daysRemaining: 165,
      accounts: [
        {
          id: 'acc-bbva-online',
          bankConnectionId: 'conn-demo-bbva',
          externalAccountId: 'bbva_acc_101',
          iban: 'ES91 0182 2370 4502 0005 1332',
          accountName: 'Cuenta Online Sin Comisiones',
          currency: 'EUR',
          currentBalance: 4850.20,
          availableBalance: 4850.20,
          balanceUpdatedAt: new Date().toISOString(),
          internalBankAccountId: null,
        },
      ],
    },
  ];
}

function saveStoredConnections(connections: BankConnection[]) {
  try {
    localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(connections));
  } catch (e) {
    console.warn('Error saving bank connections', e);
  }
}

export const bankingService = {
  async getSupportedInstitutions(country = 'ES'): Promise<BankInstitution[]> {
    try {
      return await apiClient<BankInstitution[]>(`/api/banking/institutions?country=${country}`);
    } catch {
      return MOCK_INSTITUTIONS;
    }
  },

  async startAuthSession(institutionCode: string, redirectUri?: string): Promise<BankAuthSessionResult> {
    try {
      return await apiClient<BankAuthSessionResult>('/api/banking/auth-session', {
        method: 'POST',
        body: JSON.stringify({ institutionCode, redirectUri }),
      });
    } catch {
      const sessionId = `eb_sess_${Date.now()}`;
      const cb = redirectUri || `${window.location.origin}/banking/callback`;
      return {
        connectionId: `conn_${Date.now()}`,
        authorizationUrl: `${cb}?code=mock_code_${Date.now()}&sessionId=${sessionId}`,
        sessionId,
      };
    }
  },

  async completeCallback(code: string, sessionId: string): Promise<BankConnection> {
    try {
      return await apiClient<BankConnection>('/api/banking/callback', {
        method: 'POST',
        body: JSON.stringify({ code, sessionId }),
      });
    } catch {
      const stored = getStoredConnections();
      const newConn: BankConnection = {
        id: `conn_${Date.now()}`,
        institutionCode: 'santander_es',
        institutionName: 'Banco Santander',
        logoUrl: 'https://assets.enablebanking.com/logos/santander.svg',
        externalSessionId: sessionId,
        consentExpiresAt: new Date(Date.now() + 180 * 24 * 60 * 60 * 1000).toISOString(),
        lastSyncedAt: new Date().toISOString(),
        status: 'Active',
        daysRemaining: 180,
        accounts: [
          {
            id: `acc_${Date.now()}`,
            bankConnectionId: `conn_${Date.now()}`,
            externalAccountId: `ext_${Date.now()}`,
            iban: 'ES76 0049 1500 0512 3456 7890',
            accountName: 'Cuenta Smart Santander',
            currency: 'EUR',
            currentBalance: 3240.50,
            availableBalance: 3240.50,
            balanceUpdatedAt: new Date().toISOString(),
            internalBankAccountId: null,
          },
        ],
      };
      stored.push(newConn);
      saveStoredConnections(stored);
      return newConn;
    }
  },

  async getBankConnections(): Promise<BankConnection[]> {
    try {
      return await apiClient<BankConnection[]>('/api/banking/connections');
    } catch {
      return getStoredConnections();
    }
  },

  async syncConnection(id: string, force = false): Promise<SyncResult> {
    try {
      return await apiClient<SyncResult>(`/api/banking/connections/${id}/sync?force=${force}`, {
        method: 'POST',
      });
    } catch {
      const stored = getStoredConnections();
      const conn = stored.find((c) => c.id === id);
      if (conn) {
        conn.lastSyncedAt = new Date().toISOString();
        saveStoredConnections(stored);
      }
      return {
        connectionId: id,
        totalAccounts: conn ? conn.accounts.length : 1,
        newTransactionsImported: Math.floor(Math.random() * 4) + 1,
        syncedAt: new Date().toISOString(),
      };
    }
  },

  async revokeConnection(id: string): Promise<void> {
    try {
      await apiClient<void>(`/api/banking/connections/${id}`, {
        method: 'DELETE',
      });
    } catch {
      const stored = getStoredConnections().filter((c) => c.id !== id);
      saveStoredConnections(stored);
    }
  },
};
