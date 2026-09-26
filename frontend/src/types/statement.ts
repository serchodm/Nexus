import type { BankType } from './account';

export interface StatementPreviewRow {
  operationDate: string;
  description: string;
  amount: number;
  balanceAfter?: number | null;
  bankCategory?: string | null;
}

export interface StatementPreview {
  previewToken: string;
  detectedBankType: BankType;
  detectedBankName: string;
  startingBalance: number;
  endingBalance: number;
  startDate: string;
  endDate: string;
  totalRecords: number;
  totalIncome: number;
  totalExpense: number;
  warnings: string[];
  previewRows: StatementPreviewRow[];
}

export interface ImportSummary {
  importId: string;
  bankAccountId: string;
  fileName: string;
  totalRecords: number;
  importedRecords: number;
  skippedDuplicates: number;
  finalAccountBalance: number;
}

export interface BankTransaction {
  id: string;
  bankAccountId: string;
  statementImportId: string;
  operationDate: string;
  valueDate?: string | null;
  description: string;
  amount: number;
  currency: string;
  balanceAfter?: number | null;
  bankCategory?: string | null;
  bankSubcategory?: string | null;
  bankMovementType?: string | null;
  observations?: string | null;
  fingerprint: string;
  createdAt: string;
}

export interface GetAccountTransactionsResult {
  items: BankTransaction[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
