import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './lib/auth-store';
import { AppShell } from './components/layout/AppShell';
import { DashboardPage } from './pages/DashboardPage';
import { TransactionsPage } from './pages/TransactionsPage';
import { SharedSpacePage } from './pages/SharedSpacePage';
import { WealthPage } from './pages/WealthPage';
import { SimulatorPage } from './pages/SimulatorPage';
import { AuraPage } from './pages/AuraPage';
import { SettingsPage } from './pages/SettingsPage';
import { LoginPage } from './pages/LoginPage';

export default function App() {
  const { isInitialLoading } = useAuth();

  if (isInitialLoading) {
    return (
      <div style={{
        height: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'var(--bg-app)',
      }}>
        <div style={{ textAlign: 'center' }}>
          <div style={{
            width: 48, height: 48,
            borderRadius: 12,
            background: 'var(--bg-surface-container-highest)',
            border: '1px solid var(--border-outline-variant)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            margin: '0 auto 16px',
            color: 'var(--secondary)',
            boxShadow: 'var(--shadow-cyan)',
          }}>
            <NexusLogo size={28} />
          </div>
          <p style={{ color: 'var(--text-muted)', fontSize: 'var(--text-body-sm)' }}>
            Initializing Nexus...
          </p>
        </div>
      </div>
    );
  }

  // For now, bypass auth to show the full UI (demo mode)
  // In production, wrap routes with auth guard
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<AppShell />}>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/transactions" element={<TransactionsPage />} />
        <Route path="/shared-space" element={<SharedSpacePage />} />
        <Route path="/wealth" element={<WealthPage />} />
        <Route path="/simulator" element={<SimulatorPage />} />
        <Route path="/aura" element={<AuraPage />} />
        <Route path="/settings" element={<SettingsPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

function NexusLogo({ size = 20 }: { size?: number }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="currentColor">
      <path d="M12 2L3.5 6.8V17.2L12 22L20.5 17.2V6.8L12 2ZM12 4.2L18.5 7.9V16.1L12 19.8L5.5 16.1V7.9L12 4.2ZM12 8.5L8 10.8V15.4L12 17.7L16 15.4V10.8L12 8.5Z" />
    </svg>
  );
}
