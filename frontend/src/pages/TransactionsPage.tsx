import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { CloudUpload, Search, Filter, Download, MoreHorizontal, Tag, ArrowUpDown, FileSpreadsheet, Users } from 'lucide-react';
import { ShareExpenseDrawer } from '../components/shared/ShareExpenseDrawer';

const CATEGORIES = ['All Categories', 'Groceries', 'Transport', 'Subscriptions', 'Dining', 'Utilities', 'Shopping', 'Health', 'Entertainment'];

const MOCK_TRANSACTIONS = Array.from({ length: 15 }, (_, i) => ({
  id: i + 1,
  date: `Oct ${25 - i}, ${9 + (i % 12)}:${String(15 + i * 3).padStart(2, '0')}`,
  merchant: ['Mercadona', 'Spotify', 'Netflix', 'Zara', 'Renfe', 'Amazon', 'Iberdrola', 'Uber', 'Apple', 'Farmacia Central', 'Carrefour', 'Vodafone', 'Glovo', 'MediaMarkt', 'El Corte Inglés'][i],
  category: ['Groceries', 'Subscriptions', 'Subscriptions', 'Shopping', 'Transport', 'Shopping', 'Utilities', 'Transport', 'Tech', 'Health', 'Groceries', 'Subscriptions', 'Dining', 'Electronics', 'Shopping'][i],
  account: i % 3 === 0 ? 'BBVA ••4521' : i % 3 === 1 ? 'ING ••8834' : 'Revolut ••9021',
  amount: -(Math.random() * 150 + 5).toFixed(2),
  isRecurring: i < 3,
  confidence: (85 + Math.random() * 15).toFixed(0),
}));

export function TransactionsPage() {
  const { t } = useTranslation();
  const [selectedCategory, setSelectedCategory] = useState('All Categories');
  const [searchQuery, setSearchQuery] = useState('');
  const [sharingTx, setSharingTx] = useState<{
    id: number;
    merchant: string;
    amount: number;
    date: string;
    category: string;
  } | null>(null);

  const filteredTransactions = MOCK_TRANSACTIONS.filter((tx) => {
    const matchesCategory = selectedCategory === 'All Categories' || tx.category === selectedCategory;
    const matchesQuery = !searchQuery || tx.merchant.toLowerCase().includes(searchQuery.toLowerCase());
    return matchesCategory && matchesQuery;
  });

  return (
    <>
      {/* Page Header */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 'var(--space-md)' }}>
        <div>
          <h2 style={{ margin: 0 }}>{t('transactions.title')}</h2>
          <p style={{ color: 'var(--text-muted)', fontSize: 'var(--text-body-sm)', marginTop: 4 }}>
            {t('transactions.subtitle')}
          </p>
        </div>
        <div style={{ display: 'flex', gap: 'var(--space-sm)' }}>
          <button className="btn-secondary">
            <FileSpreadsheet size={16} />
            {t('transactions.importRules')}
          </button>
          <button className="btn-primary">
            <CloudUpload size={16} />
            {t('transactions.uploadStatement')}
          </button>
        </div>
      </div>

      {/* Account Cards Summary */}
      <section style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 'var(--space-md)' }}>
        {[
          { bank: 'BBVA', last4: '4521', balance: '€18,420.40', type: 'Checking', color: 'var(--primary)' },
          { bank: 'ING', last4: '8834', balance: '€12,000.00', type: 'Savings', color: '#ff6b00' },
          { bank: 'Revolut', last4: '9021', balance: '€3,400.00', type: 'Multi-Currency', color: 'var(--secondary)' },
          { bank: 'MyInvestor', last4: '7110', balance: '€1,000.00', type: 'Investment', color: 'var(--success)' },
        ].map((acc) => (
          <div key={acc.last4} className="glass-panel" style={{
            padding: 'var(--space-md)', borderRadius: 'var(--radius-xl)',
            display: 'flex', flexDirection: 'column', gap: 8,
            borderLeft: `3px solid ${acc.color}`,
          }}>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <span style={{ fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)', textTransform: 'uppercase', fontWeight: 600, letterSpacing: '0.04em' }}>
                {acc.bank} ••{acc.last4}
              </span>
              <span className="badge badge--outline" style={{ fontSize: 10 }}>{acc.type}</span>
            </div>
            <span className="numeral" style={{ fontSize: 'var(--text-headline-md)', fontWeight: 700, color: 'var(--text-main)' }}>{acc.balance}</span>
            <span style={{ fontSize: 'var(--text-label-sm)', color: 'var(--text-dim)' }}>Synced • 2h ago</span>
          </div>
        ))}
      </section>

      {/* Filters & Table */}
      <section className="glass-panel" style={{ borderRadius: 'var(--radius-xl)', overflow: 'hidden' }}>
        {/* Toolbar */}
        <div style={{
          padding: 'var(--space-md)',
          borderBottom: '1px solid var(--border-outline-variant)',
          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
          gap: 'var(--space-md)', flexWrap: 'wrap',
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-sm)', flex: 1, maxWidth: 400 }}>
            <div style={{ position: 'relative', flex: 1 }}>
              <Search size={14} style={{ position: 'absolute', left: 10, top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
              <input
                className="input input--search"
                placeholder={t('transactions.searchMerchants')}
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                style={{ fontSize: 'var(--text-body-sm)' }}
              />
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <Filter size={14} style={{ color: 'var(--text-muted)' }} />
              <select
                value={selectedCategory}
                onChange={(e) => setSelectedCategory(e.target.value)}
                style={{
                  background: 'var(--bg-surface-container-high)',
                  border: '1px solid var(--border-outline-variant)',
                  color: 'var(--text-main)',
                  borderRadius: 'var(--radius-lg)',
                  padding: '6px 10px',
                  fontSize: 'var(--text-body-sm)',
                  cursor: 'pointer',
                }}
              >
                {CATEGORIES.map((cat) => (
                  <option key={cat} value={cat}>
                    {cat === 'All Categories' ? t('transactions.allCategories') : cat}
                  </option>
                ))}
              </select>
            </div>
            <button className="btn-secondary" style={{ gap: 6 }}>
              <ArrowUpDown size={14} />
              {t('transactions.sort')}
            </button>
            <button className="btn-ghost" title={t('transactions.export')}>
              <Download size={16} />
            </button>
          </div>
        </div>

        {/* Table */}
        <div style={{ overflowX: 'auto' }}>
          <table className="data-table">
            <thead>
              <tr>
                <th>{t('transactions.date')}</th>
                <th>{t('transactions.merchant')}</th>
                <th>{t('transactions.category')}</th>
                <th>{t('transactions.account')}</th>
                <th style={{ textAlign: 'right' }}>{t('transactions.amount')}</th>
                <th style={{ textAlign: 'center' }}>{t('transactions.confidence')}</th>
                <th style={{ textAlign: 'center' }}>{t('transactions.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {filteredTransactions.map((tx) => (
                <tr key={tx.id}>
                  <td className="numeral" style={{ color: 'var(--text-muted)' }}>{tx.date}</td>
                  <td>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                      <div className="avatar avatar--sm avatar--surface">{tx.merchant.slice(0, 2).toUpperCase()}</div>
                      <div>
                        <span style={{ fontWeight: 500, color: 'var(--text-main)' }}>{tx.merchant}</span>
                        {tx.isRecurring && <span className="badge badge--secondary" style={{ marginLeft: 8, fontSize: 10 }}>{t('transactions.recurring')}</span>}
                      </div>
                    </div>
                  </td>
                  <td>
                    <span className="badge badge--neutral">
                      <Tag size={10} /> {tx.category}
                    </span>
                  </td>
                  <td className="numeral" style={{ color: 'var(--text-muted)' }}>{tx.account}</td>
                  <td className="numeral" style={{ textAlign: 'right', fontWeight: 700, color: 'var(--danger)' }}>€{tx.amount}</td>
                  <td style={{ textAlign: 'center' }}>
                    <span className="numeral" style={{
                      fontSize: 'var(--text-label-sm)',
                      color: Number(tx.confidence) > 90 ? 'var(--success)' : 'var(--warning)',
                    }}>{tx.confidence}%</span>
                  </td>
                  <td style={{ textAlign: 'center' }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 6 }}>
                      <button
                        id={`share-btn-${tx.id}`}
                        className="btn-secondary share-expense-btn"
                        style={{
                          padding: '4px 8px',
                          fontSize: 11,
                          gap: 4,
                          borderRadius: 'var(--radius-md)',
                          background: 'rgba(76, 215, 246, 0.1)',
                          color: 'var(--secondary)',
                          border: '1px solid rgba(76, 215, 246, 0.3)',
                          cursor: 'pointer',
                        }}
                        onClick={() => setSharingTx({
                          id: tx.id,
                          merchant: tx.merchant,
                          amount: Math.abs(Number(tx.amount)),
                          date: tx.date,
                          category: tx.category,
                        })}
                        title={t('sharedSpace.shareAction')}
                      >
                        <Users size={12} />
                        <span>{t('sharedSpace.shareAction')}</span>
                      </button>
                      <button className="btn-ghost" style={{ padding: 4 }} title="Options">
                        <MoreHorizontal size={14} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      {/* Share Expense Drawer */}
      <ShareExpenseDrawer
        isOpen={!!sharingTx}
        onClose={() => setSharingTx(null)}
        transaction={sharingTx}
      />
    </>
  );
}
