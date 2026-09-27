import React, { useState } from 'react';
import { X, Plus, Trash2, CheckCircle2, AlertTriangle, Layers, Wand2 } from 'lucide-react';
import type { TransactionDetail, SplitItemInput } from '../../types/transaction';
import type { CategoryTree } from '../../types/category';

interface SplitTransactionModalProps {
  transaction: TransactionDetail;
  categories: CategoryTree[];
  onClose: () => void;
  onSave: (items: SplitItemInput[]) => Promise<void>;
  onResetSplits?: () => Promise<void>;
  isSaving: boolean;
  isResetting?: boolean;
}

export const SplitTransactionModal: React.FC<SplitTransactionModalProps> = ({
  transaction,
  categories,
  onClose,
  onSave,
  onResetSplits,
  isSaving,
  isResetting = false,
}) => {
  // Inicializar partidas: si ya tiene splits, precargarlos; si no, crear 2 iniciales
  const [items, setItems] = useState<SplitItemInput[]>(() => {
    if (transaction.hasSplits && transaction.splitItems.length > 0) {
      return transaction.splitItems.map((s) => ({
        categoryId: s.categoryId,
        subcategoryId: s.subcategoryId,
        amount: s.amount,
        description: s.description ?? '',
        expenseNature: s.expenseNature,
      }));
    }

    const firstCat = categories[0];
    const half = Math.round((transaction.amount / 2) * 100) / 100;
    const remainder = Math.round((transaction.amount - half) * 100) / 100;

    return [
      {
        categoryId: firstCat?.id ?? '',
        subcategoryId: firstCat?.subcategories[0]?.id ?? null,
        amount: half,
        description: '',
        expenseNature: 1,
      },
      {
        categoryId: firstCat?.id ?? '',
        subcategoryId: firstCat?.subcategories[0]?.id ?? null,
        amount: remainder,
        description: '',
        expenseNature: 1,
      },
    ];
  });

  const [formError, setFormError] = useState<string | null>(null);

  // Cálculos contables en tiempo real
  const totalAssigned = Math.round(items.reduce((sum, item) => sum + (Number(item.amount) || 0), 0) * 100) / 100;
  const targetAmount = Math.round(transaction.amount * 100) / 100;
  const discrepancy = Math.round((targetAmount - totalAssigned) * 100) / 100;
  const isBalanced = Math.abs(discrepancy) < 0.001;

  const handleAddItem = () => {
    const firstCat = categories[0];
    setItems((prev) => [
      ...prev,
      {
        categoryId: firstCat?.id ?? '',
        subcategoryId: firstCat?.subcategories[0]?.id ?? null,
        amount: discrepancy !== 0 ? discrepancy : 0,
        description: '',
        expenseNature: 1,
      },
    ]);
  };

  const handleRemoveItem = (index: number) => {
    if (items.length <= 2) {
      setFormError('Un desglose debe tener al menos 2 partidas.');
      return;
    }
    setFormError(null);
    setItems((prev) => prev.filter((_, i) => i !== index));
  };

  const handleItemChange = (index: number, field: keyof SplitItemInput, value: unknown) => {
    setFormError(null);
    setItems((prev) => {
      const copy = [...prev];
      copy[index] = { ...copy[index], [field]: value };
      return copy;
    });
  };

  const handleCategorySelect = (index: number, compositeId: string) => {
    // compositeId puede ser "catId" o "catId:subcatId"
    const [catId, subcatId] = compositeId.split(':');
    setItems((prev) => {
      const copy = [...prev];
      copy[index] = {
        ...copy[index],
        categoryId: catId,
        subcategoryId: subcatId || null,
      };
      return copy;
    });
  };

  const handleAutoBalance = () => {
    if (items.length === 0) return;
    setItems((prev) => {
      const copy = [...prev];
      const lastIndex = copy.length - 1;
      const currentLast = Number(copy[lastIndex].amount) || 0;
      copy[lastIndex] = {
        ...copy[lastIndex],
        amount: Math.round((currentLast + discrepancy) * 100) / 100,
      };
      return copy;
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!isBalanced) {
      setFormError(`El desglose contable no cuadra. Hay una diferencia de ${discrepancy.toFixed(2)} €.`);
      return;
    }

    for (const item of items) {
      if (!item.categoryId) {
        setFormError('Todas las partidas deben tener una categoría asignada.');
        return;
      }
      if (item.amount === 0) {
        setFormError('El importe de cada partida debe ser distinto de 0.');
        return;
      }
    }

    try {
      await onSave(items);
      onClose();
    } catch (err: unknown) {
      setFormError(err instanceof Error ? err.message : 'Error al guardar el desglose.');
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
          maxWidth: '740px',
          maxHeight: '90vh',
          display: 'flex',
          flexDirection: 'column',
          boxShadow: 'var(--shadow-glass)',
          overflow: 'hidden',
        }}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header Modal */}
        <div
          style={{
            padding: '1.25rem 1.75rem',
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
              <Layers size={18} />
            </div>
            <div>
              <h3 style={{ fontSize: '1.15rem', fontWeight: 600, color: 'var(--text-main)', margin: 0 }}>
                Desglose de Gasto Mixto (Split)
              </h3>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-dim)', margin: 0 }}>
                Invariante Contable RN-02: la suma de partidas debe coincidir con el total del movimiento.
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
              padding: '0.4rem',
              borderRadius: 'var(--radius-sm)',
            }}
          >
            <X size={20} />
          </button>
        </div>

        {/* Resumen del Movimiento Padre */}
        <div
          style={{
            padding: '1rem 1.75rem',
            background: 'hsla(0, 0%, 100%, 0.02)',
            borderBottom: '1px solid var(--border-glass)',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            fontSize: '0.85rem',
          }}
        >
          <div>
            <span style={{ color: 'var(--text-dim)', display: 'block', fontSize: '0.75rem' }}>
              {transaction.operationDate} • {transaction.cleanMerchantName || 'Comercio'}
            </span>
            <span style={{ color: 'var(--text-main)', fontWeight: 500 }}>
              {transaction.description}
            </span>
          </div>
          <div style={{ textAlign: 'right' }}>
            <span style={{ color: 'var(--text-dim)', display: 'block', fontSize: '0.75rem' }}>
              Importe Total
            </span>
            <span
              style={{
                fontSize: '1.25rem',
                fontWeight: 700,
                color: transaction.amount < 0 ? 'var(--danger)' : 'var(--success)',
              }}
            >
              {transaction.amount.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €
            </span>
          </div>
        </div>

        {/* Formulario y Lista de Partidas */}
        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', flex: 1, overflow: 'hidden' }}>
          <div style={{ padding: '1.5rem 1.75rem', overflowY: 'auto', flex: 1 }}>
            {formError && (
              <div
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.5rem',
                  padding: '0.75rem 1rem',
                  borderRadius: 'var(--radius-md)',
                  background: 'var(--danger-bg)',
                  border: '1px solid var(--danger)',
                  color: 'var(--danger)',
                  fontSize: '0.85rem',
                  marginBottom: '1.25rem',
                }}
              >
                <AlertTriangle size={16} />
                <span>{formError}</span>
              </div>
            )}

            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              {items.map((item, idx) => {
                const currentVal = item.subcategoryId
                  ? `${item.categoryId}:${item.subcategoryId}`
                  : item.categoryId;

                return (
                  <div
                    key={idx}
                    style={{
                      background: 'hsla(222, 47%, 16%, 0.5)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-lg)',
                      padding: '1rem',
                      display: 'grid',
                      gridTemplateColumns: '1.6fr 1fr 1.2fr 0.9fr auto',
                      gap: '0.75rem',
                      alignItems: 'center',
                    }}
                  >
                    {/* Selector de Categoría / Subcategoría */}
                    <div>
                      <label style={{ display: 'block', fontSize: '0.7rem', color: 'var(--text-dim)', marginBottom: '0.3rem' }}>
                        Categoría
                      </label>
                      <select
                        value={currentVal}
                        onChange={(e) => handleCategorySelect(idx, e.target.value)}
                        style={{
                          width: '100%',
                          padding: '0.5rem',
                          background: 'var(--bg-app)',
                          border: '1px solid var(--border-subtle)',
                          borderRadius: 'var(--radius-sm)',
                          color: 'var(--text-main)',
                          fontSize: '0.82rem',
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

                    {/* Importe */}
                    <div>
                      <label style={{ display: 'block', fontSize: '0.7rem', color: 'var(--text-dim)', marginBottom: '0.3rem' }}>
                        Importe (€)
                      </label>
                      <input
                        type="number"
                        step="0.01"
                        value={item.amount}
                        onChange={(e) => handleItemChange(idx, 'amount', parseFloat(e.target.value) || 0)}
                        style={{
                          width: '100%',
                          padding: '0.5rem',
                          background: 'var(--bg-app)',
                          border: '1px solid var(--border-subtle)',
                          borderRadius: 'var(--radius-sm)',
                          color: 'var(--text-main)',
                          fontSize: '0.85rem',
                          fontFamily: 'monospace',
                          fontWeight: 600,
                        }}
                      />
                    </div>

                    {/* Descripción de la partida */}
                    <div>
                      <label style={{ display: 'block', fontSize: '0.7rem', color: 'var(--text-dim)', marginBottom: '0.3rem' }}>
                        Nota / Concepto
                      </label>
                      <input
                        type="text"
                        placeholder="Ej. Ropa, Pienso..."
                        value={item.description ?? ''}
                        onChange={(e) => handleItemChange(idx, 'description', e.target.value)}
                        style={{
                          width: '100%',
                          padding: '0.5rem',
                          background: 'var(--bg-app)',
                          border: '1px solid var(--border-subtle)',
                          borderRadius: 'var(--radius-sm)',
                          color: 'var(--text-main)',
                          fontSize: '0.82rem',
                        }}
                      />
                    </div>

                    {/* Naturaleza (Individual vs Compartido) */}
                    <div>
                      <label style={{ display: 'block', fontSize: '0.7rem', color: 'var(--text-dim)', marginBottom: '0.3rem' }}>
                        Naturaleza
                      </label>
                      <select
                        value={item.expenseNature}
                        onChange={(e) => handleItemChange(idx, 'expenseNature', parseInt(e.target.value, 10))}
                        style={{
                          width: '100%',
                          padding: '0.5rem',
                          background: 'var(--bg-app)',
                          border: '1px solid var(--border-subtle)',
                          borderRadius: 'var(--radius-sm)',
                          color: 'var(--text-main)',
                          fontSize: '0.8rem',
                        }}
                      >
                        <option value={1}>Individual</option>
                        <option value={2}>Compartido</option>
                      </select>
                    </div>

                    {/* Eliminar partida */}
                    <div style={{ paddingTop: '1.2rem' }}>
                      <button
                        type="button"
                        onClick={() => handleRemoveItem(idx)}
                        disabled={items.length <= 2}
                        title={items.length <= 2 ? 'Mínimo 2 partidas requeridas' : 'Eliminar partida'}
                        style={{
                          background: 'transparent',
                          border: 'none',
                          color: items.length <= 2 ? 'var(--text-dim)' : 'var(--danger)',
                          cursor: items.length <= 2 ? 'not-allowed' : 'pointer',
                          padding: '0.4rem',
                          opacity: items.length <= 2 ? 0.3 : 1,
                        }}
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>

            {/* Botón Añadir Partida */}
            <div style={{ marginTop: '1rem', display: 'flex', gap: '0.75rem' }}>
              <button
                type="button"
                onClick={handleAddItem}
                className="btn-secondary"
                style={{ fontSize: '0.82rem', padding: '0.45rem 0.9rem' }}
              >
                <Plus size={15} />
                Añadir Partida
              </button>

              {!isBalanced && (
                <button
                  type="button"
                  onClick={handleAutoBalance}
                  className="btn-secondary"
                  style={{
                    fontSize: '0.82rem',
                    padding: '0.45rem 0.9rem',
                    borderColor: 'var(--primary-500)',
                    color: 'var(--primary-500)',
                  }}
                >
                  <Wand2 size={15} />
                  Ajustar Descuadre en Última Partida ({discrepancy > 0 ? `+${discrepancy.toFixed(2)}` : discrepancy.toFixed(2)} €)
                </button>
              )}
            </div>
          </div>

          {/* Footer con Invariante y Acciones */}
          <div
            style={{
              padding: '1.25rem 1.75rem',
              borderTop: '1px solid var(--border-subtle)',
              background: 'hsla(0, 0%, 100%, 0.015)',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
            }}
          >
            {/* Indicador del Invariante */}
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
              {isBalanced ? (
                <div
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '0.4rem',
                    color: 'var(--success)',
                    fontSize: '0.85rem',
                    fontWeight: 600,
                  }}
                >
                  <CheckCircle2 size={18} />
                  <span>Desglose Cuadrado (Suma: {totalAssigned.toFixed(2)} €)</span>
                </div>
              ) : (
                <div
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '0.4rem',
                    color: 'var(--danger)',
                    fontSize: '0.85rem',
                    fontWeight: 600,
                  }}
                >
                  <AlertTriangle size={18} />
                  <span>
                    Descuadre pendiente: {discrepancy.toFixed(2)} € (Asignado: {totalAssigned.toFixed(2)} € / Total: {targetAmount.toFixed(2)} €)
                  </span>
                </div>
              )}
            </div>

            {/* Botones de acción */}
            <div style={{ display: 'flex', gap: '0.75rem' }}>
              {transaction.hasSplits && onResetSplits && (
                <button
                  type="button"
                  onClick={async () => {
                    if (window.confirm('¿Deseas eliminar el desglose y volver a tratar esta transacción como gasto único?')) {
                      await onResetSplits();
                      onClose();
                    }
                  }}
                  disabled={isResetting}
                  className="btn-secondary"
                  style={{ color: 'var(--danger)', borderColor: 'var(--danger-bg)' }}
                >
                  {isResetting ? 'Eliminando...' : 'Deshacer Desglose'}
                </button>
              )}

              <button type="button" onClick={onClose} className="btn-secondary">
                Cancelar
              </button>

              <button
                type="submit"
                disabled={!isBalanced || isSaving}
                className="btn-primary"
                style={{ opacity: !isBalanced ? 0.5 : 1 }}
              >
                {isSaving ? 'Guardando...' : 'Confirmar Desglose'}
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>
  );
};
