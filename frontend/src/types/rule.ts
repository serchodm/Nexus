import type { ExpenseNature, ExpenseFlexibility } from './category';

export type RuleMatchType = 1 | 2 | 3; // 1 = Contains, 2 = Exact, 3 = Regex

export interface CategorizationRule {
  id: string;
  name: string;
  pattern: string;
  matchType: RuleMatchType;
  targetCategoryId: string;
  targetCategoryName?: string;
  targetSubcategoryId?: string | null;
  targetSubcategoryName?: string | null;
  priority: number;
  isActive: boolean;
  setIsRecurring?: boolean | null;
  setExpenseNature?: ExpenseNature | null;
  setExpenseFlexibility?: ExpenseFlexibility | null;
}

export interface CreateRuleRequest {
  name: string;
  pattern: string;
  matchType: RuleMatchType;
  targetCategoryId: string;
  targetSubcategoryId?: string | null;
  priority?: number;
  setIsRecurring?: boolean | null;
  setExpenseNature?: ExpenseNature | null;
  setExpenseFlexibility?: ExpenseFlexibility | null;
}

export interface ApplyRetroactiveRequest {
  ruleId?: string | null;
  overrideUserManual?: boolean;
}

export interface ApplyRetroactiveResult {
  totalProcessed: number;
  totalUpdated: number;
  details: string;
}
