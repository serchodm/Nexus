import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { X, Check, Users, ShieldCheck, Sparkles, SlidersHorizontal } from 'lucide-react';
import { useSharedSpace } from '../../hooks/useSharedSpace';

interface ShareExpenseDrawerProps {
  isOpen: boolean;
  onClose: () => void;
  transaction?: {
    id?: number | string;
    merchant: string;
    amount: number;
    date?: string;
    category?: string;
  } | null;
  onSuccess?: () => void;
}

export function ShareExpenseDrawer({ isOpen, onClose, transaction, onSuccess }: ShareExpenseDrawerProps) {
  const { t } = useTranslation();
  const { summary, shareExpense, isSharing } = useSharedSpace();

  const [ruleMode, setRuleMode] = useState<'50/50' | '55/45' | 'custom'>('55/45');
  const [customPct, setCustomPct] = useState<number>(55);
  const [hidePrivateDetails, setHidePrivateDetails] = useState<boolean>(true);
  const [isSuccessFeedback, setIsSuccessFeedback] = useState<boolean>(false);

  // Fallback transaction if opened manually
  const merchantName = transaction?.merchant || 'Gasto General';
  const totalAmount = Math.abs(transaction?.amount ? Number(transaction.amount) : 120.00);

  const ownerPercentage = ruleMode === '50/50' ? 50 : ruleMode === '55/45' ? 55 : customPct;
  const partnerPercentage = 100 - ownerPercentage;

  // Banker's calculation: debtor share = total - payer share
  const yourShare = Number((totalAmount * (ownerPercentage / 100)).toFixed(2));
  const partnerShare = Number((totalAmount - yourShare).toFixed(2));

  if (!isOpen) return null;

  const handleConfirm = async () => {
    try {
      const ownerMember = summary.members.find(m => m.role === 'Owner') || summary.members[0];
      await shareExpense({
        sharedSpaceId: summary.id,
        bankTransactionId: String(transaction?.id || Date.now()),
        paidByMemberId: ownerMember?.id || 'm-1',
        payerPercentage: ownerPercentage,
        totalAmount,
        customDescription: hidePrivateDetails ? merchantName : `${merchantName} (Detalle cuenta)`,
      });

      setIsSuccessFeedback(true);
      setTimeout(() => {
        setIsSuccessFeedback(false);
        if (onSuccess) onSuccess();
        onClose();
      }, 700);
    } catch (err) {
      console.error('Failed to share expense:', err);
    }
  };

  return (
    <div style={{
      position: 'fixed',
      top: 0,
      left: 0,
      right: 0,
      bottom: 0,
      zIndex: 1000,
      display: 'flex',
      justifyContent: 'flex-end',
      backgroundColor: 'rgba(0, 0, 0, 0.65)',
      backdropFilter: 'blur(6px)',
      animation: 'fadeIn 0.2s ease-out',
    }}>
      {/* Backdrop click handler */}
      <div
        style={{ position: 'absolute', inset: 0 }}
        onClick={onClose}
      />

      {/* Drawer Container */}
      <aside style={{
        position: 'relative',
        width: '100%',
        maxWidth: 440,
        height: '100%',
        background: 'var(--bg-surface-container, #131b26)',
        borderLeft: '1px solid var(--border-outline-variant, rgba(76, 215, 246, 0.2))',
        boxShadow: '-8px 0 32px rgba(0, 0, 0, 0.5)',
        display: 'flex',
        flexDirection: 'column',
        zIndex: 1001,
        overflowY: 'auto',
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
              width: 34,
              height: 34,
              borderRadius: 'var(--radius-md, 8px)',
              background: 'rgba(76, 215, 246, 0.15)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--secondary, #4cd7f6)',
            }}>
              <Users size={18} />
            </div>
            <div>
              <h3 style={{ margin: 0, fontSize: 16, fontWeight: 700, color: 'var(--text-main, #fff)' }}>
                {t('sharedSpace.shareExpenseTitle')}
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

        {/* Content Body */}
        <div style={{ padding: 'var(--space-lg, 20px)', display: 'flex', flexDirection: 'column', gap: 20, flex: 1 }}>
          {/* Transaction Summary Card */}
          <div className="glass-panel" style={{
            padding: 16,
            borderRadius: 'var(--radius-lg, 12px)',
            border: '1px solid var(--border-outline-variant, rgba(76, 215, 246, 0.2))',
            background: 'var(--bg-surface-container-low, rgba(16, 24, 38, 0.6))',
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <div>
                <span style={{ fontSize: 11, textTransform: 'uppercase', letterSpacing: '0.05em', color: 'var(--text-muted, #8b9bb4)', fontWeight: 600 }}>
                  {transaction?.category || 'Gasto'}
                </span>
                <h4 style={{ margin: '4px 0 0', fontSize: 16, color: 'var(--text-main, #fff)' }}>
                  {merchantName}
                </h4>
                {transaction?.date && (
                  <span style={{ fontSize: 12, color: 'var(--text-dim, #63758d)' }}>{transaction.date}</span>
                )}
              </div>
              <div style={{ textAlign: 'right' }}>
                <span className="numeral" style={{ fontSize: 22, fontWeight: 700, color: 'var(--secondary, #4cd7f6)' }}>
                  €{totalAmount.toFixed(2)}
                </span>
              </div>
            </div>
          </div>

          {/* Split Rule Selector */}
          <div>
            <label style={{ display: 'block', fontSize: 13, fontWeight: 600, color: 'var(--text-main, #fff)', marginBottom: 8 }}>
              {t('sharedSpace.splitRatioRule')}
            </label>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1.3fr 1fr', gap: 8 }}>
              {/* 50/50 */}
              <button
                type="button"
                onClick={() => setRuleMode('50/50')}
                style={{
                  padding: '8px 10px',
                  borderRadius: 'var(--radius-md, 8px)',
                  fontSize: 12,
                  fontWeight: 600,
                  cursor: 'pointer',
                  border: ruleMode === '50/50' ? '1px solid var(--secondary, #4cd7f6)' : '1px solid var(--border-outline-variant, rgba(255, 255, 255, 0.1))',
                  background: ruleMode === '50/50' ? 'rgba(76, 215, 246, 0.15)' : 'var(--bg-surface-container-high, #1b2636)',
                  color: ruleMode === '50/50' ? 'var(--secondary, #4cd7f6)' : 'var(--text-muted, #8b9bb4)',
                  transition: 'all 0.15s ease',
                }}
              >
                50 / 50
              </button>

              {/* 55/45 Fair-Share */}
              <button
                type="button"
                onClick={() => setRuleMode('55/45')}
                style={{
                  padding: '8px 10px',
                  borderRadius: 'var(--radius-md, 8px)',
                  fontSize: 12,
                  fontWeight: 600,
                  cursor: 'pointer',
                  border: ruleMode === '55/45' ? '1px solid var(--secondary, #4cd7f6)' : '1px solid var(--border-outline-variant, rgba(255, 255, 255, 0.1))',
                  background: ruleMode === '55/45' ? 'rgba(76, 215, 246, 0.15)' : 'var(--bg-surface-container-high, #1b2636)',
                  color: ruleMode === '55/45' ? 'var(--secondary, #4cd7f6)' : 'var(--text-muted, #8b9bb4)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: 4,
                  transition: 'all 0.15s ease',
                }}
              >
                <Sparkles size={12} />
                55 / 45 Fair-Share
              </button>

              {/* Custom */}
              <button
                type="button"
                onClick={() => setRuleMode('custom')}
                style={{
                  padding: '8px 10px',
                  borderRadius: 'var(--radius-md, 8px)',
                  fontSize: 12,
                  fontWeight: 600,
                  cursor: 'pointer',
                  border: ruleMode === 'custom' ? '1px solid var(--secondary, #4cd7f6)' : '1px solid var(--border-outline-variant, rgba(255, 255, 255, 0.1))',
                  background: ruleMode === 'custom' ? 'rgba(76, 215, 246, 0.15)' : 'var(--bg-surface-container-high, #1b2636)',
                  color: ruleMode === 'custom' ? 'var(--secondary, #4cd7f6)' : 'var(--text-muted, #8b9bb4)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: 4,
                  transition: 'all 0.15s ease',
                }}
              >
                <SlidersHorizontal size={12} />
                {t('sharedSpace.customSplit')}
              </button>
            </div>

            {/* Custom slider if custom mode */}
            {ruleMode === 'custom' && (
              <div style={{ marginTop: 12, padding: 12, borderRadius: 8, background: 'rgba(0, 0, 0, 0.25)' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 12, marginBottom: 6 }}>
                  <span style={{ color: 'var(--text-muted)' }}>Tú: {customPct}%</span>
                  <span style={{ color: 'var(--secondary)' }}>Pareja: {100 - customPct}%</span>
                </div>
                <input
                  type="range"
                  min={30}
                  max={70}
                  step={5}
                  value={customPct}
                  onChange={(e) => setCustomPct(Number(e.target.value))}
                  style={{ width: '100%', accentColor: 'var(--secondary, #4cd7f6)', cursor: 'pointer' }}
                />
              </div>
            )}
          </div>

          {/* Live Dynamic Breakdown */}
          <div style={{
            padding: 16,
            borderRadius: 'var(--radius-lg, 12px)',
            background: 'var(--bg-surface-container-high, #1b2636)',
            border: '1px solid var(--border-outline-variant, rgba(255, 255, 255, 0.08))',
            display: 'flex',
            flexDirection: 'column',
            gap: 12,
          }}>
            <span style={{ fontSize: 11, textTransform: 'uppercase', color: 'var(--text-muted)', fontWeight: 600 }}>
              Desglose en Tiempo Real
            </span>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <div className="avatar avatar--sm avatar--primary" style={{ fontSize: 10 }}>OP</div>
                <div>
                  <div style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-main)' }}>
                    {t('sharedSpace.yourRealCost')}
                  </div>
                  <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>
                    Cuota del {ownerPercentage}%
                  </div>
                </div>
              </div>
              <span className="numeral" style={{ fontSize: 15, fontWeight: 700, color: 'var(--text-main)' }}>
                €{yourShare.toFixed(2)}
              </span>
            </div>

            <div style={{ height: 1, background: 'var(--border-outline-variant, rgba(255, 255, 255, 0.06))' }} />

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <div className="avatar avatar--sm avatar--secondary" style={{ fontSize: 10 }}>PT</div>
                <div>
                  <div style={{ fontSize: 13, fontWeight: 600, color: 'var(--secondary, #4cd7f6)' }}>
                    {t('sharedSpace.partnerOwesAmount')}
                  </div>
                  <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>
                    Deuda imputable ({partnerPercentage}%)
                  </div>
                </div>
              </div>
              <span className="numeral" style={{ fontSize: 15, fontWeight: 700, color: 'var(--secondary, #4cd7f6)' }}>
                +€{partnerShare.toFixed(2)}
              </span>
            </div>
          </div>

          {/* Zero-Knowledge Privacy Wall Toggle */}
          <div style={{
            display: 'flex',
            alignItems: 'flex-start',
            gap: 10,
            padding: 12,
            borderRadius: 'var(--radius-md, 8px)',
            background: 'rgba(76, 215, 246, 0.05)',
            border: '1px solid rgba(76, 215, 246, 0.15)',
          }}>
            <ShieldCheck size={18} style={{ color: 'var(--secondary, #4cd7f6)', marginTop: 2, flexShrink: 0 }} />
            <div style={{ flex: 1 }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer', fontSize: 12, color: 'var(--text-main)', fontWeight: 500 }}>
                <input
                  type="checkbox"
                  checked={hidePrivateDetails}
                  onChange={(e) => setHidePrivateDetails(e.target.checked)}
                  style={{ accentColor: 'var(--secondary, #4cd7f6)', cursor: 'pointer', width: 14, height: 14 }}
                />
                {t('sharedSpace.privacyToggle')}
              </label>
            </div>
          </div>
        </div>

        {/* Footer Actions */}
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
            disabled={isSharing}
            style={{ flex: 1, justifyContent: 'center' }}
          >
            {t('sharedSpace.cancel')}
          </button>
          <button
            id="btn-confirm-share"
            type="button"
            className={`btn-primary ${isSuccessFeedback ? 'cyan-glow' : ''}`}
            onClick={handleConfirm}
            disabled={isSharing}
            style={{
              flex: 1.5,
              justifyContent: 'center',
              backgroundColor: isSuccessFeedback ? 'var(--success, #22c55e)' : undefined,
              borderColor: isSuccessFeedback ? 'var(--success, #22c55e)' : undefined,
              transition: 'all 0.2s ease',
            }}
          >
            {isSuccessFeedback ? (
              <>
                <Check size={16} />
                <span>{t('sharedSpace.settled')}</span>
              </>
            ) : (
              <>
                <Users size={16} />
                <span>{isSharing ? '...' : t('sharedSpace.confirmShare')}</span>
              </>
            )}
          </button>
        </div>
      </aside>
    </div>
  );
}
