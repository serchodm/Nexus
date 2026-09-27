import React, { useState } from 'react';
import { X, Tag, Repeat, Users, BookmarkPlus } from 'lucide-react';
import type { TransactionDetail, CategorizeTransactionRequest } from '../../types/transaction';
import type { CategoryTree } from '../../types/category';

interface CategorizeModalProps {
  transaction: TransactionDetail;
  categories: CategoryTree[];
  onClose: () => void;
  onSave: (req: CategorizeTransactionRequest) => Promise<void>;
  isSaving: boolean;
}

export const CategorizeModal: React.FC<CategorizeModalProps> = ({
  transaction,
  categories,
  onClose,
  onSave,
  isSaving,
}) => {
  const [selectedCompositeId, setSelectedCompositeId] = useState<string>(() => {
    if (transaction.categoryId) {
      return transaction.subcategoryId
        ? `${transaction.categoryId}:${transaction.subcategoryId}`
        : transaction.categoryId;
    }
    const first = categories[0];
    return first?.id ?? '';
  });

  const [isRecurring, setIsRecurring] = useState<boolean>(transaction.isRecurring ?? false);
  const [expenseNature, setExpenseNature] = useState<number>(transaction.expenseNature ?? 1);
  const [createRule, setCreateRule] = useState<boolean>(Boolean(transaction.cleanMerchantName));
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedCompositeId) {
      setError('Debes seleccionar una categoría.');
      return;
    }

    const [catId, subcatId] = selectedCompositeId.split(':');

    try {
      await onSave({
        categoryId: catId,
        subcategoryId: subcatId || null,
        isRecurring,
        expenseNature: expenseNature as 1 | 2,
        createRuleForMerchant: createRule,
      });
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error al guardar la categorización.');
    }
  };

  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.75)',
        backdropFilter: 'blur(8px)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: '1.5rem',
        zIndex: 100,
      }}
      onClick={onClose}
    >
      <div
        className="glass-card"
        style={{
          width: '100%',
          maxWidth: '520px',
          boxShadow: 'var(--shadow-glass)',
          overflow: 'hidden',
        }}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div
          style={{
            padding: '1.25rem 1.5rem',
            borderBottom: '1px solid var(--border-subtle)',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <div
              style={{
                width: '36px',
                height: '36px',
                borderRadius: 'var(--radius-md)',
                background: 'hsla(217, 91%, 60%, 0.15)',
                color: 'var(--primary-500)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <Tag size={18} />
            </div>
            <div>
              <h3 style={{ fontSize: '1.1rem', fontWeight: 600, color: 'var(--text-main)', margin: 0 }}>
                Categorizar Transacción
              </h3>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-dim)', margin: 0 }}>
                Ajuste manual inmutable (Prevalencia RN-01)
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            style={{
              background: 'transparent',
              border: 'none',
              color: 'var(--text-muted)',
              cursor: 'pointer',
              padding: '0.3rem',
            }}
          >
            <X size={18} />
          </button>
        </div>

        {/* Resumen */}
        <div
          style={{
            padding: '1rem 1.5rem',
            background: 'hsla(0, 0%, 100%, 0.02)',
            borderBottom: '1px solid var(--border-glass)',
          }}
        >
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline' }}>
            <span style={{ fontWeight: 600, color: 'var(--text-main)', fontSize: '0.95rem' }}>
              {transaction.cleanMerchantName || transaction.description}
            </span>
            <span
              style={{
                fontWeight: 700,
                color: transaction.amount < 0 ? 'var(--danger)' : 'var(--success)',
              }}
            >
              {transaction.amount.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €
            </span>
          </div>
          <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)', marginTop: '0.2rem' }}>
            {transaction.description}
          </div>
        </div>

        {/* Formulario */}
        <form onSubmit={handleSubmit} style={{ padding: '1.5rem' }}>
          {error && (
            <div
              style={{
                padding: '0.75rem',
                borderRadius: 'var(--radius-sm)',
                background: 'var(--danger-bg)',
                border: '1px solid var(--danger)',
                color: 'var(--danger)',
                fontSize: '0.82rem',
                marginBottom: '1rem',
              }}
            >
              {error}
            </div>
          )}

          {/* Categoría / Subcategoría */}
          <div style={{ marginBottom: '1.25rem' }}>
            <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.4rem' }}>
              Categoría y Subcategoría
            </label>
            <select
              value={selectedCompositeId}
              onChange={(e) => setSelectedCompositeId(e.target.value)}
              style={{
                width: '100%',
                padding: '0.65rem 0.85rem',
                background: 'var(--bg-app)',
                border: '1px solid var(--border-subtle)',
                borderRadius: 'var(--radius-sm)',
                color: 'var(--text-main)',
                fontSize: '0.88rem',
              }}
            >
              {categories.map((cat) => (
                <optgroup key={cat.id} label={cat.name}>
                  <option value={cat.id}>{cat.name} (General)</option>
                  {cat.subcategories.map((sub) => (
                    <option key={sub.id} value={`${cat.id}:${sub.id}`}>
                      {cat.name} • {sub.name}
                    </option>
                  ))}
                </optgroup>
              ))}
            </select>
          </div>

          {/* Toggles de Recurrencia y Naturaleza */}
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', marginBottom: '1.25rem' }}>
            <div>
              <label style={{ display: 'flex', alignItems: 'center', gap: '0.4rem', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.4rem' }}>
                <Users size={14} />
                Naturaleza
              </label>
              <select
                value={expenseNature}
                onChange={(e) => setExpenseNature(parseInt(e.target.value, 10))}
                style={{
                  width: '100%',
                  padding: '0.6rem 0.75rem',
                  background: 'var(--bg-app)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                  color: 'var(--text-main)',
                  fontSize: '0.82rem',
                }}
              >
                <option value={1}>Individual (100% mío)</option>
                <option value={2}>Compartido (Cotitular)</option>
              </select>
            </div>

            <div>
              <label style={{ display: 'flex', alignItems: 'center', gap: '0.4rem', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.4rem' }}>
                <Repeat size={14} />
                Frecuencia
              </label>
              <button
                type="button"
                onClick={() => setIsRecurring(!isRecurring)}
                style={{
                  width: '100%',
                  padding: '0.6rem 0.75rem',
                  background: isRecurring ? 'hsla(217, 91%, 60%, 0.15)' : 'var(--bg-app)',
                  border: `1px solid ${isRecurring ? 'var(--primary-500)' : 'var(--border-subtle)'}`,
                  borderRadius: 'var(--radius-sm)',
                  color: isRecurring ? 'var(--primary-500)' : 'var(--text-dim)',
                  fontWeight: 600,
                  fontSize: '0.82rem',
                  cursor: 'pointer',
                  textAlign: 'center',
                }}
              >
                {isRecurring ? 'Recurrente / Suscripción' : 'Gasto Puntual'}
              </button>
            </div>
          </div>

          {/* Crear regla automática */}
          <div
            style={{
              padding: '0.9rem',
              borderRadius: 'var(--radius-md)',
              background: 'hsla(0, 0%, 100%, 0.03)',
              border: '1px solid var(--border-glass)',
              marginBottom: '1.5rem',
            }}
          >
            <label style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', cursor: 'pointer' }}>
              <input
                type="checkbox"
                checked={createRule}
                onChange={(e) => setCreateRule(e.target.checked)}
                style={{ width: '16px', height: '16px', cursor: 'pointer' }}
              />
              <span style={{ fontSize: '0.82rem', color: 'var(--text-main)', fontWeight: 500, display: 'flex', alignItems: 'center', gap: '0.35rem' }}>
                <BookmarkPlus size={15} color="var(--primary-500)" />
                Crear regla para comercios futuros
              </span>
            </label>
            <p style={{ fontSize: '0.74rem', color: 'var(--text-dim)', margin: '0.35rem 0 0 1.7rem' }}>
              Se asignará automáticamente esta categoría a futuros movimientos de «{transaction.cleanMerchantName || transaction.description}».
            </p>
          </div>

          {/* Footer */}
          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.75rem' }}>
            <button type="button" onClick={onClose} className="btn-secondary">
              Cancelar
            </button>
            <button type="submit" disabled={isSaving} className="btn-primary">
              {isSaving ? 'Guardando...' : 'Aplicar Categoría'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
