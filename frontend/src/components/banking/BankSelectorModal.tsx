import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { X, Search, ShieldCheck, ExternalLink, Loader2, Landmark } from 'lucide-react';
import { useBanking } from '../../hooks/useBanking';
import type { BankInstitution } from '../../types/banking';

interface BankSelectorModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function BankSelectorModal({ isOpen, onClose }: BankSelectorModalProps) {
  const { t } = useTranslation();
  const { institutions, isLoadingInstitutions, startAuth, isStartingAuth } = useBanking('ES');
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedBank, setSelectedBank] = useState<BankInstitution | null>(null);

  if (!isOpen) return null;

  const filteredBanks = institutions.filter((b) =>
    b.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleConnect = async (bank: BankInstitution) => {
    setSelectedBank(bank);
    try {
      const redirectUri = `${window.location.origin}/banking/callback`;
      const res = await startAuth({ institutionCode: bank.code, redirectUri });
      if (res?.authorizationUrl) {
        window.location.href = res.authorizationUrl;
      }
    } catch (err) {
      console.error('Error starting bank authentication:', err);
    }
  };

  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        zIndex: 9999,
        background: 'rgba(0, 0, 0, 0.75)',
        backdropFilter: 'blur(8px)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 16,
      }}
      onClick={(e) => {
        if (e.target === e.currentTarget && !isStartingAuth) onClose();
      }}
    >
      <div
        className="glass-panel"
        style={{
          width: '100%',
          maxWidth: 620,
          maxHeight: '90vh',
          borderRadius: 'var(--radius-2xl)',
          border: '1px solid var(--border-outline-variant)',
          display: 'flex',
          flexDirection: 'column',
          background: 'var(--bg-surface-container-highest)',
          boxShadow: 'var(--shadow-lg)',
          overflow: 'hidden',
          animation: 'fadeIn 0.2s ease-out',
        }}
      >
        {/* Header */}
        <div
          style={{
            padding: '20px 24px',
            borderBottom: '1px solid var(--border-outline-variant)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <div
              style={{
                width: 40,
                height: 40,
                borderRadius: 'var(--radius-lg)',
                background: 'rgba(0, 229, 255, 0.12)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'var(--secondary)',
              }}
            >
              <Landmark size={22} />
            </div>
            <div>
              <h3 style={{ margin: 0, fontSize: 'var(--text-headline-sm)', fontWeight: 700 }}>
                {t('banking.selectBankTitle', 'Conectar Entidad Bancaria')}
              </h3>
              <p style={{ margin: 0, fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)' }}>
                {t('banking.selectBankSubtitle', 'Acceso seguro regulado bajo PSD2 con consentimiento de 180 días')}
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            disabled={isStartingAuth}
            style={{
              background: 'transparent',
              border: 'none',
              color: 'var(--text-muted)',
              cursor: isStartingAuth ? 'not-allowed' : 'pointer',
              padding: 6,
              borderRadius: 'var(--radius-md)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <X size={20} />
          </button>
        </div>

        {/* Security Badge */}
        <div
          style={{
            margin: '16px 24px 0',
            padding: '10px 14px',
            borderRadius: 'var(--radius-lg)',
            background: 'rgba(16, 185, 129, 0.08)',
            border: '1px solid rgba(16, 185, 129, 0.2)',
            display: 'flex',
            alignItems: 'center',
            gap: 10,
          }}
        >
          <ShieldCheck size={18} style={{ color: '#10b981', flexShrink: 0 }} />
          <span style={{ fontSize: 12, color: 'var(--text-main)', lineHeight: 1.4 }}>
            {t(
              'banking.securityNotice',
              'Nexus nunca almacena ni solicita tus contraseñas. Serás redirigido a la pasarela cifrada oficial de tu banco.'
            )}
          </span>
        </div>

        {/* Search Input */}
        <div style={{ padding: '16px 24px 8px' }}>
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: 10,
              padding: '10px 14px',
              borderRadius: 'var(--radius-lg)',
              background: 'var(--bg-surface-container)',
              border: '1px solid var(--border-outline-variant)',
            }}
          >
            <Search size={18} style={{ color: 'var(--text-dim)' }} />
            <input
              type="text"
              placeholder={t('banking.searchPlaceholder', 'Buscar banco (BBVA, Santander, ING, etc.)...')}
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              disabled={isStartingAuth}
              style={{
                width: '100%',
                background: 'transparent',
                border: 'none',
                color: 'var(--text-main)',
                fontSize: 14,
                outline: 'none',
              }}
            />
          </div>
        </div>

        {/* Banks Grid */}
        <div
          style={{
            padding: '12px 24px 24px',
            overflowY: 'auto',
            maxHeight: 380,
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fill, minmax(160px, 1fr))',
            gap: 12,
          }}
        >
          {isLoadingInstitutions ? (
            <div style={{ gridColumn: '1 / -1', textAlign: 'center', padding: 40, color: 'var(--text-muted)' }}>
              <Loader2 size={28} className="animate-spin" style={{ margin: '0 auto 8px' }} />
              <span>{t('banking.loadingBanks', 'Cargando entidades bancarias...')}</span>
            </div>
          ) : filteredBanks.length === 0 ? (
            <div style={{ gridColumn: '1 / -1', textAlign: 'center', padding: 40, color: 'var(--text-muted)' }}>
              <span>{t('banking.noBanksFound', 'No se encontraron bancos con ese nombre.')}</span>
            </div>
          ) : (
            filteredBanks.map((bank) => {
              const isSelected = selectedBank?.code === bank.code && isStartingAuth;
              return (
                <button
                  key={bank.code}
                  onClick={() => handleConnect(bank)}
                  disabled={isStartingAuth}
                  className="glass-panel"
                  style={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    justifyContent: 'center',
                    padding: 16,
                    borderRadius: 'var(--radius-xl)',
                    border: isSelected
                      ? '1px solid var(--secondary)'
                      : '1px solid var(--border-outline-variant)',
                    background: isSelected
                      ? 'rgba(0, 229, 255, 0.08)'
                      : 'var(--bg-surface-container)',
                    cursor: isStartingAuth ? 'not-allowed' : 'pointer',
                    transition: 'all 0.2s ease',
                    gap: 10,
                    textAlign: 'center',
                  }}
                >
                  {bank.logoUrl ? (
                    <img
                      src={bank.logoUrl}
                      alt={bank.name}
                      style={{
                        width: 44,
                        height: 44,
                        objectFit: 'contain',
                        borderRadius: 8,
                        background: '#ffffff',
                        padding: 4,
                      }}
                      onError={(e) => {
                        // Fallback icon si el logo falla
                        (e.currentTarget as HTMLElement).style.display = 'none';
                      }}
                    />
                  ) : (
                    <div
                      style={{
                        width: 44,
                        height: 44,
                        borderRadius: 8,
                        background: 'var(--bg-surface-container-high)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        color: 'var(--secondary)',
                      }}
                    >
                      <Landmark size={24} />
                    </div>
                  )}

                  <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text-main)' }}>
                    {bank.name}
                  </span>

                  <span
                    style={{
                      fontSize: 11,
                      color: isSelected ? 'var(--secondary)' : 'var(--text-dim)',
                      display: 'flex',
                      alignItems: 'center',
                      gap: 4,
                    }}
                  >
                    {isSelected ? (
                      <>
                        <Loader2 size={12} className="animate-spin" />
                        <span>{t('banking.redirecting', 'Conectando...')}</span>
                      </>
                    ) : (
                      <>
                        <span>{t('banking.connectAction', 'Conectar')}</span>
                        <ExternalLink size={11} />
                      </>
                    )}
                  </span>
                </button>
              );
            })
          )}
        </div>
      </div>
    </div>
  );
}
