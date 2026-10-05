import React, { useState, useMemo } from 'react';
import { X, Link2, Search, ArrowDownRight, ArrowUpRight, RotateCcw, ArrowLeftRight, Check, AlertCircle } from 'lucide-react';
import type { TransactionDetail, TransactionNature } from '../../types/transaction';

interface LinkTransactionModalProps {
  transaction: TransactionDetail;
  allTransactions: TransactionDetail[];
  onClose: () => void;
  onLink: (targetTransactionId: string) => Promise<void>;
  onChangeNature?: (nature: TransactionNature) => Promise<void>;
  isLinking: boolean;
}

export const LinkTransactionModal: React.FC<LinkTransactionModalProps> = ({
  transaction,
  allTransactions,
  onClose,
  onLink,
  onChangeNature,
  isLinking,
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedTargetId, setSelectedTargetId] = useState<string | null>(null);
  const [currentNature, setCurrentNature] = useState<TransactionNature>(
    transaction.transactionNature ?? (transaction.amount > 0 ? 1 : 0)
  );
  const [isUpdatingNature, setIsUpdatingNature] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Filtrar transacciones candidatas (excluyendo la actual)
  const candidateTransactions = useMemo(() => {
    return allTransactions
      .filter((t) => t.id !== transaction.id)
      .filter((t) => {
        if (!searchTerm) return true;
        const term = searchTerm.toLowerCase();
        return (
          t.description.toLowerCase().includes(term) ||
          (t.cleanMerchantName && t.cleanMerchantName.toLowerCase().includes(term)) ||
          t.amount.toString().includes(term) ||
          t.operationDate.includes(term)
        );
      })
      .sort((a, b) => {
        // Priorizar candidatos con importe compatible
        const aCompat = Math.abs(a.amount) >= Math.abs(transaction.amount) && Math.sign(a.amount) !== Math.sign(transaction.amount);
        const bCompat = Math.abs(b.amount) >= Math.abs(transaction.amount) && Math.sign(b.amount) !== Math.sign(transaction.amount);
        if (aCompat && !bCompat) return -1;
        if (!aCompat && bCompat) return 1;
        return new Date(b.operationDate).getTime() - new Date(a.operationDate).getTime();
      });
  }, [allTransactions, transaction.id, transaction.amount, searchTerm]);

  const handleLink = async () => {
    if (!selectedTargetId) {
      setError('Selecciona una transacción para vincular.');
      return;
    }

    setError(null);
    try {
      await onLink(selectedTargetId);
      setSuccessMessage('Transacción vinculada con éxito como contrapartida.');
      setTimeout(() => {
        onClose();
      }, 900);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error al vincular la transacción.');
    }
  };

  const handleNatureChange = async (newNature: TransactionNature) => {
    setCurrentNature(newNature);
    if (!onChangeNature) return;

    setIsUpdatingNature(true);
    setError(null);
    try {
      await onChangeNature(newNature);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error al actualizar la naturaleza contable.');
    } finally {
      setIsUpdatingNature(false);
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
          maxWidth: '640px',
          maxHeight: '90vh',
          display: 'flex',
          flexDirection: 'column',
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
                background: 'hsla(160, 84%, 39%, 0.15)',
                color: '#34d399',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <Link2 size={18} />
            </div>
            <div>
              <h3 style={{ fontSize: '1.1rem', fontWeight: 600, color: 'var(--text-main)', margin: 0 }}>
                Vincular Contrapartida / Devolución
              </h3>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-dim)', margin: 0 }}>
                Enlaza este movimiento con su cargo original o traspaso entre cuentas
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

        {/* Transacción actual */}
        <div
          style={{
            padding: '1rem 1.5rem',
            background: 'hsla(0, 0%, 100%, 0.02)',
            borderBottom: '1px solid var(--border-glass)',
          }}
        >
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <div>
              <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)', fontFamily: 'monospace' }}>
                {transaction.operationDate}
              </span>
              <div style={{ fontWeight: 600, color: 'var(--text-main)', fontSize: '0.95rem' }}>
                {transaction.cleanMerchantName || transaction.description}
              </div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>
                {transaction.description}
              </div>
            </div>
            <div style={{ textAlign: 'right' }}>
              <span
                style={{
                  fontWeight: 700,
                  fontSize: '1.05rem',
                  color: transaction.amount < 0 ? 'var(--danger)' : 'var(--success)',
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: '0.2rem',
                }}
              >
                {transaction.amount < 0 ? <ArrowDownRight size={14} /> : <ArrowUpRight size={14} />}
                {transaction.amount > 0 ? `+${transaction.amount.toFixed(2)} €` : `${transaction.amount.toFixed(2)} €`}
              </span>
            </div>
          </div>

          {/* Selector de Naturaleza Contable */}
          <div style={{ marginTop: '0.85rem', paddingTop: '0.75rem', borderTop: '1px solid var(--border-subtle)' }}>
            <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-dim)', marginBottom: '0.4rem', fontWeight: 600 }}>
              Naturaleza Contable del Movimiento:
            </label>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(130px, 1fr))', gap: '0.4rem' }}>
              {[
                { id: 1 as TransactionNature, label: 'Reembolso / Contrapartida', icon: RotateCcw, desc: 'Compensa gasto original' },
                { id: 2 as TransactionNature, label: 'Ingreso Real', icon: ArrowUpRight, desc: 'Nómina o abono puro' },
                { id: 3 as TransactionNature, label: 'Traspaso Interno', icon: ArrowLeftRight, desc: 'Neutro patrimonial' },
                { id: 0 as TransactionNature, label: 'Gasto Estándar', icon: ArrowDownRight, desc: 'Cargo ordinario' },
              ].map((opt) => {
                const isSelected = currentNature === opt.id;
                const IconComponent = opt.icon;
                return (
                  <button
                    key={opt.id}
                    type="button"
                    disabled={isUpdatingNature}
                    onClick={() => handleNatureChange(opt.id)}
                    style={{
                      padding: '0.4rem 0.5rem',
                      borderRadius: 'var(--radius-sm)',
                      background: isSelected ? 'hsla(217, 91%, 60%, 0.15)' : 'var(--bg-app)',
                      border: `1px solid ${isSelected ? 'var(--primary-500)' : 'var(--border-subtle)'}`,
                      color: isSelected ? 'var(--primary-500)' : 'var(--text-muted)',
                      cursor: 'pointer',
                      textAlign: 'left',
                      fontSize: '0.74rem',
                      display: 'flex',
                      flexDirection: 'column',
                      gap: '0.15rem',
                    }}
                  >
                    <span style={{ display: 'inline-flex', alignItems: 'center', gap: '0.3rem', fontWeight: isSelected ? 700 : 500 }}>
                      <IconComponent size={12} />
                      {opt.label}
                    </span>
                    <span style={{ fontSize: '0.65rem', color: 'var(--text-dim)' }}>
                      {opt.desc}
                    </span>
                  </button>
                );
              })}
            </div>
          </div>
        </div>

        {/* Buscador de candidatos */}
        <div style={{ padding: '0.85rem 1.5rem', borderBottom: '1px solid var(--border-subtle)' }}>
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
              padding: '0.45rem 0.75rem',
              background: 'var(--bg-app)',
              border: '1px solid var(--border-subtle)',
              borderRadius: 'var(--radius-md)',
            }}
          >
            <Search size={14} color="var(--text-dim)" />
            <input
              type="text"
              placeholder="Buscar cargo original por comercio, concepto o importe..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              style={{
                background: 'transparent',
                border: 'none',
                outline: 'none',
                color: 'var(--text-main)',
                fontSize: '0.85rem',
                width: '100%',
              }}
            />
          </div>
        </div>

        {/* Mensajes de feedback */}
        {error && (
          <div
            style={{
              margin: '0.75rem 1.5rem 0',
              padding: '0.6rem 0.85rem',
              borderRadius: 'var(--radius-sm)',
              background: 'var(--danger-bg)',
              border: '1px solid var(--danger)',
              color: 'var(--danger)',
              fontSize: '0.8rem',
              display: 'flex',
              alignItems: 'center',
              gap: '0.4rem',
            }}
          >
            <AlertCircle size={14} />
            {error}
          </div>
        )}

        {successMessage && (
          <div
            style={{
              margin: '0.75rem 1.5rem 0',
              padding: '0.6rem 0.85rem',
              borderRadius: 'var(--radius-sm)',
              background: 'var(--success-bg)',
              border: '1px solid var(--success)',
              color: 'var(--success)',
              fontSize: '0.8rem',
              display: 'flex',
              alignItems: 'center',
              gap: '0.4rem',
            }}
          >
            <Check size={14} />
            {successMessage}
          </div>
        )}

        {/* Lista de candidatos */}
        <div
          style={{
            flex: 1,
            overflowY: 'auto',
            padding: '1rem 1.5rem',
            display: 'flex',
            flexDirection: 'column',
            gap: '0.5rem',
          }}
        >
          <span style={{ fontSize: '0.72rem', textTransform: 'uppercase', letterSpacing: '0.05em', color: 'var(--text-dim)', fontWeight: 700 }}>
            Movimientos Candidatos ({candidateTransactions.length}):
          </span>

          {candidateTransactions.length === 0 ? (
            <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', textAlign: 'center', padding: '1.5rem' }}>
              No se encontraron transacciones para vincular.
            </p>
          ) : (
            candidateTransactions.slice(0, 30).map((cand) => {
              const isSelected = selectedTargetId === cand.id;
              const isOppositeSign = Math.sign(cand.amount) !== Math.sign(transaction.amount);
              const isAmountSufficient = Math.abs(cand.amount) >= Math.abs(transaction.amount);

              return (
                <div
                  key={cand.id}
                  onClick={() => setSelectedTargetId(cand.id)}
                  style={{
                    padding: '0.75rem 1rem',
                    borderRadius: 'var(--radius-md)',
                    background: isSelected
                      ? 'hsla(217, 91%, 60%, 0.15)'
                      : 'hsla(222, 47%, 16%, 0.4)',
                    border: `1px solid ${isSelected ? 'var(--primary-500)' : 'var(--border-subtle)'}`,
                    cursor: 'pointer',
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    transition: 'all var(--transition-fast)',
                  }}
                >
                  <div style={{ maxWidth: '70%' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.2rem' }}>
                      <span style={{ fontSize: '0.72rem', color: 'var(--text-dim)', fontFamily: 'monospace' }}>
                        {cand.operationDate}
                      </span>
                      {cand.categoryName && (
                        <span
                          style={{
                            fontSize: '0.68rem',
                            padding: '0.1rem 0.35rem',
                            borderRadius: 'var(--radius-sm)',
                            background: 'hsla(217, 33%, 25%, 0.4)',
                            color: 'var(--text-muted)',
                          }}
                        >
                          {cand.categoryName}
                        </span>
                      )}
                      {isOppositeSign && isAmountSufficient && (
                        <span
                          style={{
                            fontSize: '0.65rem',
                            padding: '0.08rem 0.3rem',
                            borderRadius: 'var(--radius-sm)',
                            background: 'hsla(160, 84%, 39%, 0.15)',
                            color: '#34d399',
                            fontWeight: 600,
                          }}
                        >
                          Compatible
                        </span>
                      )}
                    </div>
                    <span style={{ display: 'block', fontWeight: 600, fontSize: '0.85rem', color: 'var(--text-main)', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                      {cand.cleanMerchantName || cand.description}
                    </span>
                    <span style={{ display: 'block', fontSize: '0.72rem', color: 'var(--text-dim)', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                      {cand.description}
                    </span>
                  </div>

                  <div style={{ textAlign: 'right' }}>
                    <span
                      style={{
                        fontWeight: 700,
                        fontSize: '0.9rem',
                        color: cand.amount < 0 ? 'var(--danger)' : 'var(--success)',
                        display: 'block',
                      }}
                    >
                      {cand.amount < 0 ? `${cand.amount.toFixed(2)} €` : `+${cand.amount.toFixed(2)} €`}
                    </span>
                    {isSelected && (
                      <span
                        style={{
                          fontSize: '0.7rem',
                          color: 'var(--primary-500)',
                          fontWeight: 600,
                          display: 'inline-flex',
                          alignItems: 'center',
                          gap: '0.2rem',
                          marginTop: '0.2rem',
                        }}
                      >
                        <Check size={12} /> Seleccionado
                      </span>
                    )}
                  </div>
                </div>
              );
            })
          )}
        </div>

        {/* Footer */}
        <div
          style={{
            padding: '1rem 1.5rem',
            borderTop: '1px solid var(--border-subtle)',
            display: 'flex',
            justifyContent: 'flex-end',
            gap: '0.75rem',
          }}
        >
          <button
            type="button"
            onClick={onClose}
            style={{
              padding: '0.55rem 1rem',
              borderRadius: 'var(--radius-sm)',
              background: 'transparent',
              border: '1px solid var(--border-subtle)',
              color: 'var(--text-muted)',
              fontSize: '0.85rem',
              cursor: 'pointer',
            }}
          >
            Cancelar
          </button>
          <button
            type="button"
            disabled={!selectedTargetId || isLinking}
            onClick={handleLink}
            style={{
              padding: '0.55rem 1.25rem',
              borderRadius: 'var(--radius-sm)',
              background: !selectedTargetId || isLinking ? 'var(--border-subtle)' : 'var(--primary-500)',
              border: 'none',
              color: 'white',
              fontSize: '0.85rem',
              fontWeight: 600,
              cursor: !selectedTargetId || isLinking ? 'not-allowed' : 'pointer',
              display: 'inline-flex',
              alignItems: 'center',
              gap: '0.4rem',
            }}
          >
            <Link2 size={14} />
            {isLinking ? 'Vinculando...' : 'Confirmar Vinculación'}
          </button>
        </div>
      </div>
    </div>
  );
};
