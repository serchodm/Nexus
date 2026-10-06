import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { CheckCircle2, AlertTriangle, Loader2, Landmark, ArrowRight } from 'lucide-react';
import { useBanking } from '../hooks/useBanking';

export function BankCallbackPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { completeCallback } = useBanking();

  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    const code = searchParams.get('code') || 'mock_code';
    const sessionId = searchParams.get('sessionId') || searchParams.get('session_id') || `eb_sess_${Date.now()}`;

    let isMounted = true;

    async function processCallback() {
      try {
        await completeCallback({ code, sessionId });
        if (isMounted) {
          setStatus('success');
        }
      } catch (err: unknown) {
        if (isMounted) {
          console.error('Error processing banking callback:', err);
          setStatus('error');
          setErrorMessage(err instanceof Error ? err.message : 'Error desconocido al autorizar');
        }
      }
    }

    processCallback();

    return () => {
      isMounted = false;
    };
  }, [searchParams, completeCallback]);

  return (
    <div
      style={{
        minHeight: '70vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 24,
      }}
    >
      <div
        className="glass-panel"
        style={{
          width: '100%',
          maxWidth: 480,
          borderRadius: 'var(--radius-2xl)',
          padding: 36,
          textAlign: 'center',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: 20,
          background: 'var(--bg-surface-container-highest)',
          border: '1px solid var(--border-outline-variant)',
          boxShadow: 'var(--shadow-xl)',
        }}
      >
        <div
          style={{
            width: 64,
            height: 64,
            borderRadius: 'var(--radius-xl)',
            background:
              status === 'success'
                ? 'rgba(16, 185, 129, 0.15)'
                : status === 'error'
                ? 'rgba(239, 68, 68, 0.15)'
                : 'rgba(0, 229, 255, 0.15)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color:
              status === 'success'
                ? '#10b981'
                : status === 'error'
                ? '#ef4444'
                : 'var(--secondary)',
          }}
        >
          {status === 'loading' && <Loader2 size={32} className="animate-spin" />}
          {status === 'success' && <CheckCircle2 size={34} />}
          {status === 'error' && <AlertTriangle size={34} />}
        </div>

        <div>
          <h2 style={{ margin: '0 0 8px', fontSize: 'var(--text-headline-md)', fontWeight: 700 }}>
            {status === 'loading' && t('banking.callbackProcessingTitle', 'Vinculando Entidad Bancaria...')}
            {status === 'success' && t('banking.callbackSuccessTitle', '¡Banco Conectado con Éxito!')}
            {status === 'error' && t('banking.callbackErrorTitle', 'No se pudo completar la conexión')}
          </h2>

          <p style={{ margin: 0, color: 'var(--text-muted)', fontSize: 14, lineHeight: 1.5 }}>
            {status === 'loading' &&
              t(
                'banking.callbackProcessingText',
                'Estamos verificando la autorización SCA y sincronizando tus cuentas de forma segura.'
              )}
            {status === 'success' &&
              t(
                'banking.callbackSuccessText',
                'Tu consentimiento de 180 días ha sido activado. Las cuentas y saldos ya están disponibles en Nexus.'
              )}
            {status === 'error' &&
              (errorMessage ||
                t(
                  'banking.callbackErrorText',
                  'La sesión bancaria ha expirado o fue cancelada en la pasarela del banco.'
                ))}
          </p>
        </div>

        {status === 'success' && (
          <div style={{ display: 'flex', gap: 12, marginTop: 12, width: '100%' }}>
            <button
              className="btn-primary"
              onClick={() => navigate('/settings')}
              style={{ flex: 1, gap: 8, padding: '12px 20px' }}
            >
              <Landmark size={16} />
              <span>{t('banking.viewInSettings', 'Ver en Ajustes')}</span>
            </button>
            <button
              className="btn-secondary"
              onClick={() => navigate('/')}
              style={{ flex: 1, gap: 8, padding: '12px 20px' }}
            >
              <span>{t('banking.goToDashboard', 'Ir al Dashboard')}</span>
              <ArrowRight size={16} />
            </button>
          </div>
        )}

        {status === 'error' && (
          <button
            className="btn-primary"
            onClick={() => navigate('/settings')}
            style={{ marginTop: 12, width: '100%', padding: '12px 20px' }}
          >
            <span>{t('banking.returnToSettings', 'Volver a Ajustes')}</span>
          </button>
        )}
      </div>
    </div>
  );
}
