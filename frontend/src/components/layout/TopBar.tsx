import { useState } from 'react';
import { Search, Wallet, Bell, Sparkles, SlidersHorizontal, CloudUpload, ChevronDown } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { LanguageSwitcher } from '../common/LanguageSwitcher';
import { useSharedSpace } from '../../hooks/useSharedSpace';
import { SettleUpModal } from '../shared/SettleUpModal';

export function TopBar() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { summary } = useSharedSpace();
  const [isSettleOpen, setIsSettleOpen] = useState(false);

  return (
    <header className="app-shell__header">
      {/* Left: Account Switcher + Search */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-md)', flex: 1, maxWidth: 560 }}>
        {/* Account Switcher Pill */}
        <button style={{
          display: 'flex', alignItems: 'center', gap: 8,
          padding: 'var(--space-xs) var(--space-sm)',
          borderRadius: 'var(--radius-md)',
          background: 'var(--bg-surface-container)',
          border: '1px solid var(--border-outline-variant)',
          color: 'var(--text-main)',
          cursor: 'pointer',
          transition: 'border-color var(--transition-fast)',
          whiteSpace: 'nowrap',
        }}>
          <Wallet size={16} style={{ color: 'var(--secondary)' }} />
          <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-start' }}>
            <span style={{
              fontSize: 'var(--text-label-sm)',
              fontWeight: 600,
              display: 'flex', alignItems: 'center', gap: 4,
            }}>
              {t('topbar.consolidated')}
              <ChevronDown size={12} style={{ color: 'var(--text-muted)' }} />
            </span>
          </div>
        </button>

        {/* Search */}
        <div style={{ position: 'relative', flex: 1 }}>
          <Search size={16} style={{
            position: 'absolute', left: 12, top: '50%', transform: 'translateY(-50%)',
            color: 'var(--text-muted)',
          }} />
          <input
            className="input input--search"
            placeholder={t('topbar.searchPlaceholder')}
            type="text"
            style={{ paddingRight: '2.5rem' }}
          />
          <span style={{
            position: 'absolute', right: 10, top: '50%', transform: 'translateY(-50%)',
            fontSize: 10, background: 'var(--bg-surface-container-high)',
            color: 'var(--text-muted)',
            padding: '2px 6px', borderRadius: 'var(--radius-xs)',
            border: '1px solid var(--border-outline-variant)',
            fontFamily: 'var(--font-mono)',
          }}>⌘K</span>
        </div>
      </div>

      {/* Right: Couple Widget + Language + Actions + Profile */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-md)' }}>
        {/* Couple Balance Mini-Widget */}
        <div style={{
          display: 'flex', alignItems: 'center', gap: 8,
          padding: 'var(--space-xs) var(--space-sm)',
          borderRadius: 'var(--radius-md)',
          background: 'var(--bg-surface-container-low)',
          border: '1px solid var(--border-outline-variant)',
        }}>
          <div style={{ display: 'flex' }}>
            <div className="avatar avatar--sm avatar--primary" style={{ marginRight: -6, zIndex: 2 }}>OP</div>
            <div className="avatar avatar--sm avatar--secondary">PT</div>
          </div>
          <div style={{ textAlign: 'left', lineHeight: 1.2 }}>
            <span style={{ fontSize: 10, color: 'var(--text-muted)', textTransform: 'uppercase', fontWeight: 500, display: 'block' }}>
              {t('dashboard.fairShare')}
            </span>
            <span className="numeral" style={{ fontSize: 'var(--text-label-sm)', fontWeight: 600, color: summary.netBalance !== 0 ? 'var(--secondary)' : 'var(--text-muted)' }}>
              {summary.netBalance > 0
                ? `${t('sharedSpace.partnerOwesYou')} €${summary.netBalance.toFixed(2)}`
                : summary.netBalance < 0
                ? `${t('dashboard.owedByYou')} €${Math.abs(summary.netBalance).toFixed(2)}`
                : t('sharedSpace.settledBalanceAllGood')}
            </span>
          </div>
          <button className="btn-secondary" style={{
            marginLeft: 4, fontSize: 11, padding: '2px 8px',
            background: 'var(--secondary-bg)',
            color: 'var(--secondary)',
            border: '1px solid var(--secondary-border)',
            cursor: 'pointer',
          }}
            onClick={() => setIsSettleOpen(true)}
          >
            {t('topbar.settle')}
          </button>
        </div>

        {/* Partner Synced */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)' }}>
          <span style={{ width: 8, height: 8, borderRadius: '50%', background: 'var(--success)' }} />
          <span style={{ color: 'var(--text-on-surface-variant)' }}>{t('topbar.partnerSynced')}</span>
        </div>

        {/* Upload Statement CTA */}
        <button className="btn-primary" onClick={() => navigate('/transactions')}>
          <CloudUpload size={16} />
          <span>{t('topbar.uploadStatement')}</span>
        </button>

        {/* Global Multi-Language Switcher */}
        <LanguageSwitcher />

        {/* Tool Icons */}
        <div style={{
          display: 'flex', alignItems: 'center', gap: 4,
          borderLeft: '1px solid var(--border-outline-variant)',
          paddingLeft: 'var(--space-sm)',
          color: 'var(--text-muted)',
        }}>
          <button className="btn-ghost" style={{ position: 'relative' }} title={t('topbar.notifications')}>
            <Bell size={18} />
            <span style={{
              position: 'absolute', top: 4, right: 4,
              width: 6, height: 6,
              borderRadius: '50%',
              background: 'var(--secondary)',
            }} />
          </button>
          <button className="btn-ghost" style={{ color: 'var(--tertiary)' }}
            onClick={() => navigate('/aura')}
            title="Nexus Aura AI"
          >
            <Sparkles size={18} />
          </button>
          <button className="btn-ghost" onClick={() => navigate('/settings')} title={t('nav.settings')}>
            <SlidersHorizontal size={18} />
          </button>
        </div>

        {/* Profile Chip */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 8, paddingLeft: 'var(--space-xs)', cursor: 'pointer' }} onClick={() => navigate('/settings')}>
          <div className="avatar avatar--md avatar--secondary cyan-glow" style={{ fontSize: 12 }}>
            NX
          </div>
        </div>
      </div>

      {/* Settle Up Modal */}
      <SettleUpModal isOpen={isSettleOpen} onClose={() => setIsSettleOpen(false)} />
    </header>
  );
}
