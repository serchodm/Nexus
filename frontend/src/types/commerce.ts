export const CommerceProvider = {
  Amazon: 1,
  PayPal: 2,
} as const;
export type CommerceProvider = (typeof CommerceProvider)[keyof typeof CommerceProvider];

export type MatchConfidence = 'Exact100' | 'High' | 'Medium' | 'Low';

export interface ReconciledOrderItemDto {
  id: string;
  sku?: string;
  title: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface ReconciliationCandidate {
  id: string;
  bankTransactionId: string;
  transactionDate: string;
  transactionDescription: string;
  transactionAmount: number;
  externalCommerceOrderId: string;
  provider: CommerceProvider | string;
  externalOrderId: string;
  orderDate: string;
  orderAmount: number;
  counterpartyName?: string;
  confidence: MatchConfidence | string;
  matchReason: string;
  matchedItems: ReconciledOrderItemDto[];
}

export interface ImportCommerceReportResult {
  totalOrdersImported: number;
  newOrdersAdded: number;
  skippedDuplicates: number;
  totalImportedAmount: number;
}

export interface AcceptReconciliationResult {
  success: boolean;
  transactionId: string;
  createdSplitsCount: number;
  merchantName?: string;
  message?: string;
}

export interface CommerceOrderItem {
  id: string;
  sku?: string;
  title: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  bankTransactionId?: string;
}

export interface CommerceOrder {
  id: string;
  provider: CommerceProvider | string;
  externalOrderId: string;
  orderDate: string;
  totalAmount: number;
  currency: string;
  counterpartyName?: string;
  isFullyReconciled: boolean;
  items: CommerceOrderItem[];
}
