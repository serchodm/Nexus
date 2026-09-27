import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../lib/api-client';
import type { Category, CategoryTree, CreateCategoryRequest } from '../types/category';

export function useCategories() {
  const queryClient = useQueryClient();

  const categoriesQuery = useQuery({
    queryKey: ['categories', 'tree'],
    queryFn: async () => {
      return apiClient<CategoryTree[]>('/api/categories');
    },
    staleTime: 1000 * 60 * 10, // 10 minutos de caché
  });

  const createCategoryMutation = useMutation({
    mutationFn: async (req: CreateCategoryRequest) => {
      return apiClient<Category>('/api/categories', {
        method: 'POST',
        body: JSON.stringify(req),
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
    },
  });

  return {
    categoriesTree: categoriesQuery.data ?? [],
    isLoading: categoriesQuery.isLoading,
    isError: categoriesQuery.isError,
    refetch: categoriesQuery.refetch,
    createCategory: createCategoryMutation.mutateAsync,
    isCreating: createCategoryMutation.isPending,
  };
}
