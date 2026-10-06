import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  TrendingUp,
  Wallet,
  Building,
  Coins,
  Shield,
  Download,
  Plus,
  ArrowUpRight,
  ArrowDownRight,
  PieChart,
  BarChart3,
  Search,
  ExternalLink,
} from 'lucide-react';

interface AssetHolding {
  id: string;
  name: string;
  category: 'Equities' | 'Real Estate' | 'Cash Vaults' | 'Crypto' | 'Private Equity';
  institution: string;
  allocationPercent: number;
  currentValue: number;
  unrealizedGain: number;
  gainPercent: number;
  ownership: 'Personal' | 'Shared 50/50';
  isSynced: boolean;
}

const ASSET_HOLDINGS: AssetHolding[] = [
  {
    id: 'h-1',
    name: 'Vanguard FTSE All-World UCITS ETF (VWCE)',
    category: 'Equities',
    institution: 'MyInvestor',
    allocationPercent: 32.4,
    currentValue: 59840.0,
    unrealizedGain: 8420.5,
    gainPercent: 16.4,
    ownership: 'Personal',
    isSynced: true,
  },
  {
    id: 'h-2',
    name: 'Piso Gran Vía (Equity Valuation)',
    category: 'Real Estate',
    institution: 'Idealista / Tinsa API',
    allocationPercent: 32.5,
    currentValue: 60000.0,
    unrealizedGain: 12500.0,
    gainPercent: 26.3,
    ownership: 'Shared 50/50',
    isSynced: true,
  },
  {
    id: 'h-3',
    name: 'High-Yield Liquid Cash Reserve (3.75% APY)',
    category: 'Cash Vaults',
    institution: 'Trade Republic',
    allocationPercent: 18.9,
    currentValue: 34820.0,
    unrealizedGain: 820.0,
    gainPercent: 2.4,
    ownership: 'Personal',
    isSynced: true,
  },
  {
    id: 'h-4',
    name: 'iShares Core S&P 500 UCITS (SXR8)',
    category: 'Equities',
    institution: 'Degiro Core',
    allocationPercent: 10.8,
    currentValue: 19920.0,
    unrealizedGain: 3100.2,
    gainPercent: 18.4,
    ownership: 'Personal',
    isSynced: true,
  },
  {
    id: 'h-5',
    name: 'Bitcoin (Cold Storage Vault)',
    category: 'Crypto',
    institution: 'Hardware Ledger',
    allocationPercent: 5.4,
    currentValue: 9940.0,
    unrealizedGain: 4210.0,
    gainPercent: 73.5,
    ownership: 'Personal',
    isSynced: false,
  },
];

export function WealthPage() {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState<'live' | 'yoy'>('live');
  const [categoryFilter, setCategoryFilter] = useState<string>('All');
  const [searchQuery, setSearchQuery] = useState<string>('');

  const filteredAssets = ASSET_HOLDINGS.filter((asset) => {
    const matchesCategory = categoryFilter === 'All' || asset.category === categoryFilter;
    const matchesSearch =
      asset.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      asset.institution.toLowerCase().includes(searchQuery.toLowerCase());
    return matchesCategory && matchesSearch;
  });

  const totalValue = ASSET_HOLDINGS.reduce((acc, a) => acc + a.currentValue, 0);
  const totalGain = ASSET_HOLDINGS.reduce((acc, a) => acc + a.unrealizedGain, 0);
  const overallYieldPercent = ((totalGain / (totalValue - totalGain)) * 100).toFixed(1);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-lg)' }}>
      {/* Header Title & Global Controls */}
      <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', flexWrap: 'wrap', gap: 'var(--space-md)' }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <h2 style={{ margin: 0, fontSize: 'var(--text-headline-lg)', fontWeight: 700, letterSpacing: '-0.02em' }}>
              {t('wealth.title')}
            </h2>
            <span className="badge badge--secondary" style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
              <Shield size={12} /> Institutional Grade
            </span>
          </div>
          <p style={{ color: 'var(--text-muted)', fontSize: 'var(--text-body-md)', marginTop: 4 }}>
            {t('wealth.subtitle')}
          </p>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-sm)' }}>
          <div style={{
            display: 'flex',
            background: 'var(--bg-surface-container-low)',
            padding: 3,
            borderRadius: 'var(--radius-lg)',
            border: '1px solid var(--border-outline-variant)'
          }}>
            <button
              onClick={() => setActiveTab('live')}
              style={{
                background: activeTab === 'live' ? 'var(--bg-surface-container-highest)' : 'transparent',
                color: activeTab === 'live' ? 'var(--secondary)' : 'var(--text-muted)',
                border: 'none',
                padding: '6px 14px',
                borderRadius: 'var(--radius-md)',
                fontSize: 'var(--text-label-md)',
                fontWeight: 600,
                cursor: 'pointer',
                transition: 'all 0.15s ease',
              }}
            >
              {t('wealth.liveConsolidation')}
            </button>
            <button
              onClick={() => setActiveTab('yoy')}
              style={{
                background: activeTab === 'yoy' ? 'var(--bg-surface-container-highest)' : 'transparent',
                color: activeTab === 'yoy' ? 'var(--secondary)' : 'var(--text-muted)',
                border: 'none',
                padding: '6px 14px',
                borderRadius: 'var(--radius-md)',
                fontSize: 'var(--text-label-md)',
                fontWeight: 600,
                cursor: 'pointer',
                transition: 'all 0.15s ease',
              }}
            >
              {t('wealth.historicalYoY')}
            </button>
          </div>

          <button className="btn-secondary" style={{ gap: 6 }}>
            <Download size={15} />
            {t('wealth.exportReport')}
          </button>
          <button className="btn-primary" style={{ gap: 6 }}>
            <Plus size={15} />
            {t('wealth.addAsset')}
          </button>
        </div>
      </div>

      {/* 5-Card Top Summary Telemetry */}
      <section style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(210px, 1fr))',
        gap: 'var(--space-md)',
      }}>
        {/* Card 1: Total Net Worth */}
        <div className="glass-panel" style={{
          padding: 'var(--space-md)',
          borderRadius: 'var(--radius-xl)',
          position: 'relative',
          overflow: 'hidden',
          borderLeft: '3px solid var(--secondary)',
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
            <span style={{ fontSize: 'var(--text-label-md)', color: 'var(--text-muted)', fontWeight: 500 }}>
              {t('wealth.totalNetWorth')}
            </span>
            <div style={{ color: 'var(--secondary)', padding: 6, borderRadius: 8, background: 'rgba(76, 215, 246, 0.1)' }}>
              <Wallet size={18} />
            </div>
          </div>
          <div className="numeral" style={{ fontSize: 'var(--text-headline-lg)', fontWeight: 700, color: 'var(--text-main)', letterSpacing: '-0.02em' }}>
            €184,520.00
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginTop: 8 }}>
            <span className="badge badge--success" style={{ display: 'flex', alignItems: 'center', gap: 2, padding: '2px 6px', fontSize: 11 }}>
              <ArrowUpRight size={12} /> +3.4%
            </span>
            <span style={{ fontSize: 'var(--text-body-sm)', color: 'var(--text-dim)' }}>vs last month</span>
          </div>
        </div>

        {/* Card 2: Liquid Assets */}
        <div className="glass-panel" style={{
          padding: 'var(--space-md)',
          borderRadius: 'var(--radius-xl)',
          borderLeft: '3px solid var(--primary)',
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
            <span style={{ fontSize: 'var(--text-label-md)', color: 'var(--text-muted)', fontWeight: 500 }}>
              {t('wealth.liquidAssets')}
            </span>
            <div style={{ color: 'var(--primary)', padding: 6, borderRadius: 8, background: 'rgba(173, 198, 255, 0.1)' }}>
              <Coins size={18} />
            </div>
          </div>
          <div className="numeral" style={{ fontSize: 'var(--text-headline-lg)', fontWeight: 700, color: 'var(--text-main)' }}>
            €34,820.00
          </div>
          <div style={{ marginTop: 8, fontSize: 'var(--text-body-sm)', color: 'var(--text-dim)' }}>
            Cash &amp; 3.75% High-Yield Vaults
          </div>
        </div>

        {/* Card 3: Invested Capital */}
        <div className="glass-panel" style={{
          padding: 'var(--space-md)',
          borderRadius: 'var(--radius-xl)',
          borderLeft: '3px solid var(--tertiary)',
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
            <span style={{ fontSize: 'var(--text-label-md)', color: 'var(--text-muted)', fontWeight: 500 }}>
              {t('wealth.investedCapital')}
            </span>
            <div style={{ color: 'var(--tertiary)', padding: 6, borderRadius: 8, background: 'rgba(208, 188, 255, 0.1)' }}>
              <TrendingUp size={18} />
            </div>
          </div>
          <div className="numeral" style={{ fontSize: 'var(--text-headline-lg)', fontWeight: 700, color: 'var(--text-main)' }}>
            €89,700.00
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginTop: 8 }}>
            <span className="badge badge--success" style={{ display: 'flex', alignItems: 'center', gap: 2, padding: '2px 6px', fontSize: 11 }}>
              <ArrowUpRight size={12} /> +5.8%
            </span>
            <span style={{ fontSize: 'var(--text-body-sm)', color: 'var(--text-dim)' }}>Equities &amp; Crypto</span>
          </div>
        </div>

        {/* Card 4: Real Estate Equity */}
        <div className="glass-panel" style={{
          padding: 'var(--space-md)',
          borderRadius: 'var(--radius-xl)',
          borderLeft: '3px solid #22c55e',
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
            <span style={{ fontSize: 'var(--text-label-md)', color: 'var(--text-muted)', fontWeight: 500 }}>
              {t('wealth.realEstateEquity')}
            </span>
            <div style={{ color: '#22c55e', padding: 6, borderRadius: 8, background: 'rgba(34, 197, 94, 0.1)' }}>
              <Building size={18} />
            </div>
          </div>
          <div className="numeral" style={{ fontSize: 'var(--text-headline-lg)', fontWeight: 700, color: 'var(--text-main)' }}>
            €60,000.00
          </div>
          <div style={{ marginTop: 8, fontSize: 'var(--text-body-sm)', color: 'var(--text-dim)' }}>
            50% Shared Ownership (Gran Vía)
          </div>
        </div>

        {/* Card 5: Total Liabilities */}
        <div className="glass-panel" style={{
          padding: 'var(--space-md)',
          borderRadius: 'var(--radius-xl)',
          borderLeft: '3px solid var(--danger)',
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
            <span style={{ fontSize: 'var(--text-label-md)', color: 'var(--text-muted)', fontWeight: 500 }}>
              {t('wealth.liabilities')}
            </span>
            <div style={{ color: 'var(--danger)', padding: 6, borderRadius: 8, background: 'rgba(255, 180, 171, 0.1)' }}>
              <ArrowDownRight size={18} />
            </div>
          </div>
          <div className="numeral" style={{ fontSize: 'var(--text-headline-lg)', fontWeight: 700, color: 'var(--text-main)' }}>
            -€120,400.00
          </div>
          <div style={{ marginTop: 8, fontSize: 'var(--text-body-sm)', color: 'var(--text-dim)' }}>
            Fixed 2.15% Euribor-free Mortgage
          </div>
        </div>
      </section>

      {/* Asset Allocation Multi-Segment Visualizer */}
      <section className="glass-panel" style={{
        padding: 'var(--space-lg)',
        borderRadius: 'var(--radius-xl)',
        display: 'flex',
        flexDirection: 'column',
        gap: 'var(--space-md)',
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 8 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <PieChart size={18} style={{ color: 'var(--secondary)' }} />
            <h3 style={{ margin: 0, fontSize: 'var(--text-headline-sm)' }}>{t('wealth.strategicAllocation')}</h3>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 16, fontSize: 'var(--text-label-sm)' }}>
            <span style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <span style={{ width: 10, height: 10, borderRadius: 2, background: 'var(--tertiary)' }} />
              {t('wealth.equities')} (43.2%)
            </span>
            <span style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <span style={{ width: 10, height: 10, borderRadius: 2, background: '#22c55e' }} />
              {t('wealth.realEstate')} (32.5%)
            </span>
            <span style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <span style={{ width: 10, height: 10, borderRadius: 2, background: 'var(--primary)' }} />
              {t('wealth.cashVaults')} (18.9%)
            </span>
            <span style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <span style={{ width: 10, height: 10, borderRadius: 2, background: 'var(--secondary)' }} />
              {t('wealth.crypto')} (5.4%)
            </span>
          </div>
        </div>

        {/* Allocation Progress Bar */}
        <div style={{
          height: 14,
          borderRadius: 7,
          background: 'var(--bg-surface-container-high)',
          display: 'flex',
          overflow: 'hidden',
          boxShadow: 'inset 0 1px 3px rgba(0,0,0,0.4)',
        }}>
          <div style={{ width: '43.2%', background: 'var(--tertiary)', transition: 'width 0.4s ease' }} title="Equities 43.2%" />
          <div style={{ width: '32.5%', background: '#22c55e', transition: 'width 0.4s ease' }} title="Real Estate 32.5%" />
          <div style={{ width: '18.9%', background: 'var(--primary)', transition: 'width 0.4s ease' }} title="Cash Vaults 18.9%" />
          <div style={{ width: '5.4%', background: 'var(--secondary)', transition: 'width 0.4s ease' }} title="Crypto 5.4%" />
        </div>

        <div style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          fontSize: 'var(--text-body-sm)',
          color: 'var(--text-muted)',
          paddingTop: 6,
        }}>
          <span>{t('wealth.totalAssetValue')}: <strong style={{ color: 'var(--text-main)' }}>€{totalValue.toLocaleString('es-ES', { minimumFractionDigits: 2 })}</strong></span>
          <span>{t('wealth.unrealizedGain')}: <strong style={{ color: 'var(--success)' }}>+€{totalGain.toLocaleString('es-ES', { minimumFractionDigits: 2 })} (+{overallYieldPercent}%)</strong></span>
        </div>
      </section>

      {/* Asset Holdings Ledger Table */}
      <section className="glass-panel" style={{ borderRadius: 'var(--radius-xl)', overflow: 'hidden' }}>
        {/* Table Toolbar */}
        <div style={{
          padding: 'var(--space-md)',
          borderBottom: '1px solid var(--border-outline-variant)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          flexWrap: 'wrap',
          gap: 'var(--space-md)',
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, flex: 1, maxWidth: 420 }}>
            <div style={{ position: 'relative', width: '100%' }}>
              <Search size={14} style={{ position: 'absolute', left: 10, top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
              <input
                className="input input--search"
                placeholder={t('wealth.searchHoldings')}
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                style={{ fontSize: 'var(--text-body-sm)' }}
              />
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
            <div style={{ display: 'flex', gap: 4 }}>
              {['All', 'Equities', 'Real Estate', 'Cash Vaults', 'Crypto'].map((cat) => (
                <button
                  key={cat}
                  onClick={() => setCategoryFilter(cat)}
                  style={{
                    background: categoryFilter === cat ? 'var(--bg-surface-container-highest)' : 'transparent',
                    color: categoryFilter === cat ? 'var(--secondary)' : 'var(--text-muted)',
                    border: '1px solid',
                    borderColor: categoryFilter === cat ? 'var(--secondary)' : 'var(--border-outline-variant)',
                    padding: '4px 10px',
                    borderRadius: 'var(--radius-md)',
                    fontSize: 'var(--text-label-sm)',
                    cursor: 'pointer',
                    transition: 'all 0.15s ease',
                  }}
                >
                  {cat}
                </button>
              ))}
            </div>

            <button className="btn-secondary" style={{ gap: 6 }}>
              <BarChart3 size={14} />
              {t('wealth.rebalance')}
            </button>
          </div>
        </div>

        {/* Holdings Table */}
        <div style={{ overflowX: 'auto' }}>
          <table className="data-table">
            <thead>
              <tr>
                <th>{t('wealth.holding')}</th>
                <th>{t('wealth.assetClass')}</th>
                <th>{t('wealth.custodian')}</th>
                <th>{t('wealth.ownership')}</th>
                <th style={{ textAlign: 'right' }}>{t('wealth.currentValue')}</th>
                <th style={{ textAlign: 'right' }}>{t('wealth.pnl')}</th>
                <th style={{ textAlign: 'center' }}>{t('wealth.syncStatus')}</th>
              </tr>
            </thead>
            <tbody>
              {filteredAssets.map((asset) => (
                <tr key={asset.id}>
                  <td>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                      <div style={{
                        width: 34,
                        height: 34,
                        borderRadius: 8,
                        background: 'var(--bg-surface-container-high)',
                        border: '1px solid var(--border-outline-variant)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        color: asset.category === 'Equities' ? 'var(--tertiary)' : asset.category === 'Real Estate' ? '#22c55e' : 'var(--secondary)',
                      }}>
                        {asset.category === 'Real Estate' ? <Building size={16} /> : <TrendingUp size={16} />}
                      </div>
                      <div>
                        <div style={{ fontWeight: 600, color: 'var(--text-main)', display: 'flex', alignItems: 'center', gap: 6 }}>
                          {asset.name}
                          <ExternalLink size={12} style={{ color: 'var(--text-dim)', cursor: 'pointer' }} />
                        </div>
                        <span style={{ fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)' }}>
                          Weight: {asset.allocationPercent}%
                        </span>
                      </div>
                    </div>
                  </td>
                  <td>
                    <span className="badge badge--neutral">
                      {asset.category}
                    </span>
                  </td>
                  <td>
                    <span style={{ color: 'var(--text-main)', fontSize: 'var(--text-body-sm)' }}>
                      {asset.institution}
                    </span>
                  </td>
                  <td>
                    <span className={`badge ${asset.ownership === 'Shared 50/50' ? 'badge--secondary' : 'badge--outline'}`}>
                      {asset.ownership}
                    </span>
                  </td>
                  <td className="numeral" style={{ textAlign: 'right', fontWeight: 700, color: 'var(--text-main)', fontSize: 'var(--text-body-md)' }}>
                    €{asset.currentValue.toLocaleString('es-ES', { minimumFractionDigits: 2 })}
                  </td>
                  <td className="numeral" style={{ textAlign: 'right' }}>
                    <div style={{ fontWeight: 700, color: asset.unrealizedGain >= 0 ? 'var(--success)' : 'var(--danger)' }}>
                      {asset.unrealizedGain >= 0 ? '+' : ''}€{asset.unrealizedGain.toLocaleString('es-ES', { minimumFractionDigits: 2 })}
                    </div>
                    <span style={{ fontSize: 'var(--text-label-sm)', color: asset.gainPercent >= 0 ? 'var(--success)' : 'var(--danger)' }}>
                      ({asset.gainPercent >= 0 ? '+' : ''}{asset.gainPercent}%)
                    </span>
                  </td>
                  <td style={{ textAlign: 'center' }}>
                    {asset.isSynced ? (
                      <span className="badge badge--success" style={{ fontSize: 10, padding: '2px 6px' }}>
                        Live API
                      </span>
                    ) : (
                      <span className="badge badge--outline" style={{ fontSize: 10, padding: '2px 6px' }}>
                        Manual
                      </span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}
