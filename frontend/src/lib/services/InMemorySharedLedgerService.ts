import type { SharedSpaceSummary, SharedExpenseEntry, ShareExpenseInput, SettleBalanceInput } from '../../types/shared-space';

const STORAGE_KEY = 'nexus_shared_space_state';

const DEFAULT_MEMBERS = [
  { id: 'm-1', displayName: 'Sergio', initials: 'OP', avatarColor: 'var(--primary)', role: 'Owner' as const, declaredMonthlyIncome: 2750 },
  { id: 'm-2', displayName: 'Elena', initials: 'PT', avatarColor: 'var(--secondary)', role: 'Member' as const, declaredMonthlyIncome: 2250 },
];

const INITIAL_PENDING: SharedExpenseEntry[] = [
  {
    id: 'exp-1',
    sharedSpaceId: 'sp-1',
    description: 'Mercadona Gran Vía - Provisiones',
    transactionDate: 'Today, 14:28',
    totalAmount: 120.00,
    paidByShare: 60.00,
    owedShare: 60.00,
    paidByMemberId: 'm-1',
    paidByName: 'Sergio',
    categoryName: 'Groceries & Home',
    categoryIcon: 'shopping-cart',
    status: 'pending',
    splitRatio: '50/50',
  },
  {
    id: 'exp-2',
    sharedSpaceId: 'sp-1',
    description: 'Agua + Luz + Gas Oct',
    transactionDate: 'Oct 20',
    totalAmount: 245.80,
    paidByShare: 135.19,
    owedShare: 110.61,
    paidByMemberId: 'm-1',
    paidByName: 'Sergio',
    categoryName: 'Utilities & Services',
    categoryIcon: 'zap',
    status: 'pending',
    splitRatio: '55/45',
  },
];

class InMemorySharedLedger {
  private summary: SharedSpaceSummary;

  constructor() {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) {
      try {
        this.summary = JSON.parse(saved);
        return;
      } catch {
        // Fallback to initial
      }
    }

    this.summary = {
      id: 'sp-1',
      name: 'Hogar Sergio & Elena',
      defaultSplitRatio: '55/45',
      netBalance: 170.61, // 60.00 + 110.61 = 170.61 €
      totalSharedThisMonth: 728.29,
      userPaidThisMonth: 400.56,
      youPaidThisMonth: 400.56,
      partnerPaidThisMonth: 327.73,
      members: DEFAULT_MEMBERS,
      pendingExpenses: INITIAL_PENDING,
    };
    this.save();
  }

  private save() {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(this.summary));
  }

  public getSummary(): SharedSpaceSummary {
    // Recalcular saldo en vivo para garantizar consistencia estricta (FIN-03)
    const net = this.summary.pendingExpenses.reduce((acc, curr) => {
      if (curr.status !== 'pending') return acc;
      return curr.paidByMemberId === 'm-1' ? acc + curr.owedShare : acc - curr.owedShare;
    }, 0);

    this.summary.netBalance = Number(net.toFixed(2));
    return { ...this.summary };
  }

  public shareExpense(input: ShareExpenseInput): SharedExpenseEntry {
    const pct = input.payerPercentage > 0 ? input.payerPercentage : 55;
    const total = input.totalAmount !== undefined && input.totalAmount > 0 ? input.totalAmount : 120.00;
    const payerShare = Number((total * (pct / 100)).toFixed(2));
    const debtorShare = Number((total - payerShare).toFixed(2));

    const newExpense: SharedExpenseEntry = {
      id: `exp-${Date.now()}`,
      sharedSpaceId: input.sharedSpaceId,
      bankTransactionId: input.bankTransactionId,
      transactionSplitItemId: input.transactionSplitItemId,
      description: input.customDescription || 'Gasto Compartido',
      transactionDate: 'Today, Just now',
      totalAmount: total,
      paidByShare: payerShare,
      owedShare: debtorShare,
      paidByMemberId: input.paidByMemberId || 'm-1',
      paidByName: 'Sergio',
      categoryName: 'General',
      categoryIcon: 'users',
      status: 'pending',
      splitRatio: `${pct}/${100 - pct}`,
    };

    this.summary.pendingExpenses = [newExpense, ...this.summary.pendingExpenses];
    this.getSummary();
    this.save();
    return newExpense;
  }

  public settle(_input: SettleBalanceInput): SharedSpaceSummary {
    // Todas las partidas pasan a saldadas (status: 'settled')
    this.summary.pendingExpenses = this.summary.pendingExpenses.map(e => ({
      ...e,
      status: 'settled',
    }));
    this.summary.netBalance = 0.00;
    this.save();
    return this.getSummary();
  }

  public updateSplitRatio(ratio: string): SharedSpaceSummary {
    this.summary.defaultSplitRatio = ratio;
    this.save();
    return this.getSummary();
  }
}

export const inMemorySharedLedger = new InMemorySharedLedger();
