import { NavLink, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  LayoutDashboard,
  Landmark,
  Users,
  TrendingUp,
  FlaskConical,
  Brain,
  Settings,
  ShieldCheck,
  HelpCircle,
} from 'lucide-react';

export function Sidebar() {
  const location = useLocation();
  const { t } = useTranslation();

  const NAV_ITEMS = [
    { to: '/', label: t('nav.dashboard'), icon: LayoutDashboard },
    { to: '/transactions', label: t('nav.transactions'), icon: Landmark },
    { to: '/shared-space', label: t('nav.sharedSpace'), icon: Users, badge: '2 Sync' },
    { to: '/wealth', label: t('nav.wealth'), icon: TrendingUp },
    { to: '/simulator', label: t('nav.simulator'), icon: FlaskConical },
    { to: '/aura', label: t('nav.aura'), icon: Brain, isAura: true },
    { to: '/settings', label: t('nav.settings'), icon: Settings },
  ];

  return (
    <aside className="app-shell__sidebar">
      {/* Top: Brand + Navigation */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
        {/* Brand Anchor */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-sm)', padding: 'var(--space-xs)' }}>
          <div style={{
            position: 'relative',
            width: 32, height: 32,
            borderRadius: 'var(--radius-md)',
            background: 'var(--bg-surface-container-highest)',
            border: '1px solid var(--border-outline-variant)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: 'var(--secondary)',
          }} className="cyan-glow">
            <svg width={20} height={20} viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 2L3.5 6.8V17.2L12 22L20.5 17.2V6.8L12 2ZM12 4.2L18.5 7.9V16.1L12 19.8L5.5 16.1V7.9L12 4.2ZM12 8.5L8 10.8V15.4L12 17.7L16 15.4V10.8L12 8.5Z" />
            </svg>
            <div style={{
              position: 'absolute', top: -4, right: -4,
              width: 8, height: 8,
              borderRadius: '50%',
              background: 'var(--secondary)',
            }} className="animate-pulse" />
          </div>
          <div>
            <span style={{
              fontFamily: 'var(--font-display)',
              fontSize: 'var(--text-headline-sm)',
              fontWeight: 600,
              color: 'var(--primary)',
              letterSpacing: '-0.01em',
              display: 'block',
              lineHeight: 1.2,
            }}>Nexus Financial OS</span>
            <span style={{
              fontSize: 'var(--text-label-sm)',
              fontWeight: 600,
              color: 'var(--text-muted)',
              letterSpacing: '0.04em',
              textTransform: 'uppercase',
              display: 'block',
            }}>Personal &amp; Shared</span>
          </div>
        </div>

        {/* Terminal Mode Badge */}
        <div style={{
          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
          padding: 'var(--space-xs) var(--space-sm)',
          borderRadius: 'var(--radius-md)',
          background: 'var(--bg-surface-container)',
          border: '1px solid var(--border-outline-variant)',
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-xs)' }}>
            <span style={{ width: 8, height: 8, borderRadius: '50%', background: 'var(--success)' }} />
            <span style={{ fontSize: 'var(--text-label-sm)', fontWeight: 500, color: 'var(--text-on-surface-variant)' }}>
              {t('nav.terminalMode')}
            </span>
          </div>
          <span style={{
            fontSize: 'var(--text-label-sm)', fontWeight: 500,
            padding: '1px 6px',
            borderRadius: 'var(--radius-xs)',
            background: 'var(--bg-surface-container-highest)',
            color: 'var(--secondary)',
            border: '1px solid rgba(76, 215, 246, 0.2)',
          }}>v1.0</span>
        </div>

        {/* Navigation Links */}
        <nav style={{ display: 'flex', flexDirection: 'column', gap: 4, marginTop: 'var(--space-sm)' }}>
          {NAV_ITEMS.map(({ to, label, icon: Icon, badge, isAura }) => {
            const isActive = location.pathname === to;
            return (
              <NavLink
                key={to}
                to={to}
                className={`nav-item ${isActive ? 'nav-item--active' : ''} ${isAura ? 'nav-item--aura' : ''}`}
                style={{ textDecoration: 'none' }}
              >
                <Icon size={18} style={{ opacity: isActive || isAura ? 1 : 0.6 }} />
                <span style={{ fontWeight: isActive ? 600 : 500, position: 'relative', zIndex: 1 }}>
                  {label}
                </span>
                {badge && (
                  <span style={{
                    marginLeft: 'auto',
                    fontSize: 'var(--text-label-sm)',
                    padding: '1px 6px',
                    borderRadius: 'var(--radius-xs)',
                    background: 'var(--bg-surface-container)',
                    color: 'var(--secondary)',
                  }}>{badge}</span>
                )}
                {isAura && (
                  <span style={{ marginLeft: 'auto', position: 'relative', display: 'flex', width: 8, height: 8 }}>
                    <span style={{
                      position: 'absolute', width: '100%', height: '100%',
                      borderRadius: '50%',
                      background: 'var(--tertiary-container)',
                      opacity: 0.75,
                      animation: 'ping 1.5s cubic-bezier(0, 0, 0.2, 1) infinite',
                    }} />
                    <span style={{
                      position: 'relative', width: 8, height: 8,
                      borderRadius: '50%',
                      background: 'var(--tertiary)',
                    }} />
                  </span>
                )}
                {isActive && !badge && !isAura && (
                  <span style={{
                    marginLeft: 'auto', width: 6, height: 6,
                    borderRadius: '50%',
                    background: 'var(--secondary)',
                    boxShadow: '0 0 8px var(--secondary)',
                  }} />
                )}
              </NavLink>
            );
          })}
        </nav>
      </div>

      {/* Bottom: Security Status */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-sm)', paddingTop: 'var(--space-md)', borderTop: '1px solid rgba(66, 71, 84, 0.5)' }}>
        <div className="glass-panel" style={{
          padding: 'var(--space-sm)', borderRadius: 'var(--radius-md)',
          display: 'flex', flexDirection: 'column', gap: 6,
          border: '1px solid var(--border-outline-variant)',
        }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <ShieldCheck size={14} style={{ color: 'var(--secondary)' }} />
              <span style={{ fontSize: 'var(--text-label-sm)', fontWeight: 600, color: 'var(--text-main)' }}>
                {t('nav.securityStatus')}
              </span>
            </div>
            <span className="numeral" style={{ fontSize: 'var(--text-label-sm)', color: 'var(--success)' }}>AES-256-GCM</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)' }}>
            <span>Zero XSS Storage</span>
            <span className="numeral" style={{ color: 'var(--text-main)' }}>HttpOnly + RAM</span>
          </div>
        </div>

        <div style={{
          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
          padding: '0 var(--space-xs)',
          fontSize: 'var(--text-label-sm)',
          color: 'var(--text-on-surface-variant)',
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
            <span style={{ width: 6, height: 6, borderRadius: '50%', background: 'var(--success)' }} />
            <span>Sync: <span className="numeral" style={{ color: 'var(--text-main)' }}>18ms</span></span>
          </div>
          <span style={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 4 }}>
            <HelpCircle size={12} />
            Support
          </span>
        </div>
      </div>
    </aside>
  );
}
