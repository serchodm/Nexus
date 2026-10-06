import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Shield, Lock, ArrowRight, Fingerprint, Sparkles } from 'lucide-react';
import { authStore } from '../lib/auth-store';
import { LanguageSwitcher } from '../components/common/LanguageSwitcher';

export function LoginPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [email, setEmail] = useState('executive@nexus-os.internal');
  const [password, setPassword] = useState('••••••••••••');
  const [isLoading, setIsLoading] = useState(false);

  const handleLogin = (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    setIsLoading(true);

    setTimeout(() => {
      authStore.setSession('mock-jwt-token-nexus-2026', {
        id: 'usr-1',
        email: email || 'executive@nexus-os.internal',
        fullName: 'Sergio & Elena',
        role: 'OWNER',
        isMfaEnabled: true,
      });
      setIsLoading(false);
      navigate('/');
    }, 400);
  };

  return (
    <div style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      background: 'radial-gradient(circle at 50% 20%, #171b26 0%, #0a0e18 100%)',
      padding: 'var(--space-md)',
      position: 'relative',
    }}>
      {/* Background Neon Glow Orbs */}
      <div style={{
        position: 'absolute',
        top: '20%',
        left: '50%',
        transform: 'translate(-50%, -50%)',
        width: 400,
        height: 400,
        borderRadius: '50%',
        background: 'radial-gradient(circle, rgba(76, 215, 246, 0.08) 0%, transparent 70%)',
        pointerEvents: 'none',
      }} />

      {/* Top Language Switcher */}
      <div style={{ position: 'absolute', top: 20, right: 24, zIndex: 10 }}>
        <LanguageSwitcher />
      </div>

      <div className="glass-panel" style={{
        width: '100%',
        maxWidth: 440,
        padding: 'var(--space-xl)',
        borderRadius: 'var(--radius-xl)',
        display: 'flex',
        flexDirection: 'column',
        gap: 'var(--space-lg)',
        border: '1px solid rgba(76, 215, 246, 0.25)',
        boxShadow: '0 24px 60px rgba(0, 0, 0, 0.6), 0 0 30px rgba(76, 215, 246, 0.1)',
        position: 'relative',
        zIndex: 1,
      }}>
        {/* Brand Icon & Heading */}
        <div style={{ textAlign: 'center', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 12 }}>
          <div style={{
            width: 56,
            height: 56,
            borderRadius: 14,
            background: 'linear-gradient(135deg, rgba(76, 215, 246, 0.2), rgba(173, 198, 255, 0.1))',
            border: '1px solid rgba(76, 215, 246, 0.4)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'var(--secondary)',
            boxShadow: '0 0 20px rgba(76, 215, 246, 0.3)',
          }}>
            <Shield size={30} />
          </div>

          <div>
            <h1 style={{ margin: 0, fontSize: 'var(--text-headline-md)', fontWeight: 700, letterSpacing: '-0.02em', color: 'var(--text-main)' }}>
              {t('login.title')}
            </h1>
            <p style={{ margin: '4px 0 0', fontSize: 'var(--text-label-md)', color: 'var(--text-muted)' }}>
              {t('login.subtitle')}
            </p>
          </div>
        </div>

        {/* 1-Click Sandbox Demo Quick-Launch */}
        <button
          onClick={() => handleLogin()}
          style={{
            background: 'linear-gradient(135deg, rgba(76, 215, 246, 0.15), rgba(173, 198, 255, 0.1))',
            border: '1px solid rgba(76, 215, 246, 0.5)',
            color: 'var(--secondary)',
            padding: '12px 16px',
            borderRadius: 'var(--radius-lg)',
            fontWeight: 600,
            fontSize: 'var(--text-body-sm)',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            transition: 'all 0.2s ease',
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.boxShadow = '0 0 20px rgba(76, 215, 246, 0.3)';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.boxShadow = 'none';
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Sparkles size={18} />
            <div style={{ textAlign: 'left' }}>
              <div style={{ color: 'var(--text-main)' }}>{t('login.sandboxDemo')}</div>
              <div style={{ fontSize: 11, color: 'var(--text-muted)' }}>{t('login.instantDemoSub')}</div>
            </div>
          </div>
          <ArrowRight size={16} />
        </button>

        <div style={{ display: 'flex', alignItems: 'center', gap: 10, color: 'var(--text-dim)', fontSize: 'var(--text-label-sm)' }}>
          <div style={{ flex: 1, height: 1, background: 'var(--border-outline-variant)' }} />
          <span>{t('login.orManual')}</span>
          <div style={{ flex: 1, height: 1, background: 'var(--border-outline-variant)' }} />
        </div>

        {/* Form */}
        <form onSubmit={handleLogin} style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
          <div>
            <label style={{ display: 'block', fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)', marginBottom: 6, fontWeight: 500 }}>
              {t('login.emailLabel')}
            </label>
            <input
              className="input"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="operator@nexus-os.internal"
              style={{ width: '100%', padding: '10px 14px', borderRadius: 'var(--radius-lg)' }}
            />
          </div>

          <div>
            <label style={{ display: 'block', fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)', marginBottom: 6, fontWeight: 500 }}>
              {t('login.passkeyLabel')}
            </label>
            <input
              className="input"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••••••"
              style={{ width: '100%', padding: '10px 14px', borderRadius: 'var(--radius-lg)' }}
            />
          </div>

          <button
            type="submit"
            className="btn-primary"
            disabled={isLoading}
            style={{ width: '100%', padding: '12px', borderRadius: 'var(--radius-lg)', marginTop: 8 }}
          >
            {isLoading ? t('login.verifying') : t('login.authenticate')}
          </button>
        </form>

        {/* Biometric Passkey Alternative */}
        <button
          onClick={() => handleLogin()}
          className="btn-secondary"
          style={{ width: '100%', padding: '10px', borderRadius: 'var(--radius-lg)', gap: 8 }}
        >
          <Fingerprint size={18} style={{ color: 'var(--secondary)' }} />
          <span>{t('login.biometric')}</span>
        </button>

        {/* Security Footer */}
        <div style={{
          textAlign: 'center',
          fontSize: 11,
          color: 'var(--text-dim)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          gap: 6,
          paddingTop: 8,
          borderTop: '1px solid var(--border-outline-variant)',
        }}>
          <Lock size={12} style={{ color: 'var(--secondary)' }} />
          <span>{t('login.encryptedSession')}</span>
        </div>
      </div>
    </div>
  );
}
