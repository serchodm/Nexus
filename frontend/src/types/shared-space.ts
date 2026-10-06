export interface SharedSpaceMember {
  id: string;
  displayName: string;
  initials: string;
  avatarColor: string;
  role: 'Owner' | 'Member';
  declaredMonthlyIncome?: number;
}

export interface SharedExpenseEntry {
  id: string;
  sharedSpaceId: string;
  bankTransactionId?: string;
  transactionSplitItemId?: string;
  description: string;
  transactionDate: string;
  totalAmount: number;
  paidByShare: number;
  owedShare: number;
  paidByMemberId: string;
  paidByName: string;
  categoryName: string;
  categoryIcon: string;
  status: 'pending' | 'settled';
  splitRatio?: string;
}

export interface SharedSpaceSummary {
  id: string;
  name: string;
  defaultSplitRatio: string; // ej. "55/45"
  netBalance: number; // Positivo: pareja te debe dinero; Negativo: debes dinero
  totalSharedThisMonth: number;
  youPaidThisMonth: number;
  partnerPaidThisMonth: number;
  members: SharedSpaceMember[];
  pendingExpenses: SharedExpenseEntry[];
}

export interface ShareExpenseInput {
  sharedSpaceId: string;
  bankTransactionId: string;
  transactionSplitItemId?: string;
  paidByMemberId: string;
  payerPercentage: number;
  customDescription?: string;
  totalAmount?: number;
}

export interface SettleBalanceInput {
  sharedSpaceId: string;
  payerMemberId: string;
  receiverMemberId: string;
  amount: number;
  method: 'Bizum' | 'SepaTransfer' | 'Cash' | 'Offset';
  referenceCode?: string;
}

export interface UpdateSplitRatioInput {
  sharedSpaceId: string;
  ruleType: 'Equal5050' | 'FixedRatio' | 'ProportionalToIncome';
  ownerPercentage: number;
  ownerIncome?: number;
  memberIncome?: number;
}
