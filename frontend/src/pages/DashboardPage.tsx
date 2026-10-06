import { useTranslation } from 'react-i18next';
import { Shield, ArrowUpRight, MoreHorizontal, Split, Receipt, ChevronDown } from 'lucide-react';
import { useSharedSpace } from '../hooks/useSharedSpace';

const RECENT_TRANSACTIONS = [
  {
    id: 1,
    date: 'Today, 14:28',
    merchant: 'Mercadona Gran Vía',
    initials: 'MR',
    subtitle: 'TXID #NX-99841-EUR',
    nature: 'Shared Split',
    natureType: 'split',
    category: 'Groceries & Home',
    account: 'BBVA ••4521',
    amount: -120.00,
    postBalance: 34820.40,
    hasSplit: true,
    splitItems: [
      { name: 'Supermarket Provisions & Kitchen', share: 'Fair-Share (50% / 50%)', gross: 80.00, partnerShare: '€40.00 (Pending)', status: 'Reconciled' },
      { name: 'Personal Electronics Cable', share: '100% Personal', gross: 40.00, partnerShare: '€40.00 (Self)', status: 'Excluded' },
    ],
  },
  {
    id: 2,
    date: 'Yesterday, 09:12',
    merchant: 'Empresa Nómina SL',
    initials: 'EN',
    subtitle: 'Monthly Salary Deposit',
    nature: 'True Income',
    natureType: 'income',
    category: 'Salary / Direct Deposit',
    account: 'BBVA ••4521',
    amount: 4850.00,
    postBalance: 34940.40,
  },
  {
    id: 3,
    date: 'Oct 24, 18:05',
    merchant: 'Pareja (Bizum Settle)',
    initials: '👤',
    subtitle: 'SEPA Instant Settlement Ref #419',
    nature: 'Refund Offset',
    natureType: 'refund',
    category: 'Peer Reimbursement',
    account: 'BBVA ••4521',
    amount: 142.50,
    postBalance: 30090.40,
  },
  {
    id: 4,
    date: 'Oct 22, 11:30',
    merchant: 'Cuenta Ahorro ING',
    initials: '⇄',
    subtitle: 'Auto-Sweep Yield Transfer',
    nature: 'Internal Transfer',
    natureType: 'transfer',
    category: 'Liquidity Sweep',
    account: 'BBVA → ING',
    amount: 1500.00,
    postBalance: 29947.90,
  },
  {
    id: 5,
    date: 'Oct 21, 20:14',
    merchant: 'Amazon Web Services EMEA',
    initials: 'AWS',
    subtitle: 'eu-central-1 Server Instances',
    nature: 'Operating Expense',
    natureType: 'expense',
    category: 'Cloud Infrastructure',
    account: 'Revolut ••9021',
    amount: -64.80,
    postBalance: 28447.90,
  },
];

function NatureBadge({ type, label }: { type: string; label: string }) {
  const styles: Record<string, { bg: string; color: string; border: string }> = {
    split:    { bg: 'var(--secondary-bg)', color: 'var(--secondary)', border: 'var(--secondary-border)' },
    income:   { bg: 'var(--success-bg)', color: 'var(--success)', border: 'var(--success-border)' },
    refund:   { bg: 'var(--secondary-bg)', color: 'var(--secondary)', border: 'var(--secondary-border)' },
    transfer: { bg: 'rgba(49, 53, 64, 0.6)', color: 'var(--text-on-surface-variant)', border: 'var(--border-outline-variant)' },
    expense:  { bg: 'var(--danger-bg)', color: 'var(--danger)', border: 'var(--danger-border)' },
  };
  const s = styles[type] || styles.expense;
  return (
    <span style={{
      display: 'inline-flex', alignItems: 'center', gap: 4,
      fontSize: 11, fontWeight: 600,
      padding: '2px 8px', borderRadius: 'var(--radius-full)',
      background: s.bg, color: s.color, border: `1px solid ${s.border}`,
    }}>
      {label}
    </span>
  );
}

export function DashboardPage() {
  const { t } = useTranslation();
  const { summary } = useSharedSpace();

  return (
    <>
      {/* KPI Cards Grid */}
      <section style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: 'var(--space-md)' }}>
        {/* KPI 1: Consolidated Net Worth */}
        <div className="glass-panel kpi-card" style={{ borderColor: 'transparent' }}>
          <div style={{ position: 'absolute', right: -32, top: -32, width: 96, height: 96, background: 'rgba(76, 215, 246, 0.05)', borderRadius: '50%', filter: 'blur(20px)', pointerEvents: 'none' }} />
          <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
            <div>
              <span className="kpi-card__label">{t('dashboard.netWorth')}</span>
              <div style={{ display: 'flex', alignItems: 'baseline', gap: 8, marginTop: 4 }}>
                <span className="kpi-card__value numeral" style={{ color: 'var(--text-main)' }}>€34,820.40</span>
              </div>
            </div>
            <span className="badge badge--success" style={{ fontSize: 11 }}>
              <ArrowUpRight size={12} /> +7.4% MTD
            </span>
          </div>
          {/* Sparkline SVG */}
          <div className="kpi-card__sparkline">
            <svg width="100%" height="100%" viewBox="0 0 200 48" preserveAspectRatio="none" style={{ overflow: 'visible' }}>
              <defs>
                <linearGradient id="cyanSpark" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="#4cd7f6" stopOpacity="0.35" />
                  <stop offset="100%" stopColor="#4cd7f6" stopOpacity="0" />
                </linearGradient>
              </defs>
              <path d="M0,38 C20,35 40,42 60,30 C80,18 100,28 120,16 C140,22 160,8 180,12 C190,14 200,4 200,4 L200,48 L0,48 Z" fill="url(#cyanSpark)" />
              <path d="M0,38 C20,35 40,42 60,30 C80,18 100,28 120,16 C140,22 160,8 180,12 C190,14 200,4 200,4" fill="none" stroke="#4cd7f6" strokeWidth="2" strokeLinecap="round" />
            </svg>
          </div>
          <div className="kpi-card__footer">
            <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <span style={{ width: 6, height: 6, borderRadius: '50%', background: 'var(--secondary)' }} />
              <span>Equities 62%</span>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <span style={{ width: 6, height: 6, borderRadius: '50%', background: 'var(--primary)' }} />
              <span>Cash 28%</span>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <span style={{ width: 6, height: 6, borderRadius: '50%', background: 'var(--tertiary)' }} />
              <span>Crypto 10%</span>
            </div>
          </div>
        </div>

        {/* KPI 2: Monthly Net Cash Flow */}
        <div className="glass-panel kpi-card">
          <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
            <div>
              <span className="kpi-card__label">{t('dashboard.cashFlow')}</span>
              <div style={{ marginTop: 4 }}>
                <span className="kpi-card__value numeral text-success">+€1,840.25</span>
              </div>
            </div>
            <span className="badge badge--outline" style={{ fontSize: 10 }}>{t('dashboard.yieldEfficiency')}</span>
          </div>
          <div style={{ marginTop: 12, display: 'flex', flexDirection: 'column', gap: 6 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 'var(--text-label-sm)' }}>
              <span style={{ color: 'var(--text-muted)' }}>Inflow: <span className="numeral" style={{ color: 'var(--text-main)', fontWeight: 500 }}>€5,420.00</span></span>
              <span style={{ color: 'var(--text-muted)' }}>Outflow: <span className="numeral text-danger" style={{ fontWeight: 500 }}>€3,579.75</span></span>
            </div>
            <div className="progress-bar">
              <div className="progress-bar__fill progress-bar__fill--success" style={{ width: '60%' }} />
              <div className="progress-bar__fill progress-bar__fill--danger" style={{ width: '40%' }} />
            </div>
          </div>
          <div className="kpi-card__footer">
            <span>Offsets deducted:</span>
            <span className="numeral text-secondary">+€420.00 peer return</span>
          </div>
        </div>

        {/* KPI 3: Nexus Health Score */}
        <div className="glass-panel kpi-card">
          <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
            <div>
              <span className="kpi-card__label">{t('dashboard.healthScore')}</span>
              <div style={{ marginTop: 4 }}>
                <span className="kpi-card__value numeral">84<span style={{ fontSize: 'var(--text-headline-md)', color: 'var(--text-muted)' }}>/100</span></span>
              </div>
            </div>
            {/* Ring Gauge */}
            <div style={{ position: 'relative', width: 44, height: 44, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <svg width="100%" height="100%" viewBox="0 0 36 36" style={{ transform: 'rotate(-90deg)' }}>
                <path d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" fill="none" stroke="var(--bg-surface-container-highest)" strokeWidth="3.5" />
                <path d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" fill="none" stroke="var(--secondary)" strokeWidth="3.5" strokeDasharray="84, 100" strokeLinecap="round" />
              </svg>
              <span className="numeral" style={{ position: 'absolute', fontSize: 10, fontWeight: 700, color: 'var(--secondary)' }}>A+</span>
            </div>
          </div>
          <div style={{
            marginTop: 12, display: 'flex', alignItems: 'center', justifyContent: 'space-between',
            fontSize: 'var(--text-label-sm)',
            background: 'rgba(28, 31, 42, 0.6)',
            padding: 8, borderRadius: 'var(--radius-sm)',
            border: '1px solid rgba(66, 71, 84, 0.4)',
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <Shield size={14} style={{ color: 'var(--secondary)' }} />
              <span style={{ color: 'var(--text-muted)' }}>{t('dashboard.runwayMonths')}:</span>
            </div>
            <span className="numeral" style={{ fontWeight: 700, color: 'var(--text-main)' }}>14.2 Months Liquidity</span>
          </div>
          <div className="kpi-card__footer">
            <span>Burn Tolerance</span>
            <span className="numeral text-success">Low Fragility</span>
          </div>
        </div>

        {/* KPI 4: Fair-Share Shared Balance */}
        <div className="glass-panel kpi-card">
          <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
            <div>
              <span className="kpi-card__label">{t('dashboard.fairShare')}</span>
              <div style={{ marginTop: 4 }}>
                <span className="kpi-card__value numeral text-secondary">€{summary.totalSharedThisMonth.toFixed(2)}</span>
              </div>
            </div>
            <span className="badge badge--secondary" style={{ fontSize: 10 }}>Active Sync</span>
          </div>
          <div style={{ marginTop: 8, display: 'flex', flexDirection: 'column', gap: 4 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 'var(--text-label-sm)' }}>
              <span className="numeral" style={{ color: 'var(--text-main)' }}>You: {summary.defaultSplitRatio.split('/')[0]}%</span>
              <span className="numeral" style={{ color: 'var(--text-muted)' }}>Partner: {summary.defaultSplitRatio.split('/')[1]}%</span>
            </div>
            <div className="progress-bar">
              <div className="progress-bar__fill progress-bar__fill--secondary" style={{ width: `${summary.defaultSplitRatio.split('/')[0]}%`, borderRadius: 'var(--radius-full) 0 0 var(--radius-full)' }} />
              <div className="progress-bar__fill progress-bar__fill--primary" style={{ width: `${summary.defaultSplitRatio.split('/')[1]}%`, opacity: 0.5, borderRadius: '0 var(--radius-full) var(--radius-full) 0' }} />
            </div>
          </div>
          <div className="kpi-card__footer">
            <span>{t('sharedSpace.status')}:</span>
            <span className="numeral text-success" style={{ fontWeight: 600 }}>
              {summary.netBalance > 0
                ? `${t('sharedSpace.partnerOwesYou')} €${summary.netBalance.toFixed(2)}`
                : summary.netBalance < 0
                ? `${t('dashboard.owedByYou')} €${Math.abs(summary.netBalance).toFixed(2)}`
                : t('sharedSpace.settledBalanceAllGood')}
            </span>
          </div>
        </div>
      </section>

      {/* Transactions Ledger Table */}
      <section className="glass-panel" style={{ borderRadius: 'var(--radius-xl)', overflow: 'hidden', border: '1px solid var(--border-outline-variant)' }}>
        {/* Table Header Toolbar */}
        <div style={{
          padding: 'var(--space-md)',
          borderBottom: '1px solid var(--border-outline-variant)',
          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
          flexWrap: 'wrap', gap: 'var(--space-md)',
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-sm)' }}>
            <h4 style={{ margin: 0, letterSpacing: '-0.01em' }}>{t('dashboard.institutionalLedger')}</h4>
            <span className="badge badge--outline numeral" style={{ borderRadius: 'var(--radius-full)', fontSize: 'var(--text-label-sm)' }}>38 Verified MTD</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
            <div className="filter-bar">
              <button className="filter-pill filter-pill--active">{t('dashboard.filterAll')}</button>
              <button className="filter-pill">Subscriptions (6)</button>
              <button className="filter-pill">Splits (4)</button>
              <button className="filter-pill">Reconcilable (3)</button>
              <button className="filter-pill">Reimbursements (2)</button>
            </div>
            <button className="btn-secondary" style={{ gap: 6, fontSize: 'var(--text-body-sm)' }}>
              All Categories <ChevronDown size={14} />
            </button>
          </div>
        </div>

        {/* Data Table */}
        <div style={{ overflowX: 'auto' }}>
          <table className="data-table">
            <thead>
              <tr>
                <th>{t('dashboard.date')}</th>
                <th>{t('dashboard.merchant')}</th>
                <th>{t('dashboard.sharedSplit')}</th>
                <th>{t('dashboard.category')}</th>
                <th>{t('dashboard.account')}</th>
                <th style={{ textAlign: 'right' }}>{t('dashboard.amount')}</th>
                <th style={{ textAlign: 'right' }}>{t('dashboard.balanceAfter')}</th>
                <th style={{ textAlign: 'center' }}>{t('dashboard.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {RECENT_TRANSACTIONS.map((tx) => (
                <>
                  <tr key={tx.id} style={{
                    borderLeft: tx.hasSplit ? '2px solid var(--secondary)' : 'none',
                    background: tx.hasSplit ? 'rgba(28, 31, 42, 0.7)' : undefined,
                  }}>
                    <td className="numeral" style={{ color: 'var(--text-muted)' }}>{tx.date}</td>
                    <td>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <div className="avatar avatar--sm avatar--surface">{tx.initials}</div>
                        <div>
                          <div style={{ fontWeight: 500, color: 'var(--text-main)', display: 'flex', alignItems: 'center', gap: 6 }}>
                            {tx.merchant}
                            {tx.hasSplit && <span className="badge badge--secondary" style={{ fontSize: 10 }}>Multi-Split ({tx.splitItems?.length})</span>}
                          </div>
                          <div className="numeral" style={{ fontSize: 11, color: 'var(--text-muted)' }}>{tx.subtitle}</div>
                        </div>
                      </div>
                    </td>
                    <td><NatureBadge type={tx.natureType} label={tx.nature} /></td>
                    <td style={{ color: 'var(--text-on-surface-variant)' }}>{tx.category}</td>
                    <td className="numeral" style={{ color: 'var(--text-muted)' }}>{tx.account}</td>
                    <td className="numeral" style={{
                      textAlign: 'right', fontWeight: 700,
                      color: tx.amount > 0
                        ? (tx.natureType === 'refund' ? 'var(--secondary)' : 'var(--success)')
                        : tx.natureType === 'transfer'
                          ? 'var(--text-main)'
                          : 'var(--danger)',
                    }}>
                      {tx.amount > 0 ? '+' : ''}€{Math.abs(tx.amount).toFixed(2)}
                    </td>
                    <td className="numeral" style={{ textAlign: 'right', color: 'var(--text-muted)' }}>€{tx.postBalance.toFixed(2)}</td>
                    <td style={{ textAlign: 'center' }}>
                      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 4 }}>
                        {tx.natureType === 'expense' && (
                          <>
                            <button className="btn-ghost" title="Split with Partner" style={{ color: 'var(--secondary)' }}><Split size={14} /></button>
                            <button className="btn-ghost" title="Receipt"><Receipt size={14} /></button>
                          </>
                        )}
                        <button className="btn-ghost"><MoreHorizontal size={16} /></button>
                      </div>
                    </td>
                  </tr>
                  {/* Sub-rows for splits */}
                  {tx.hasSplit && tx.splitItems && (
                    <tr key={`${tx.id}-split`} style={{ background: 'rgba(23, 27, 38, 0.4)' }}>
                      <td colSpan={8} style={{ padding: 0 }}>
                        <div style={{
                          padding: '10px 32px', marginLeft: 24,
                          borderLeft: '1px dashed rgba(76, 215, 246, 0.4)',
                          display: 'flex', flexDirection: 'column', gap: 8,
                        }}>
                          {tx.splitItems.map((item, idx) => (
                            <div key={idx} style={{
                              display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                              fontSize: 'var(--text-body-sm)',
                              background: 'rgba(49, 53, 64, 0.4)',
                              padding: 8, borderRadius: 'var(--radius-md)',
                              border: '1px solid rgba(66, 71, 84, 0.3)',
                            }}>
                              <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                                <div>
                                  <span style={{ fontWeight: 500, color: 'var(--text-main)', fontSize: 'var(--text-label-md)' }}>{item.name}</span>
                                  <span style={{ fontSize: 11, color: 'var(--text-muted)', marginLeft: 8 }}>Assigned: {item.share}</span>
                                </div>
                              </div>
                              <div style={{ display: 'flex', alignItems: 'center', gap: 24 }}>
                                <div style={{ textAlign: 'right' }}>
                                  <span className="numeral" style={{ color: 'var(--text-muted)', fontSize: 'var(--text-label-sm)' }}>Gross: €{item.gross.toFixed(2)}</span>
                                  <span className="numeral" style={{ display: 'block', color: 'var(--secondary)', fontWeight: 600, fontSize: 'var(--text-label-md)' }}>Partner Share: {item.partnerShare}</span>
                                </div>
                                <span className="badge badge--secondary" style={{ fontSize: 10 }}>{item.status}</span>
                              </div>
                            </div>
                          ))}
                        </div>
                      </td>
                    </tr>
                  )}
                </>
              ))}
            </tbody>
          </table>
        </div>

        {/* Pagination Footer */}
        <div style={{
          padding: 'var(--space-sm) var(--space-md)',
          background: 'rgba(10, 14, 24, 0.7)',
          borderTop: '1px solid var(--border-outline-variant)',
          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
          fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)',
        }}>
          <span className="numeral">Showing 1 – 5 of 38 verified entries</span>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <button className="btn-secondary" disabled style={{ fontSize: 'var(--text-label-sm)', padding: '4px 8px' }}>Previous</button>
            <span className="numeral" style={{ padding: '2px 8px', borderRadius: 'var(--radius-xs)', background: 'var(--bg-surface-container-highest)', color: 'var(--secondary)' }}>1</span>
            <span className="numeral" style={{ padding: '2px 8px', cursor: 'pointer' }}>2</span>
            <span className="numeral" style={{ padding: '2px 8px', cursor: 'pointer' }}>3</span>
            <button className="btn-secondary" style={{ fontSize: 'var(--text-label-sm)', padding: '4px 8px' }}>Next</button>
          </div>
        </div>
      </section>
    </>
  );
}
