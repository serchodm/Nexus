import type { 
  CategorizationSource, 
  ExpenseNature, 
  ExpenseFlexibility, 
  ExternalPlatform 
} from './category';

export type RecurrencePeriod = 0 | 1 | 2 | 3 | 4; // 0=None, 1=Weekly, 2=Monthly, 3=Quarterly, 4=Annual

export interface SplitItem {
  id: string;
  categoryId: string;
  categoryName: string;
  subcategoryId?: string | null;
  subcategoryName?: string | null;
  amount: number;
  description?: string | null;
  expenseNature: ExpenseNature;
}

export interface SplitItemInput {
  categoryId: string;
  subcategoryId?: string | null;
  amount: number;
  description?: string | null;
  expenseNature: ExpenseNature;
}

export interface TransactionDetail {
  id: string;
  bankAccountId: string;
  operationDate: string;
  valueDate?: string | null;
  description: string;
  cleanMerchantName?: string | null;
  amount: number;
  currency: string;
  balanceAfter?: number | null;
  categoryId?: string | null;
  categoryName?: string | null;
  categoryColor?: string | null;
  categoryIcon?: string | null;
  subcategoryId?: string | null;
  subcategoryName?: string | null;
  categorizationSource: CategorizationSource;
  isManual: boolean;
  isRecurring: boolean;
  recurrencePeriod: RecurrencePeriod;
  expenseFlexibility: ExpenseFlexibility;
  expenseNature: ExpenseNature;
  externalPlatform: ExternalPlatform;
  pendingPlatformEnrichment: boolean;
  hasSplits: boolean;
  splitItems: SplitItem[];
}

export interface TransactionsPagedResult {
  items: TransactionDetail[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface TransactionsFilter {
  bankAccountId?: string | null;
  categoryId?: string | null;
  subcategoryId?: string | null;
  searchTerm?: string | null;
  fromDate?: string | null;
  toDate?: string | null;
  isRecurring?: boolean | null;
  hasSplits?: boolean | null;
  expenseNature?: ExpenseNature | null;
  page?: number;
  pageSize?: number;
}

export interface SplitTransactionRequest {
  items: SplitItemInput[];
}

export interface CategorizeTransactionRequest {
  categoryId: string;
  subcategoryId?: string | null;
  expenseNature?: ExpenseNature | null;
  expenseFlexibility?: ExpenseFlexibility | null;
  isRecurring?: boolean | null;
  createRuleForMerchant?: boolean;
}
