export type AccountType = 1 | 2 | 3; // 1 = Checking, 2 = Savings, 3 = Investment
export type HolderRole = 1 | 2 | 3 | 4; // 1 = Owner, 2 = JointOwner, 3 = Authorized, 4 = LegalGuardian
export type BankType = 0 | 1 | 2 | 3 | 4; // 0 = Generic, 1 = BBVA, 2 = ING, 3 = Santander, 4 = CaixaBank

export const HolderRoleLabels: Record<HolderRole, string> = {
  1: 'Titular Único',
  2: 'Cotitular',
  3: 'Apoderado / Autorizado',
  4: 'Tutor / Cuenta Menor',
};

export interface BankAccount {
  id: string;
  userId?: string;
  name: string;
  bankName: string;
  bankCode?: string | null;
  bankType: BankType;
  iban?: string | null;
  ibanMasked?: string | null;
  last4Digits: string;
  currency: string;
  type: AccountType;
  holderRole: HolderRole;
  ownershipPercentage: number;
  currentBalance?: number | null;
  userNetWorthShare?: number | null;
  balanceLastUpdatedAt?: string | null;
  linkedCardsLast4?: string[] | null;
  createdAt: string;
}

export interface GetBankAccountsResult {
  accounts: BankAccount[];
  totalNetWorthShare: number;
}

export interface CreateBankAccountRequest {
  name: string;
  iban?: string | null;
  type: AccountType;
  holderRole: HolderRole;
  ownershipPercentage?: number | null;
  linkedCardsLast4?: string[] | null;
}
