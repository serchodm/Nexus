import React, { useState } from 'react';
import { 
  X, 
  Check, 
  Trash2, 
  ShoppingBag, 
  CreditCard, 
  Layers, 
  ArrowRight, 
  ShieldCheck,
  AlertCircle
} from 'lucide-react';
import { useCommerce } from '../../hooks/useCommerce';
import type { ReconciliationCandidate } from '../../types/commerce';

interface ReconciliationReviewModalProps {
  candidate: ReconciliationCandidate | null;
  isOpen: boolean;
  onClose: () => void;
  onReconciliationAccepted?: () => void;
}

export const ReconciliationReviewModal: React.FC<ReconciliationReviewModalProps> = ({
  candidate,
  isOpen,
  onClose,
  onReconciliationAccepted,
}) => {
  const { acceptReconciliation, rejectReconciliation } = useCommerce();
  const [rejectReason, setRejectReason] = useState('');
  const [showRejectInput, setShowRejectInput] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  if (!isOpen || !candidate) return null;

  const isAmazon = candidate.provider === 1 || candidate.provider === 'Amazon';
  const providerName = isAmazon ? 'Amazon' : 'PayPal';
  const providerColor = isAmazon ? '#f59e0b' : '#38bdf8';

  const bankAmount = Math.abs(candidate.transactionAmount);
  const itemsSum = candidate.matchedItems.reduce((acc, item) => acc + item.totalPrice, 0);
  const discrepancy = Math.abs(bankAmount - itemsSum);

  const handleAccept = async () => {
    setErrorMessage(null);
    try {
      await acceptReconciliation.mutateAsync(candidate.id);
      if (onReconciliationAccepted) {
        onReconciliationAccepted();
      }
      onClose();
    } catch (err: any) {
      setErrorMessage(err.message || 'Error al aceptar la reconciliación');
    }
  };

  const handleReject = async () => {
    setErrorMessage(null);
    try {
      await rejectReconciliation.mutateAsync({
        reconciliationId: candidate.id,
        reason: rejectReason || undefined,
      });
      onClose();
    } catch (err: any) {
      setErrorMessage(err.message || 'Error al descartar la propuesta');
    }
  };

  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        zIndex: 50,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'rgba(0, 0, 0, 0.75)',
        backdropFilter: 'blur(8px)',
        padding: '1rem',
      }}
    >
      <div
        style={{
          width: '100%',
          maxWidth: '740px',
          background: 'var(--bg-card, #111827)',
          border: '1px solid var(--border-glass, rgba(255, 255, 255, 0.1))',
          borderRadius: 'var(--radius-xl, 1rem)',
          boxShadow: 'var(--shadow-xl, 0 20px 25px -5px rgba(0, 0, 0, 0.5))',
          overflow: 'hidden',
          display: 'flex',
          flexDirection: 'column',
          maxHeight: '90vh',
        }}
      >
        {/* Cabecera */}
        <div
          style={{
            padding: '1.25rem 1.5rem',
            borderBottom: '1px solid var(--border-glass, rgba(255, 255, 255, 0.08))',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            background: 'hsla(222, 47%, 12%, 0.5)',
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <div
              style={{
                width: '2.5rem',
                height: '2.5rem',
                borderRadius: '0.5rem',
                background: isAmazon
                  ? 'linear-gradient(135deg, #f59e0b 0%, #b45309 100%)'
                  : 'linear-gradient(135deg, #0284c7 0%, #0369a1 100%)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#fff',
              }}
            >
              {isAmazon ? <ShoppingBag size={20} /> : <CreditCard size={20} />}
            </div>
            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <h2 style={{ fontSize: '1.15rem', fontWeight: 700, color: 'var(--text-main, #f3f4f6)', margin: 0 }}>
                  Reconciliación: {providerName}
                </h2>
                <span
                  style={{
                    fontSize: '0.7rem',
                    padding: '0.15rem 0.5rem',
                    borderRadius: '1rem',
                    background: 'hsla(142, 71%, 45%, 0.15)',
                    border: '1px solid hsla(142, 71%, 45%, 0.3)',
                    color: '#4ade80',
                    fontWeight: 700,
                  }}
                >
                  {candidate.confidence === 'Exact100' ? '100% Coincidencia' : candidate.confidence}
                </span>
              </div>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-dim, #9ca3af)', margin: '0.15rem 0 0' }}>
                {candidate.matchReason}
              </p>
            </div>
          </div>

          <button
            onClick={onClose}
            style={{
              background: 'transparent',
              border: 'none',
              color: 'var(--text-muted, #6b7280)',
              cursor: 'pointer',
              padding: '0.4rem',
              borderRadius: '0.375rem',
            }}
          >
            <X size={20} />
          </button>
        </div>

        {/* Comparativa Cara a Cara */}
        <div style={{ padding: '1.5rem', overflowY: 'auto' }}>
          <div
            style={{
              display: 'grid',
              gridTemplateColumns: '1fr auto 1fr',
              gap: '1rem',
              alignItems: 'center',
              marginBottom: '1.5rem',
              background: 'hsla(222, 47%, 16%, 0.3)',
              padding: '1.25rem',
              borderRadius: '0.75rem',
              border: '1px solid var(--border-glass, rgba(255, 255, 255, 0.08))',
            }}
          >
            {/* Cargo Bancario */}
            <div>
              <span style={{ fontSize: '0.72rem', color: 'var(--text-muted, #9ca3af)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                Movimiento Bancario
              </span>
              <div style={{ fontSize: '1.25rem', fontWeight: 700, color: '#f87171', margin: '0.25rem 0' }}>
                {candidate.transactionAmount.toFixed(2)} €
              </div>
              <div style={{ fontSize: '0.85rem', color: 'var(--text-main, #f3f4f6)', fontWeight: 600 }}>
                {candidate.transactionDescription}
              </div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-dim, #9ca3af)', marginTop: '0.2rem' }}>
                Fecha operación: {candidate.transactionDate}
              </div>
            </div>

            {/* Flecha Central */}
            <div style={{ color: 'var(--text-muted, #6b7280)', display: 'flex', alignItems: 'center' }}>
              <ArrowRight size={22} />
            </div>

            {/* Pedido de Comercio */}
            <div>
              <span style={{ fontSize: '0.72rem', color: providerColor, textTransform: 'uppercase', letterSpacing: '0.05em', fontWeight: 600 }}>
                Pedido {providerName}
              </span>
              <div style={{ fontSize: '1.25rem', fontWeight: 700, color: providerColor, margin: '0.25rem 0' }}>
                {candidate.orderAmount.toFixed(2)} €
              </div>
              <div style={{ fontSize: '0.85rem', color: 'var(--text-main, #f3f4f6)', fontWeight: 600 }}>
                {candidate.counterpartyName || `Pedido ${candidate.externalOrderId}`}
              </div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-dim, #9ca3af)', marginTop: '0.2rem' }}>
                ID: {candidate.externalOrderId}
              </div>
            </div>
          </div>

          {/* Lista de Partidas / Artículos Desglosados */}
          <div>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '0.75rem' }}>
              <h3 style={{ fontSize: '0.95rem', fontWeight: 700, color: 'var(--text-main, #f3f4f6)', margin: 0, display: 'flex', alignItems: 'center', gap: '0.4rem' }}>
                <Layers size={16} />
                Artículos a Desglosar ({candidate.matchedItems.length} partidas)
              </h3>
              <span style={{ fontSize: '0.78rem', color: discrepancy === 0 ? '#4ade80' : '#fbbf24', fontWeight: 600 }}>
                {discrepancy === 0 ? '✓ Cuadre exacto (0,00 €)' : `Ajuste automático de envío: ${discrepancy.toFixed(2)} €`}
              </span>
            </div>

            <div
              style={{
                borderRadius: '0.5rem',
                border: '1px solid var(--border-glass, rgba(255, 255, 255, 0.08))',
                overflow: 'hidden',
              }}
            >
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.85rem', textAlign: 'left' }}>
                <thead>
                  <tr style={{ background: 'hsla(222, 47%, 12%, 0.6)', borderBottom: '1px solid var(--border-glass, rgba(255, 255, 255, 0.08))' }}>
                    <th style={{ padding: '0.65rem 1rem', color: 'var(--text-dim, #9ca3af)', fontWeight: 600 }}>Artículo</th>
                    <th style={{ padding: '0.65rem 1rem', color: 'var(--text-dim, #9ca3af)', fontWeight: 600, textAlign: 'center' }}>Cant.</th>
                    <th style={{ padding: '0.65rem 1rem', color: 'var(--text-dim, #9ca3af)', fontWeight: 600, textAlign: 'right' }}>Total</th>
                  </tr>
                </thead>
                <tbody>
                  {candidate.matchedItems.map((item, idx) => (
                    <tr
                      key={item.id || idx}
                      style={{
                        borderBottom: '1px solid var(--border-glass, rgba(255, 255, 255, 0.05))',
                      }}
                    >
                      <td style={{ padding: '0.75rem 1rem', color: 'var(--text-main, #f3f4f6)', fontWeight: 500 }}>
                        {item.title}
                        {item.sku && (
                          <span style={{ display: 'block', fontSize: '0.72rem', color: 'var(--text-muted, #6b7280)', fontFamily: 'monospace' }}>
                            SKU: {item.sku}
                          </span>
                        )}
                      </td>
                      <td style={{ padding: '0.75rem 1rem', textAlign: 'center', color: 'var(--text-dim, #9ca3af)' }}>
                        {item.quantity}
                      </td>
                      <td style={{ padding: '0.75rem 1rem', textAlign: 'right', fontWeight: 600, color: 'var(--text-main, #f3f4f6)' }}>
                        {item.totalPrice.toFixed(2)} €
                      </td>
                    </tr>
                  ))}
                  {discrepancy > 0 && (
                    <tr style={{ background: 'hsla(38, 92%, 50%, 0.05)' }}>
                      <td style={{ padding: '0.75rem 1rem', color: '#fbbf24', fontWeight: 500 }} colSpan={2}>
                        Gastos de Envío / Ajuste Pedido
                      </td>
                      <td style={{ padding: '0.75rem 1rem', textAlign: 'right', fontWeight: 600, color: '#fbbf24' }}>
                        {discrepancy.toFixed(2)} €
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* Salvaguarda Invariante RN-02 */}
          <div
            style={{
              marginTop: '1.25rem',
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
              fontSize: '0.78rem',
              color: 'var(--text-dim, #9ca3af)',
            }}
          >
            <ShieldCheck size={16} style={{ color: '#4ade80' }} />
            <span>
              Invariante Contable Garantizada: El desglose se registrará con discrepancia exacta de 0,00 € y protegerá la
              transacción de sobreescrituras automáticas futuras (RN-01).
            </span>
          </div>

          {showRejectInput && (
            <div style={{ marginTop: '1rem' }}>
              <input
                type="text"
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
                placeholder="Motivo del descarte (opcional, ej. no corresponde a esta compra)"
                style={{
                  width: '100%',
                  padding: '0.65rem 0.85rem',
                  background: 'hsla(222, 47%, 16%, 0.4)',
                  border: '1px solid var(--border-glass, rgba(255, 255, 255, 0.1))',
                  borderRadius: '0.5rem',
                  color: '#fff',
                  fontSize: '0.85rem',
                }}
              />
            </div>
          )}

          {errorMessage && (
            <div
              style={{
                marginTop: '1rem',
                padding: '0.75rem 1rem',
                borderRadius: '0.5rem',
                background: 'hsla(0, 84%, 60%, 0.12)',
                border: '1px solid hsla(0, 84%, 60%, 0.3)',
                color: '#f87171',
                fontSize: '0.85rem',
                display: 'flex',
                alignItems: 'center',
                gap: '0.5rem',
              }}
            >
              <AlertCircle size={16} />
              <span>{errorMessage}</span>
            </div>
          )}
        </div>

        {/* Acciones */}
        <div
          style={{
            padding: '1.25rem 1.5rem',
            borderTop: '1px solid var(--border-glass, rgba(255, 255, 255, 0.08))',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            background: 'hsla(222, 47%, 12%, 0.5)',
          }}
        >
          {!showRejectInput ? (
            <button
              onClick={() => setShowRejectInput(true)}
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: '0.4rem',
                background: 'transparent',
                border: '1px solid hsla(0, 84%, 60%, 0.3)',
                borderRadius: '0.5rem',
                color: '#f87171',
                padding: '0.6rem 1rem',
                fontSize: '0.85rem',
                fontWeight: 600,
                cursor: 'pointer',
              }}
            >
              <Trash2 size={15} />
              Descartar propuesta
            </button>
          ) : (
            <button
              onClick={handleReject}
              disabled={rejectReconciliation.isPending}
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: '0.4rem',
                background: 'hsla(0, 84%, 60%, 0.2)',
                border: '1px solid #ef4444',
                borderRadius: '0.5rem',
                color: '#fca5a5',
                padding: '0.6rem 1rem',
                fontSize: '0.85rem',
                fontWeight: 600,
                cursor: 'pointer',
              }}
            >
              Confirmar descarte
            </button>
          )}

          <div style={{ display: 'flex', gap: '0.75rem' }}>
            <button
              onClick={onClose}
              style={{
                background: 'transparent',
                border: '1px solid var(--border-glass, rgba(255, 255, 255, 0.15))',
                borderRadius: '0.5rem',
                color: 'var(--text-dim, #9ca3af)',
                padding: '0.6rem 1.25rem',
                fontSize: '0.85rem',
                fontWeight: 600,
                cursor: 'pointer',
              }}
            >
              Cancelar
            </button>
            <button
              onClick={handleAccept}
              disabled={acceptReconciliation.isPending}
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: '0.5rem',
                background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
                border: 'none',
                borderRadius: '0.5rem',
                color: '#fff',
                padding: '0.6rem 1.5rem',
                fontSize: '0.85rem',
                fontWeight: 600,
                cursor: acceptReconciliation.isPending ? 'not-allowed' : 'pointer',
                opacity: acceptReconciliation.isPending ? 0.7 : 1,
              }}
            >
              <Check size={16} />
              {acceptReconciliation.isPending ? 'Aplicando partidas...' : 'Aceptar y Desglosar en Extracto'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
