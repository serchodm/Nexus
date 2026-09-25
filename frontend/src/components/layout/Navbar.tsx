import { Activity, Zap } from 'lucide-react';
import { useHealthCheck } from '../../hooks/useHealthCheck';

export function Navbar() {
  const { data: health, isSuccess, isError, isLoading } = useHealthCheck();

  return (
    <header
      style={{
        position: 'sticky',
        top: 0,
        zIndex: 50,
        backdropFilter: 'blur(16px)',
        WebkitBackdropFilter: 'blur(16px)',
        borderBottom: '1px solid var(--border-glass)',
        backgroundColor: 'hsla(224, 71%, 4%, 0.8)',
        padding: '0.9rem 2rem',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', gap: '0.8rem' }}>
        <div
          style={{
            width: '38px',
            height: '38px',
            borderRadius: 'var(--radius-md)',
            background: 'linear-gradient(135deg, var(--primary-500), var(--accent-cyan))',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            boxShadow: 'var(--shadow-glow)',
          }}
        >
          <Zap size={22} color="#ffffff" />
        </div>
        <div>
          <span
            style={{
              fontFamily: 'var(--font-display)',
              fontWeight: 800,
              fontSize: '1.25rem',
              letterSpacing: '-0.03em',
              background: 'linear-gradient(90deg, #ffffff, var(--text-muted))',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
            }}
          >
            NEXUS
          </span>
          <span
            style={{
              marginLeft: '0.45rem',
              fontSize: '0.72rem',
              fontWeight: 600,
              color: 'var(--accent-cyan)',
              textTransform: 'uppercase',
              letterSpacing: '0.08em',
              padding: '0.1rem 0.4rem',
              borderRadius: 'var(--radius-sm)',
              background: 'hsla(188, 94%, 53%, 0.1)',
            }}
          >
            v1.0
          </span>
        </div>
      </div>

      <div style={{ display: 'flex', alignItems: 'center', gap: '1.25rem' }}>
        {/* API Backend Health Indicator */}
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '0.6rem',
            padding: '0.35rem 0.85rem',
            borderRadius: 'var(--radius-full)',
            background: isSuccess
              ? 'var(--success-bg)'
              : isError
              ? 'var(--danger-bg)'
              : 'var(--bg-surface-elevated)',
            border: `1px solid ${
              isSuccess
                ? 'hsla(152, 69%, 45%, 0.3)'
                : isError
                ? 'hsla(354, 70%, 54%, 0.3)'
                : 'var(--border-subtle)'
            }`,
          }}
        >
          <span
            className={isSuccess ? 'badge-pulse' : undefined}
            style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              backgroundColor: isSuccess
                ? 'var(--success)'
                : isError
                ? 'var(--danger)'
                : 'var(--warning)',
            }}
          />
          <span style={{ fontSize: '0.8rem', fontWeight: 600, color: 'var(--text-main)' }}>
            {isLoading
              ? 'Conectando con Backend...'
              : isSuccess
              ? `Backend Operativo (${health?.status})`
              : 'API Desconectada'}
          </span>
        </div>

        <a
          href="http://localhost:5276/swagger"
          target="_blank"
          rel="noopener noreferrer"
          className="btn-secondary"
          style={{ padding: '0.45rem 0.95rem', fontSize: '0.82rem' }}
        >
          <Activity size={15} />
          Swagger UI
        </a>
      </div>
    </header>
  );
}
