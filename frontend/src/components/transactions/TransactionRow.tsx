import React, { useState } from 'react';
import { 
  ArrowDownRight, 
  ArrowUpRight, 
  Repeat, 
  Users, 
  Layers, 
  ChevronDown, 
  ChevronUp, 
  Edit3,
  Sparkles
} from 'lucide-react';
import { CategoryBadge } from '../categories/CategoryBadge';
import type { TransactionDetail } from '../../types/transaction';
import type { ReconciliationCandidate } from '../../types/commerce';

interface TransactionRowProps {
  transaction: TransactionDetail;
  onOpenCategorize: (tx: TransactionDetail) => void;
  onOpenSplit: (tx: TransactionDetail) => void;
  reconciliationCandidate?: ReconciliationCandidate;
  onReviewReconciliation?: (candidate: ReconciliationCandidate) => void;
}

export const TransactionRow: React.FC<TransactionRowProps> = ({
  transaction: tx,
  onOpenCategorize,
  onOpenSplit,
  reconciliationCandidate,
  onReviewReconciliation,
}) => {
  const [isExpanded, setIsExpanded] = useState(false);

  const getPlatformBadge = (platform: number) => {
    switch (platform) {
      case 1:
        return (
          <span
            style={{
              fontSize: '0.68rem',
              padding: '0.15rem 0.45rem',
              borderRadius: 'var(--radius-sm)',
              background: 'hsla(210, 100%, 50%, 0.12)',
              border: '1px solid hsla(210, 100%, 50%, 0.3)',
              color: '#38bdf8',
              fontWeight: 700,
            }}
          >
            PayPal
          </span>
        );
      case 2:
        return (
          <span
            style={{
              fontSize: '0.68rem',
              padding: '0.15rem 0.45rem',
              borderRadius: 'var(--radius-sm)',
              background: 'hsla(33, 100%, 50%, 0.12)',
              border: '1px solid hsla(33, 100%, 50%, 0.3)',
              color: '#fbbf24',
              fontWeight: 700,
            }}
          >
            Amazon
          </span>
        );
      default:
        return null;
    }
  };

  return (
    <>
      <tr
        style={{
          borderBottom: isExpanded ? 'none' : '1px solid var(--border-glass)',
          background: isExpanded ? 'hsla(222, 47%, 16%, 0.3)' : 'transparent',
          transition: 'background var(--transition-fast)',
        }}
      >
        {/* Fecha */}
        <td style={{ padding: '0.85rem 1rem', color: 'var(--text-muted)', fontFamily: 'monospace', whiteSpace: 'nowrap' }}>
          {tx.operationDate}
        </td>

        {/* Comercio y Concepto */}
        <td style={{ padding: '0.85rem 1rem', maxWidth: '300px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', flexWrap: 'wrap' }}>
            <span style={{ color: 'var(--text-main)', fontWeight: 600, fontSize: '0.92rem' }}>
              {tx.cleanMerchantName || tx.description}
            </span>

            {getPlatformBadge(tx.externalPlatform)}

            {tx.isRecurring && (
              <span
                title="Gasto recurrente o suscripción periódica detectada"
                style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: '0.2rem',
                  fontSize: '0.68rem',
                  padding: '0.12rem 0.4rem',
                  borderRadius: 'var(--radius-sm)',
                  background: 'hsla(263, 70%, 50%, 0.15)',
                  border: '1px solid hsla(263, 70%, 50%, 0.3)',
                  color: '#c084fc',
                  fontWeight: 600,
                }}
              >
                <Repeat size={10} />
                Suscripción
              </span>
            )}

            {tx.expenseNature === 2 && (
              <span
                title="Gasto compartido con cotitular"
                style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: '0.2rem',
                  fontSize: '0.68rem',
                  padding: '0.12rem 0.4rem',
                  borderRadius: 'var(--radius-sm)',
                  background: 'hsla(188, 94%, 53%, 0.12)',
                  border: '1px solid hsla(188, 94%, 53%, 0.3)',
                  color: 'var(--accent-cyan)',
                  fontWeight: 600,
                }}
              >
                <Users size={10} />
                Compartido
              </span>
            )}

            {reconciliationCandidate && (
              <button
                type="button"
                onClick={(e) => {
                  e.stopPropagation();
                  onReviewReconciliation?.(reconciliationCandidate);
                }}
                title={`Reconciliación disponible: ${reconciliationCandidate.matchReason}`}
                style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: '0.25rem',
                  fontSize: '0.68rem',
                  padding: '0.12rem 0.45rem',
                  borderRadius: 'var(--radius-sm)',
                  background: 'linear-gradient(135deg, hsla(38, 92%, 50%, 0.18) 0%, hsla(217, 91%, 60%, 0.18) 100%)',
                  border: '1px solid hsla(38, 92%, 50%, 0.45)',
                  color: '#fbbf24',
                  fontWeight: 700,
                  cursor: 'pointer',
                }}
              >
                <Sparkles size={11} />
                Desglosar pedido
              </button>
            )}
          </div>

          <span
            style={{
              display: 'block',
              fontSize: '0.74rem',
              color: 'var(--text-dim)',
              marginTop: '0.2rem',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
            }}
          >
            {tx.description}
          </span>
        </td>

        {/* Categoría o Desglose */}
        <td style={{ padding: '0.85rem 1rem' }}>
          {tx.hasSplits ? (
            <button
              onClick={() => setIsExpanded(!isExpanded)}
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: '0.4rem',
                background: 'hsla(217, 91%, 60%, 0.12)',
                border: '1px solid var(--border-focus)',
                borderRadius: 'var(--radius-full)',
                color: 'var(--primary-500)',
                padding: '0.25rem 0.65rem',
                fontSize: '0.78rem',
                fontWeight: 600,
                cursor: 'pointer',
              }}
            >
              <Layers size={13} />
              <span>{tx.splitItems.length} partidas</span>
              {isExpanded ? <ChevronUp size={13} /> : <ChevronDown size={13} />}
            </button>
          ) : tx.categoryName ? (
            <div style={{ display: 'inline-flex', alignItems: 'center', gap: '0.4rem' }}>
              <CategoryBadge
                categoryName={tx.categoryName}
                subcategoryName={tx.subcategoryName}
                colorHex={tx.categoryColor}
                icon={tx.categoryIcon}
                size="sm"
              />
              <button
                onClick={() => onOpenCategorize(tx)}
                title="Cambiar categoría"
                style={{
                  background: 'transparent',
                  border: 'none',
                  color: 'var(--text-dim)',
                  cursor: 'pointer',
                  padding: '0.2rem',
                }}
              >
                <Edit3 size={13} />
              </button>
            </div>
          ) : (
            <button
              onClick={() => onOpenCategorize(tx)}
              style={{
                background: 'transparent',
                border: '1px dashed var(--border-subtle)',
                borderRadius: 'var(--radius-full)',
                color: 'var(--text-dim)',
                padding: '0.2rem 0.6rem',
                fontSize: '0.74rem',
                cursor: 'pointer',
              }}
            >
              + Categorizar
            </button>
          )}
        </td>

        {/* Importe */}
        <td
          style={{
            padding: '0.85rem 1rem',
            textAlign: 'right',
            fontWeight: 700,
            fontSize: '0.92rem',
            color: tx.amount < 0 ? 'var(--danger)' : 'var(--success)',
            whiteSpace: 'nowrap',
          }}
        >
          <span style={{ display: 'inline-flex', alignItems: 'center', gap: '0.25rem' }}>
            {tx.amount < 0 ? <ArrowDownRight size={14} /> : <ArrowUpRight size={14} />}
            {tx.amount > 0 ? `+${tx.amount.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €` : `${tx.amount.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €`}
          </span>
        </td>

        {/* Saldo Posterior */}
        <td
          style={{
            padding: '0.85rem 1rem',
            textAlign: 'right',
            color: 'var(--text-muted)',
            fontFamily: 'monospace',
            whiteSpace: 'nowrap',
          }}
        >
          {tx.balanceAfter !== null && tx.balanceAfter !== undefined
            ? `${tx.balanceAfter.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €`
            : '—'}
        </td>

        {/* Acciones */}
        <td style={{ padding: '0.85rem 1rem', textAlign: 'center', whiteSpace: 'nowrap' }}>
          <button
            onClick={() => onOpenSplit(tx)}
            title={tx.hasSplits ? 'Editar o deshacer desglose' : 'Desglosar en varias partidas'}
            style={{
              background: tx.hasSplits ? 'hsla(217, 91%, 60%, 0.1)' : 'transparent',
              border: `1px solid ${tx.hasSplits ? 'var(--primary-500)' : 'var(--border-subtle)'}`,
              borderRadius: 'var(--radius-sm)',
              color: tx.hasSplits ? 'var(--primary-500)' : 'var(--text-muted)',
              padding: '0.3rem 0.6rem',
              cursor: 'pointer',
              display: 'inline-flex',
              alignItems: 'center',
              gap: '0.3rem',
              fontSize: '0.74rem',
              fontWeight: 500,
            }}
          >
            <Layers size={13} />
            {tx.hasSplits ? 'Editar Splits' : 'Desglosar'}
          </button>
        </td>
      </tr>

      {/* Accordion Expandido para Splits */}
      {isExpanded && tx.hasSplits && (
        <tr style={{ background: 'hsla(222, 47%, 14%, 0.4)' }}>
          <td colSpan={6} style={{ padding: '0.75rem 1.5rem 1rem 3.5rem' }}>
            <div
              style={{
                borderLeft: '2px solid var(--primary-500)',
                paddingLeft: '1rem',
                display: 'flex',
                flexDirection: 'column',
                gap: '0.5rem',
              }}
            >
              <span style={{ fontSize: '0.72rem', textTransform: 'uppercase', letterSpacing: '0.05em', color: 'var(--text-dim)', fontWeight: 700 }}>
                Desglose Contable ({tx.splitItems.length} Partidas):
              </span>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))', gap: '0.6rem' }}>
                {tx.splitItems.map((item, idx) => (
                  <div
                    key={idx}
                    style={{
                      background: 'hsla(222, 47%, 18%, 0.7)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-md)',
                      padding: '0.5rem 0.85rem',
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                    }}
                  >
                    <div>
                      <span style={{ display: 'block', fontWeight: 600, fontSize: '0.82rem', color: 'var(--text-main)' }}>
                        {item.categoryName} {item.subcategoryName ? `• ${item.subcategoryName}` : ''}
                      </span>
                      {item.description && (
                        <span style={{ display: 'block', fontSize: '0.72rem', color: 'var(--text-dim)' }}>
                          {item.description}
                        </span>
                      )}
                    </div>
                    <div style={{ textAlign: 'right' }}>
                      <span
                        style={{
                          fontWeight: 700,
                          fontSize: '0.85rem',
                          color: item.amount < 0 ? 'var(--danger)' : 'var(--success)',
                        }}
                      >
                        {item.amount.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €
                      </span>
                      <span style={{ display: 'block', fontSize: '0.68rem', color: 'var(--text-dim)' }}>
                        {item.expenseNature === 2 ? 'Compartido' : 'Individual'}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </td>
        </tr>
      )}
    </>
  );
};
