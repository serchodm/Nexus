export type FlowType = 1 | 2; // 1 = Expense, 2 = Income
export type ExpenseNature = 1 | 2; // 1 = Individual, 2 = Shared
export type ExpenseFlexibility = 1 | 2; // 1 = Fixed, 2 = Discretionary
export type CategorizationSource = 0 | 1 | 2 | 3 | 4 | 5; // 0=None, 1=System, 2=BankNative, 3=Bizum, 4=UserRule, 5=Manual
export type ExternalPlatform = 0 | 1 | 2 | 3 | 4 | 5; // 0=None, 1=PayPal, 2=Amazon, 3=SumUp, 4=Stripe, 5=Bizum

export interface Category {
  id: string;
  parentCategoryId?: string | null;
  name: string;
  icon: string;
  colorHex: string;
  flowType: FlowType;
  defaultExpenseNature: ExpenseNature;
  isSystem: boolean;
  displayOrder: number;
}

export interface CategoryTree extends Category {
  subcategories: Category[];
}

export interface CreateCategoryRequest {
  name: string;
  parentCategoryId?: string | null;
  icon: string;
  colorHex: string;
  flowType: FlowType;
  defaultExpenseNature: ExpenseNature;
  displayOrder?: number;
}
