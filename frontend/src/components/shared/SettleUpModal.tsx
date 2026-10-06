import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { X, Check, Copy, Send, CheckCircle2, Smartphone, Landmark } from 'lucide-react';
import { useSharedSpace } from '../../hooks/useSharedSpace';

interface SettleUpModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess?: () => void;
}

export function SettleUpModal({ isOpen, onClose, onSuccess }: SettleUpModalProps) {
  const { t } = useTranslation();
  const { summary, settleBalance, isSettling } = useSharedSpace();

  const [paymentMethod, setPaymentMethod] = useState<'Bizum' | 'SepaTransfer'>('Bizum');
  const [copied, setCopied] = useState<boolean>(false);
  const [isSuccessFeedback, setIsSuccessFeedback] = useState<boolean>(false);
  const [animatedBalance, setAnimatedBalance] = useState<number | null>(null);

  if (!isOpen) return null;

  const currentBalance = summary.netBalance;
  const displayAmount = animatedBalance !== null ? animatedBalance : currentBalance;

  const debtorMember = summary.members.find(m => m.role === 'Member') || summary.members[1] || { displayName: 'Elena', id: 'm-2' };
  const creditorMember = summary.members.find(m => m.role === 'Owner') || summary.members[0] || { displayName: 'Sergio', id: 'm-1' };

  const bizumConcept = `Nexus Liq #104 ${debtorMember.displayName} ${Math.abs(currentBalance).toFixed(2)} EUR`;

  const handleCopyConcept = async () => {
    try {
      await navigator.clipboard.writeText(bizumConcept);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // Fallback
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  const handleConfirmSettle = async () => {
    try {
      // Animated count down to 0.00
      setAnimatedBalance(0.00);
      setIsSuccessFeedback(true);

      await settleBalance({
        sharedSpaceId: summary.id,
        payerMemberId: debtorMember.id,
        receiverMemberId: creditorMember.id,
        amount: Math.abs(currentBalance),
        method: paymentMethod,
        referenceCode: bizumConcept,
      });

      setTimeout(() => {
        setIsSuccessFeedback(false);
        setAnimatedBalance(null);
        if (onSuccess) onSuccess();
        onClose();
      }, 1100);
    } catch (err) {
      console.error('Failed to settle balance:', err);
      setIsSuccessFeedback(false);
      setAnimatedBalance(null);
    }
  };

  return (
    <div style={{
      position: 'fixed',
      top: 0,
      left: 0,
      right: 0,
      bottom: 0,
      zIndex: 1100,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      backgroundColor: 'rgba(0, 0, 0, 0.75)',
      backdropFilter: 'blur(8px)',
      padding: 'var(--space-md, 16px)',
      animation: 'fadeIn 0.2s ease-out',
    }}>
      {/* Backdrop */}
      <div style={{ position: 'absolute', inset: 0 }} onClick={onClose} />

      {/* Modal Dialog */}
      <div style={{
        position: 'relative',
        width: '100%',
        maxWidth: 480,
        background: 'var(--bg-surface-container, #131b26)',
        borderRadius: 'var(--radius-xl, 16px)',
        border: '1px solid var(--border-outline-variant, rgba(76, 215, 246, 0.3))',
        boxShadow: '0 20px 50px rgba(0, 0, 0, 0.6), 0 0 30px rgba(76, 215, 246, 0.1)',
        display: 'flex',
        flexDirection: 'column',
        zIndex: 1101,
        overflow: 'hidden',
      }}>
        {/* Header */}
        <div style={{
          padding: 'var(--space-lg, 20px)',
          borderBottom: '1px solid var(--border-outline-variant, rgba(255, 255, 255, 0.08))',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <div style={{
              width: 36,
              height: 36,
              borderRadius: 'var(--radius-md, 8px)',
              background: 'rgba(76, 215, 246, 0.15)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--secondary, #4cd7f6)',
            }}>
              <Send size={18} />
            </div>
            <div>
              <h3 style={{ margin: 0, fontSize: 16, fontWeight: 700, color: 'var(--text-main, #fff)' }}>
                {t('sharedSpace.settleModalTitle')}
              </h3>
              <span style={{ fontSize: 12, color: 'var(--text-muted, #8b9bb4)' }}>
                {summary.name}
              </span>
            </div>
          </div>
          <button
            className="btn-ghost"
            onClick={onClose}
            style={{ padding: 6, borderRadius: '50%' }}
            aria-label="Close"
          >
            <X size={18} />
          </button>
        </div>

        {/* Modal Body */}
        <div style={{ padding: 'var(--space-lg, 20px)', display: 'flex', flexDirection: 'column', gap: 18 }}>
          {/* Amount Hero Card */}
          <div style={{
            textAlign: 'center',
            padding: '24px 16px',
            borderRadius: 'var(--radius-lg, 12px)',
            background: 'linear-gradient(180deg, rgba(76, 215, 246, 0.08) 0%, rgba(19, 27, 38, 0.8) 100%)',
            border: '1px solid rgba(76, 215, 246, 0.25)',
          }}>
            <span style={{ fontSize: 12, textTransform: 'uppercase', letterSpacing: '0.05em', color: 'var(--text-muted, #8b9bb4)', fontWeight: 600 }}>
              {t('sharedSpace.currentBalance')}
            </span>
            <div style={{ marginTop: 6, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 4 }}>
              <span
                className="numeral"
                style={{
                  fontSize: 38,
                  fontWeight: 800,
                  color: isSuccessFeedback ? 'var(--success, #22c55e)' : 'var(--secondary, #4cd7f6)',
                  transition: 'all 0.4s ease',
                }}
              >
                €{Math.abs(displayAmount).toFixed(2)}
              </span>
            </div>
            <p style={{ margin: '6px 0 0', fontSize: 13, color: 'var(--text-muted, #8b9bb4)' }}>
              {isSuccessFeedback ? t('sharedSpace.settledBalanceAllGood') : `${debtorMember.displayName} te reembolsa`}
            </p>
          </div>

          {/* Payment Method Selector */}
          <div>
            <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--text-main, #fff)', marginBottom: 8 }}>
              Método de Liquidación
            </label>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10 }}>
              <button
                type="button"
                onClick={() => setPaymentMethod('Bizum')}
                style={{
                  padding: '10px 12px',
                  borderRadius: 'var(--radius-md, 8px)',
                  fontSize: 13,
                  fontWeight: 600,
                  cursor: 'pointer',
                  border: paymentMethod === 'Bizum' ? '1px solid var(--secondary, #4cd7f6)' : '1px solid var(--border-outline-variant, rgba(255, 255, 255, 0.1))',
                  background: paymentMethod === 'Bizum' ? 'rgba(76, 215, 246, 0.15)' : 'var(--bg-surface-container-high, #1b2636)',
                  color: paymentMethod === 'Bizum' ? 'var(--secondary, #4cd7f6)' : 'var(--text-muted, #8b9bb4)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: 8,
                  transition: 'all 0.15s ease',
                }}
              >
                <Smartphone size={16} />
                Bizum Inmediato
              </button>

              <button
                type="button"
                onClick={() => setPaymentMethod('SepaTransfer')}
                style={{
                  padding: '10px 12px',
                  borderRadius: 'var(--radius-md, 8px)',
                  fontSize: 13,
                  fontWeight: 600,
                  cursor: 'pointer',
                  border: paymentMethod === 'SepaTransfer' ? '1px solid var(--secondary, #4cd7f6)' : '1px solid var(--border-outline-variant, rgba(255, 255, 255, 0.1))',
                  background: paymentMethod === 'SepaTransfer' ? 'rgba(76, 215, 246, 0.15)' : 'var(--bg-surface-container-high, #1b2636)',
                  color: paymentMethod === 'SepaTransfer' ? 'var(--secondary, #4cd7f6)' : 'var(--text-muted, #8b9bb4)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: 8,
                  transition: 'all 0.15s ease',
                }}
              >
                <Landmark size={16} />
                Transferencia SEPA
              </button>
            </div>
          </div>

          {/* Bizum Reference Box */}
          <div style={{
            padding: 14,
            borderRadius: 'var(--radius-lg, 12px)',
            background: 'var(--bg-surface-container-high, #1b2636)',
            border: '1px solid var(--border-outline-variant, rgba(255, 255, 255, 0.08))',
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 6 }}>
              <span style={{ fontSize: 11, textTransform: 'uppercase', color: 'var(--text-muted, #8b9bb4)', fontWeight: 600 }}>
                {t('sharedSpace.bizumConcept')}
              </span>
              <button
                type="button"
                className="btn-ghost"
                onClick={handleCopyConcept}
                style={{ fontSize: 11, padding: '2px 8px', gap: 4, color: copied ? 'var(--success, #22c55e)' : 'var(--secondary, #4cd7f6)' }}
              >
                {copied ? <Check size={12} /> : <Copy size={12} />}
                {copied ? t('sharedSpace.conceptCopied') : t('sharedSpace.copyConcept')}
              </button>
            </div>
            <code style={{
              display: 'block',
              padding: '8px 10px',
              borderRadius: 6,
              background: 'rgba(0, 0, 0, 0.35)',
              color: 'var(--text-main, #fff)',
              fontSize: 12,
              fontFamily: 'var(--font-mono, monospace)',
              wordBreak: 'break-all',
            }}>
              {bizumConcept}
            </code>
          </div>

          {/* Quick instructions / phone */}
          <div style={{ fontSize: 12, color: 'var(--text-muted, #8b9bb4)', lineHeight: 1.5 }}>
            <p style={{ margin: 0 }}>
              {t('sharedSpace.settleModalSubtitle')}
            </p>
          </div>
        </div>

        {/* Footer */}
        <div style={{
          padding: 'var(--space-lg, 20px)',
          borderTop: '1px solid var(--border-outline-variant, rgba(255, 255, 255, 0.08))',
          display: 'flex',
          gap: 10,
        }}>
          <button
            type="button"
            className="btn-secondary"
            onClick={onClose}
            disabled={isSettling}
            style={{ flex: 1, justifyContent: 'center' }}
          >
            {t('sharedSpace.cancel')}
          </button>
          <button
            id="btn-confirm-settle"
            type="button"
            className={`btn-primary ${isSuccessFeedback ? 'cyan-glow' : ''}`}
            onClick={handleConfirmSettle}
            disabled={isSettling}
            style={{
              flex: 1.8,
              justifyContent: 'center',
              backgroundColor: isSuccessFeedback ? 'var(--success, #22c55e)' : undefined,
              borderColor: isSuccessFeedback ? 'var(--success, #22c55e)' : undefined,
              transition: 'all 0.25s ease',
            }}
          >
            {isSuccessFeedback ? (
              <>
                <CheckCircle2 size={16} />
                <span>{t('sharedSpace.settlementSuccess')}</span>
              </>
            ) : (
              <>
                <Check size={16} />
                <span>{isSettling ? '...' : t('sharedSpace.confirmSettlement')}</span>
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
