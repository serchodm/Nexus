import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useSearchParams } from 'react-router-dom';
import { Users, Plus, Clock, CheckCircle2, AlertCircle, Send as SendIcon } from 'lucide-react';
import { useSharedSpace } from '../hooks/useSharedSpace';
import { ShareExpenseDrawer } from '../components/shared/ShareExpenseDrawer';
import { SettleUpModal } from '../components/shared/SettleUpModal';

export function SharedSpacePage() {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const { summary } = useSharedSpace();

  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [isSettleOpen, setIsSettleOpen] = useState(false);

  // Check if routed with ?action=settle
  useEffect(() => {
    if (searchParams.get('action') === 'settle') {
      setIsSettleOpen(true);
    }
  }, [searchParams]);

  const pendingExpenses = summary.pendingExpenses.filter((e) => e.status === 'pending');
  const settledExpenses = summary.pendingExpenses.filter((e) => e.status === 'settled');
  const netBalance = summary.netBalance;

  const totalShared = summary.totalSharedThisMonth || 728.29;
  const youPaid = summary.youPaidThisMonth || 400.56;
  const partnerPaid = summary.partnerPaidThisMonth || 327.73;
  const youPaidPct = totalShared > 0 ? Math.round((youPaid / totalShared) * 100) : 55;

  return (
    <>
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 'var(--space-md)' }}>
        <div>
          <h2 style={{ margin: 0, display: 'flex', alignItems: 'center', gap: 8 }}>
            <Users size={24} style={{ color: 'var(--secondary)' }} />
            {t('sharedSpace.title')}
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: 'var(--text-body-sm)', marginTop: 4 }}>
            {t('sharedSpace.subtitle')}
          </p>
        </div>
        <button className="btn-primary" onClick={() => setIsDrawerOpen(true)}>
          <Plus size={16} />
          {t('sharedSpace.newExpense')}
        </button>
      </div>

      {/* Summary Cards */}
      <section style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: 'var(--space-md)' }}>
        {/* Members */}
        <div className="glass-panel" style={{ padding: 'var(--space-md)', borderRadius: 'var(--radius-xl)' }}>
          <span className="kpi-card__label">{t('sharedSpace.householdMembers')}</span>
          <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginTop: 12 }}>
            <div style={{ display: 'flex', gap: -8 }}>
              <div className="avatar avatar--lg avatar--primary" style={{ marginRight: -8, zIndex: 2 }}>OP</div>
              <div className="avatar avatar--lg avatar--secondary">PT</div>
            </div>
            <div>
              <div style={{ fontSize: 'var(--text-body-md)', fontWeight: 600 }}>
                {summary.members.map((m) => m.displayName).join(' & ') || 'Sergio & Elena'}
              </div>
              <div style={{ fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)' }}>
                {summary.defaultSplitRatio} {t('dashboard.fairShare')}
              </div>
            </div>
          </div>
        </div>

        {/* Settlement Balance */}
        <div className="glass-panel" style={{
          padding: 'var(--space-md)', borderRadius: 'var(--radius-xl)',
          borderColor: 'rgba(76, 215, 246, 0.3)',
        }}>
          <span className="kpi-card__label">{t('sharedSpace.currentBalance')}</span>
          <div style={{ marginTop: 8 }}>
            <span className="kpi-card__value numeral text-secondary">
              €{Math.abs(netBalance).toFixed(2)}
            </span>
            <p style={{ fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)', marginTop: 4 }}>
              {netBalance > 0
                ? t('sharedSpace.partnerOwesYou')
                : netBalance < 0
                ? t('dashboard.owedByYou')
                : t('sharedSpace.settledBalanceAllGood')}
            </p>
          </div>
          <button
            className="btn-primary"
            style={{ marginTop: 12, width: '100%', justifyContent: 'center' }}
            onClick={() => setIsSettleOpen(true)}
          >
            <SendIcon size={14} />
            {t('sharedSpace.settleBizum')}
          </button>
        </div>

        {/* Monthly Shared Total */}
        <div className="glass-panel" style={{ padding: 'var(--space-md)', borderRadius: 'var(--radius-xl)' }}>
          <span className="kpi-card__label">{t('sharedSpace.totalSharedMonth')}</span>
          <div style={{ marginTop: 8 }}>
            <span className="kpi-card__value numeral">€{totalShared.toFixed(2)}</span>
          </div>
          <div style={{ marginTop: 12 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 'var(--text-label-sm)', marginBottom: 4 }}>
              <span className="numeral" style={{ color: 'var(--text-main)' }}>{t('sharedSpace.youPaid')}: €{youPaid.toFixed(2)}</span>
              <span className="numeral" style={{ color: 'var(--text-muted)' }}>{t('sharedSpace.partnerPaid')}: €{partnerPaid.toFixed(2)}</span>
            </div>
            <div className="progress-bar">
              <div className="progress-bar__fill progress-bar__fill--secondary" style={{ width: `${youPaidPct}%` }} />
              <div className="progress-bar__fill progress-bar__fill--primary" style={{ width: `${100 - youPaidPct}%`, opacity: 0.5 }} />
            </div>
          </div>
        </div>
      </section>

      {/* Pending Settlements */}
      <section className="glass-panel" style={{ borderRadius: 'var(--radius-xl)', overflow: 'hidden' }}>
        <div style={{ padding: 'var(--space-md)', borderBottom: '1px solid var(--border-outline-variant)', display: 'flex', alignItems: 'center', gap: 'var(--space-sm)' }}>
          <Clock size={18} style={{ color: 'var(--warning)' }} />
          <h4 style={{ margin: 0 }}>{t('sharedSpace.pendingExpenses')}</h4>
          <span className="badge badge--outline numeral">{pendingExpenses.length}</span>
        </div>
        <div style={{ overflowX: 'auto' }}>
          <table className="data-table">
            <thead>
              <tr>
                <th>{t('dashboard.date')}</th>
                <th>{t('sharedSpace.description')}</th>
                <th>{t('sharedSpace.paidBy')}</th>
                <th>{t('sharedSpace.splitRatio')}</th>
                <th style={{ textAlign: 'right' }}>{t('sharedSpace.total')}</th>
                <th style={{ textAlign: 'right' }}>{t('sharedSpace.yourShare')}</th>
                <th style={{ textAlign: 'right' }}>{t('sharedSpace.partnerShare')}</th>
                <th style={{ textAlign: 'center' }}>{t('sharedSpace.status')}</th>
              </tr>
            </thead>
            <tbody>
              {pendingExpenses.length === 0 ? (
                <tr>
                  <td colSpan={8} style={{ textAlign: 'center', padding: 'var(--space-xl)', color: 'var(--text-muted)' }}>
                    {t('sharedSpace.settledBalanceAllGood')}
                  </td>
                </tr>
              ) : (
                pendingExpenses.map((exp) => (
                  <tr key={exp.id}>
                    <td className="numeral" style={{ color: 'var(--text-muted)' }}>{exp.transactionDate}</td>
                    <td style={{ fontWeight: 500, color: 'var(--text-main)' }}>{exp.description}</td>
                    <td>
                      <span className="badge badge--secondary" style={{ fontSize: 11 }}>
                        {exp.paidByName || (exp.paidByMemberId === 'm-1' ? 'Sergio' : 'Elena')}
                      </span>
                    </td>
                    <td className="numeral" style={{ color: 'var(--text-muted)' }}>{exp.splitRatio || summary.defaultSplitRatio}</td>
                    <td className="numeral" style={{ textAlign: 'right', fontWeight: 600 }}>€{exp.totalAmount.toFixed(2)}</td>
                    <td className="numeral" style={{ textAlign: 'right', color: 'var(--text-main)' }}>€{exp.paidByShare.toFixed(2)}</td>
                    <td className="numeral" style={{ textAlign: 'right', color: 'var(--secondary)' }}>€{exp.owedShare.toFixed(2)}</td>
                    <td style={{ textAlign: 'center' }}>
                      <span className="badge badge--neutral" style={{ gap: 4 }}>
                        <AlertCircle size={10} /> {t('sharedSpace.pending')}
                      </span>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </section>

      {/* Settled History */}
      <section className="glass-panel" style={{ borderRadius: 'var(--radius-xl)', overflow: 'hidden' }}>
        <div style={{ padding: 'var(--space-md)', borderBottom: '1px solid var(--border-outline-variant)', display: 'flex', alignItems: 'center', gap: 'var(--space-sm)' }}>
          <CheckCircle2 size={18} style={{ color: 'var(--success)' }} />
          <h4 style={{ margin: 0 }}>{t('sharedSpace.settledExpenses')}</h4>
          <span className="badge badge--outline numeral">{settledExpenses.length}</span>
        </div>
        <div style={{ overflowX: 'auto' }}>
          <table className="data-table">
            <thead>
              <tr>
                <th>{t('dashboard.date')}</th>
                <th>{t('sharedSpace.description')}</th>
                <th>{t('sharedSpace.paidBy')}</th>
                <th>{t('sharedSpace.splitRatio')}</th>
                <th style={{ textAlign: 'right' }}>{t('sharedSpace.total')}</th>
                <th style={{ textAlign: 'right' }}>{t('sharedSpace.yourShare')}</th>
                <th style={{ textAlign: 'right' }}>{t('sharedSpace.partnerShare')}</th>
                <th style={{ textAlign: 'center' }}>{t('sharedSpace.status')}</th>
              </tr>
            </thead>
            <tbody>
              {settledExpenses.length === 0 ? (
                <tr>
                  <td colSpan={8} style={{ textAlign: 'center', padding: 'var(--space-lg)', color: 'var(--text-muted)' }}>
                    No hay liquidaciones históricas registradas.
                  </td>
                </tr>
              ) : (
                settledExpenses.map((exp) => (
                  <tr key={exp.id} style={{ opacity: 0.75 }}>
                    <td className="numeral" style={{ color: 'var(--text-muted)' }}>{exp.transactionDate}</td>
                    <td style={{ fontWeight: 500, color: 'var(--text-main)' }}>{exp.description}</td>
                    <td>
                      <span className="badge badge--neutral" style={{ fontSize: 11 }}>
                        {exp.paidByName || (exp.paidByMemberId === 'm-1' ? 'Sergio' : 'Elena')}
                      </span>
                    </td>
                    <td className="numeral" style={{ color: 'var(--text-muted)' }}>{exp.splitRatio || summary.defaultSplitRatio}</td>
                    <td className="numeral" style={{ textAlign: 'right' }}>€{exp.totalAmount.toFixed(2)}</td>
                    <td className="numeral" style={{ textAlign: 'right', color: 'var(--text-main)' }}>€{exp.paidByShare.toFixed(2)}</td>
                    <td className="numeral" style={{ textAlign: 'right', color: 'var(--text-muted)' }}>€{exp.owedShare.toFixed(2)}</td>
                    <td style={{ textAlign: 'center' }}>
                      <span className="badge badge--success" style={{ gap: 4 }}>
                        <CheckCircle2 size={10} /> {t('sharedSpace.settled')}
                      </span>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </section>

      {/* Drawers and Modals */}
      <ShareExpenseDrawer
        isOpen={isDrawerOpen}
        onClose={() => setIsDrawerOpen(false)}
      />

      <SettleUpModal
        isOpen={isSettleOpen}
        onClose={() => setIsSettleOpen(false)}
      />
    </>
  );
}
