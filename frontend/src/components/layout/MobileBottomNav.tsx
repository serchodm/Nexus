import { NavLink, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { LayoutDashboard, Landmark, Users, TrendingUp, Brain } from 'lucide-react';

export function MobileBottomNav() {
  const location = useLocation();
  const { t } = useTranslation();

  const BOTTOM_ITEMS = [
    { to: '/', label: t('nav.dashboard'), icon: LayoutDashboard },
    { to: '/transactions', label: t('nav.transactions'), icon: Landmark },
    { to: '/shared-space', label: t('nav.sharedSpace'), icon: Users },
    { to: '/wealth', label: t('nav.wealth'), icon: TrendingUp },
    { to: '/aura', label: t('nav.aura'), icon: Brain, isAura: true },
  ];

  return (
    <nav className="mobile-bottom-nav">
      <ul className="mobile-bottom-nav__items">
        {BOTTOM_ITEMS.map(({ to, label, icon: Icon, isAura }) => {
          const isActive = location.pathname === to;
          return (
            <li key={to}>
              <NavLink
                to={to}
                className={`mobile-bottom-nav__item ${isActive ? 'mobile-bottom-nav__item--active' : ''}`}
                style={{ textDecoration: 'none', color: isActive ? (isAura ? 'var(--tertiary)' : 'var(--secondary)') : 'var(--text-muted)' }}
              >
                <Icon size={22} />
                <span>{label}</span>
              </NavLink>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}
