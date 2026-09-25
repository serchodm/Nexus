import { useState } from 'react';
import { QRCodeSVG } from 'qrcode.react';
import { 
  CheckCircle2, 
  KeyRound, 
  LogOut, 
  QrCode, 
  ShieldCheck, 
  User as UserIcon, 
  AlertCircle,
  Sparkles
} from 'lucide-react';
import { useAuth, authStore } from '../../lib/auth-store';
import { apiClient } from '../../lib/api-client';
import type { AuthSuccessResponse, MfaRequiredResponse } from '../../types/auth';

export function AuthSection() {
  const { user, isAuthenticated } = useAuth();
  
  // Estados para Login
  const [mockEmail, setMockEmail] = useState('sergio@nexus.local');
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Estados para Desafío MFA (Paso 2)
  const [mfaChallengeToken, setMfaChallengeToken] = useState<string | null>(null);
  const [mfaCode, setMfaCode] = useState(['', '', '', '', '', '']);

  // Estados para Enrolamiento QR (Activar MFA)
  const [setupData, setSetupData] = useState<{ secretKey: string; qrCodeUri: string } | null>(null);
  const [setupCode, setSetupCode] = useState(['', '', '', '', '', '']);
  const [setupSuccess, setSetupSuccess] = useState(false);

  // 1. Manejo de Login con Google (o Dev Mock)
  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setErrorMessage(null);

    try {
      // Usamos el formato dev-mock para pruebas locales sin credenciales de Google Cloud
      const idToken = `dev-mock:${mockEmail.trim()}`;
      const response = await apiClient<AuthSuccessResponse | MfaRequiredResponse>('/api/auth/google', {
        method: 'POST',
        body: JSON.stringify({ idToken }),
      });

      if ('mfaRequired' in response && response.mfaRequired) {
        // Usuario tiene MFA activado -> Mostrar formulario de 6 dígitos
        setMfaChallengeToken(response.mfaToken);
      } else if ('accessToken' in response) {
        // Login directo exitoso
        authStore.setSession(response.accessToken, response.user, response.refreshToken);
      }
    } catch (err) {
      setErrorMessage(err instanceof Error ? err.message : 'Error al autenticar');
    } finally {
      setIsLoading(false);
    }
  };

  // 2. Manejo de verificación TOTP en Desafío de Login (Paso 2)
  const handleVerifyChallenge = async (e: React.FormEvent) => {
    e.preventDefault();
    const code = mfaCode.join('');
    if (code.length !== 6) return;

    setIsLoading(true);
    setErrorMessage(null);

    try {
      const response = await apiClient<{ success: boolean; accessToken: string; refreshToken?: string; user: AuthSuccessResponse['user'] }>('/api/auth/mfa/verify', {
        method: 'POST',
        body: JSON.stringify({
          code,
          mfaToken: mfaChallengeToken,
        }),
      });

      if (response.success && response.accessToken) {
        authStore.setSession(response.accessToken, response.user, response.refreshToken);
        setMfaChallengeToken(null);
        setMfaCode(['', '', '', '', '', '']);
      }
    } catch (err) {
      setErrorMessage(err instanceof Error ? err.message : 'Código TOTP incorrecto o expirado');
    } finally {
      setIsLoading(false);
    }
  };

  // 3. Manejo de Enrolamiento MFA (Generar QR)
  const handleStartMfaSetup = async () => {
    setIsLoading(true);
    setErrorMessage(null);
    try {
      const data = await apiClient<{ secretKey: string; qrCodeUri: string }>('/api/auth/mfa/setup', {
        method: 'POST',
      });
      setSetupData(data);
    } catch (err) {
      setErrorMessage(err instanceof Error ? err.message : 'No se pudo iniciar el setup MFA');
    } finally {
      setIsLoading(false);
    }
  };

  // 4. Confirmar primer código para activar MFA definitivamente
  const handleConfirmMfaActivation = async (e: React.FormEvent) => {
    e.preventDefault();
    const code = setupCode.join('');
    if (code.length !== 6) return;

    setIsLoading(true);
    setErrorMessage(null);
    try {
      const response = await apiClient<{ success: boolean; user: AuthSuccessResponse['user'] }>('/api/auth/mfa/verify', {
        method: 'POST',
        body: JSON.stringify({ code }),
      });

      if (response.success) {
        setSetupSuccess(true);
        if (response.user) {
          authStore.setSession(authStore.getAccessToken()!, response.user);
        }
        setTimeout(() => {
          setSetupData(null);
          setSetupSuccess(false);
          setSetupCode(['', '', '', '', '', '']);
        }, 2500);
      }
    } catch (err) {
      setErrorMessage(err instanceof Error ? err.message : 'Código inválido');
    } finally {
      setIsLoading(false);
    }
  };

  // 5. Cerrar Sesión
  const handleLogout = async () => {
    try {
      const mobileToken = authStore.getRefreshToken();
      await apiClient('/api/auth/logout', { 
        method: 'POST',
        body: JSON.stringify(mobileToken ? { refreshToken: mobileToken } : {})
      });
    } finally {
      authStore.clearSession();
      setMfaChallengeToken(null);
      setSetupData(null);
    }
  };

  // Helper para inputs de 6 dígitos
  const updateDigits = (
    currentDigits: string[],
    setDigits: (d: string[]) => void,
    index: number,
    val: string,
    prefix: string
  ) => {
    if (val.length > 1) {
      const digits = val.replace(/\D/g, '').slice(0, 6).split('');
      const updated = [...currentDigits];
      digits.forEach((d, i) => {
        if (i < 6) updated[i] = d;
      });
      setDigits(updated);
      return;
    }
    const updated = [...currentDigits];
    updated[index] = val.replace(/\D/g, '');
    setDigits(updated);
    if (val && index < 5) {
      document.getElementById(`${prefix}-${index + 1}`)?.focus();
    }
  };

  return (
    <section className="glass-card" style={{ padding: '2.5rem', marginBottom: '3.5rem' }}>
      {errorMessage && (
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '0.6rem',
            padding: '0.75rem 1rem',
            borderRadius: 'var(--radius-md)',
            background: 'var(--danger-bg)',
            border: '1px solid hsla(354, 70%, 54%, 0.3)',
            color: 'var(--danger)',
            fontSize: '0.9rem',
            marginBottom: '1.5rem',
          }}
        >
          <AlertCircle size={18} />
          <span>{errorMessage}</span>
        </div>
      )}

      {/* CASO A: Usuario Autenticado */}
      {isAuthenticated && user ? (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem', marginBottom: '2rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
              {user.avatarUrl ? (
                <img
                  src={user.avatarUrl}
                  alt={user.fullName}
                  style={{ width: '56px', height: '56px', borderRadius: '50%', border: '2px solid var(--primary-500)' }}
                />
              ) : (
                <div style={{ width: '56px', height: '56px', borderRadius: '50%', background: 'var(--bg-surface-elevated)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                  <UserIcon size={26} color="var(--primary-500)" />
                </div>
              )}
              <div>
                <h3 style={{ fontSize: '1.4rem' }}>{user.fullName}</h3>
                <div style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>{user.email}</div>
              </div>
            </div>

            <button type="button" className="btn-secondary" onClick={handleLogout} style={{ color: 'var(--danger)' }}>
              <LogOut size={16} />
              Cerrar Sesión
            </button>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))', gap: '1.25rem', marginBottom: '2rem' }}>
            <div style={{ padding: '1rem', borderRadius: 'var(--radius-md)', background: 'var(--bg-surface-elevated)' }}>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-dim)', marginBottom: '0.25rem' }}>ROL ASIGNADO</div>
              <div style={{ fontWeight: 600, fontSize: '1.1rem' }}>{user.role}</div>
            </div>

            <div style={{ padding: '1rem', borderRadius: 'var(--radius-md)', background: 'var(--bg-surface-elevated)' }}>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-dim)', marginBottom: '0.25rem' }}>ESTADO MFA (TOTP)</div>
              <div style={{ fontWeight: 600, fontSize: '1.1rem', color: user.isMfaEnabled ? 'var(--success)' : 'var(--warning)' }}>
                {user.isMfaEnabled ? 'Protegido con Google Authenticator' : 'MFA Desactivado'}
              </div>
            </div>
          </div>

          {/* Modal / Panel de Enrolamiento MFA */}
          {!user.isMfaEnabled && !setupData && (
            <div style={{ padding: '1.5rem', borderRadius: 'var(--radius-md)', background: 'hsla(217, 91%, 60%, 0.08)', border: '1px solid hsla(217, 91%, 60%, 0.2)', display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
              <div>
                <h4 style={{ fontSize: '1.1rem', marginBottom: '0.25rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                  <ShieldCheck size={18} color="var(--primary-500)" />
                  Protege tu cuenta con verificación en 2 pasos
                </h4>
                <p style={{ color: 'var(--text-muted)', fontSize: '0.88rem' }}>
                  Vincula Google Authenticator para solicitar un código de 6 dígitos cada vez que inicies sesión.
                </p>
              </div>
              <button type="button" className="btn-primary" onClick={handleStartMfaSetup} disabled={isLoading}>
                <QrCode size={16} />
                Activar MFA
              </button>
            </div>
          )}

          {setupData && (
            <div style={{ padding: '2rem', borderRadius: 'var(--radius-md)', background: 'var(--bg-surface-elevated)', marginTop: '1.5rem', textAlign: 'center' }}>
              <h4 style={{ fontSize: '1.25rem', marginBottom: '0.5rem' }}>Escanea este código QR con Google Authenticator</h4>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: '1.5rem' }}>
                Abre la app Google Authenticator o Authy en tu móvil y apunta con la cámara.
              </p>

              <div style={{ display: 'inline-block', padding: '1rem', background: '#ffffff', borderRadius: 'var(--radius-md)', marginBottom: '1rem' }}>
                <QRCodeSVG value={setupData.qrCodeUri} size={180} />
              </div>

              <div style={{ fontSize: '0.85rem', color: 'var(--text-dim)', marginBottom: '1.5rem' }}>
                Clave manual en Base32: <code style={{ color: 'var(--accent-cyan)', fontWeight: 700 }}>{setupData.secretKey}</code>
              </div>

              {setupSuccess ? (
                <div style={{ color: 'var(--success)', fontWeight: 700, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '0.5rem' }}>
                  <CheckCircle2 size={20} />
                  ¡MFA Activado con éxito!
                </div>
              ) : (
                <form onSubmit={handleConfirmMfaActivation}>
                  <p style={{ fontSize: '0.9rem', color: 'var(--text-muted)', marginBottom: '0.75rem' }}>
                    Introduce el código de 6 dígitos que muestra tu móvil para confirmar:
                  </p>
                  <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', marginBottom: '1.25rem' }}>
                    {setupCode.map((digit, idx) => (
                      <input
                        key={idx}
                        id={`setup-${idx}`}
                        type="text"
                        inputMode="numeric"
                        maxLength={6}
                        value={digit}
                        onChange={(e) => updateDigits(setupCode, setSetupCode, idx, e.target.value, 'setup')}
                        style={{
                          width: '42px',
                          height: '50px',
                          textAlign: 'center',
                          fontSize: '1.3rem',
                          fontWeight: 700,
                          borderRadius: 'var(--radius-sm)',
                          background: 'var(--bg-app)',
                          border: '1px solid var(--border-subtle)',
                          color: '#fff',
                        }}
                      />
                    ))}
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'center', gap: '1rem' }}>
                    <button type="submit" className="btn-primary" disabled={isLoading || setupCode.join('').length !== 6}>
                      Confirmar y Activar
                    </button>
                    <button type="button" className="btn-secondary" onClick={() => setSetupData(null)}>
                      Cancelar
                    </button>
                  </div>
                </form>
              )}
            </div>
          )}
        </div>
      ) : mfaChallengeToken ? (
        /* CASO B: Desafío MFA de Paso 2 durante el Login */
        <div style={{ textAlign: 'center', maxWidth: '420px', margin: '0 auto' }}>
          <div style={{ display: 'inline-flex', padding: '0.75rem', borderRadius: '50%', background: 'hsla(188, 94%, 53%, 0.1)', color: 'var(--accent-cyan)', marginBottom: '1rem' }}>
            <KeyRound size={32} />
          </div>
          <h3 style={{ fontSize: '1.5rem', marginBottom: '0.5rem' }}>Verificación en 2 Pasos</h3>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: '1.75rem' }}>
            Esta cuenta tiene MFA activado. Introduce el código de 6 dígitos emitido por tu aplicación autenticadora:
          </p>

          <form onSubmit={handleVerifyChallenge}>
            <div style={{ display: 'flex', justifyContent: 'center', gap: '0.6rem', marginBottom: '1.5rem' }}>
              {mfaCode.map((digit, idx) => (
                <input
                  key={idx}
                  id={`challenge-${idx}`}
                  type="text"
                  inputMode="numeric"
                  maxLength={6}
                  value={digit}
                  onChange={(e) => updateDigits(mfaCode, setMfaCode, idx, e.target.value, 'challenge')}
                  style={{
                    width: '46px',
                    height: '56px',
                    textAlign: 'center',
                    fontSize: '1.5rem',
                    fontWeight: 700,
                    borderRadius: 'var(--radius-md)',
                    background: 'var(--bg-surface-elevated)',
                    border: digit ? '1px solid var(--accent-cyan)' : '1px solid var(--border-subtle)',
                    color: '#fff',
                    outline: 'none',
                    boxShadow: digit ? '0 0 12px var(--accent-cyan-glow)' : 'none',
                  }}
                />
              ))}
            </div>

            <div style={{ display: 'flex', justifyContent: 'center', gap: '1rem' }}>
              <button type="submit" className="btn-primary" disabled={isLoading || mfaCode.join('').length !== 6}>
                {isLoading ? 'Verificando...' : 'Completar Acceso'}
              </button>
              <button
                type="button"
                className="btn-secondary"
                onClick={() => {
                  setMfaChallengeToken(null);
                  setMfaCode(['', '', '', '', '', '']);
                }}
              >
                Volver
              </button>
            </div>
          </form>
        </div>
      ) : (
        /* CASO C: Login Inicial (Google OAuth / Dev Mock) */
        <div style={{ textAlign: 'center', maxWidth: '480px', margin: '0 auto' }}>
          <div style={{ display: 'inline-flex', padding: '0.75rem', borderRadius: '50%', background: 'hsla(217, 91%, 60%, 0.1)', color: 'var(--primary-500)', marginBottom: '1rem' }}>
            <Sparkles size={28} />
          </div>
          <h3 style={{ fontSize: '1.6rem', marginBottom: '0.5rem' }}>Acceso a Nexus</h3>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.92rem', marginBottom: '2rem' }}>
            Autenticación sin contraseñas mediante Google OAuth y sesiones blindadas con Cookies HttpOnly.
          </p>

          <form onSubmit={handleLogin} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            <div style={{ textAlign: 'left' }}>
              <label style={{ fontSize: '0.85rem', color: 'var(--text-dim)', marginBottom: '0.35rem', display: 'block' }}>
                Email de prueba (Modo Desarrollo sin credenciales Cloud):
              </label>
              <input
                type="email"
                required
                value={mockEmail}
                onChange={(e) => setMockEmail(e.target.value)}
                style={{
                  width: '100%',
                  padding: '0.75rem 1rem',
                  borderRadius: 'var(--radius-md)',
                  background: 'var(--bg-surface-elevated)',
                  border: '1px solid var(--border-subtle)',
                  color: '#ffffff',
                  outline: 'none',
                }}
              />
            </div>

            <button type="submit" className="btn-primary" disabled={isLoading} style={{ width: '100%', padding: '0.9rem' }}>
              {isLoading ? 'Iniciando sesión...' : 'Continuar con Google (Simulación Dev)'}
            </button>
          </form>

          <p style={{ fontSize: '0.78rem', color: 'var(--text-dim)', marginTop: '1.25rem' }}>
            En producción se activa el popup nativo de Google Identity Services con el Client ID corporativo.
          </p>
        </div>
      )}
    </section>
  );
}
