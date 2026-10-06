import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Landmark,
  RefreshCw,
  Plus,
  Trash2,
  CheckCircle2,
  AlertTriangle,
  Clock,
  ExternalLink,
} from 'lucide-react';
import { useBanking } from '../../hooks/useBanking';
import { BankSelectorModal } from './BankSelectorModal';

export function ConnectedBanksCard() {
  const { t } = useTranslation();
  const {
    connections,
    isLoadingConnections,
    syncConnection,
    isSyncing,
    revokeConnection,
    isRevoking,
    startAuth,
  } = useBanking();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [syncingId, setSyncingId] = useState<string | null>(null);
  const [syncFeedback, setSyncFeedback] = useState<string | null>(null);

  const handleSync = async (connectionId: string) => {
    setSyncingId(connectionId);
    try {
      const res = await syncConnection({ connectionId });
      setSyncFeedback(
        t('banking.syncSuccess', 'Sincronizado: {{count}} movimientos nuevos.', {
          count: res.newTransactionsImported,
        })
      );
      setTimeout(() => setSyncFeedback(null), 3000);
    } catch (err) {
      console.error('Sync failed:', err);
    } finally {
      setSyncingId(null);
    }
  };

  const handleRenew = async (institutionCode: string) => {
    try {
      const redirectUri = `${window.location.origin}/banking/callback`;
      const res = await startAuth({ institutionCode, redirectUri });
      if (res?.authorizationUrl) {
        window.location.href = res.authorizationUrl;
      }
    } catch (err) {
      console.error('Renew SCA failed:', err);
    }
  };

  const handleRevoke = async (connectionId: string) => {
    if (confirm(t('banking.confirmRevoke', '¿Deseas desvincular esta entidad bancaria?'))) {
      await revokeConnection(connectionId);
    }
  };

  const formatCurrency = (val: number | null | undefined) => {
    if (val === null || val === undefined) return '0,00 €';
    return new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' }).format(val);
  };

  return (
    <>
      <section
        className="glass-panel"
        style={{
          padding: 'var(--space-lg)',
          borderRadius: 'var(--radius-xl)',
          display: 'flex',
          flexDirection: 'column',
          gap: 'var(--space-md)',
        }}
      >
        {/* Card Header */}
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderBottom: '1px solid var(--border-outline-variant)',
            paddingBottom: 'var(--space-sm)',
            flexWrap: 'wrap',
            gap: 12,
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Landmark size={20} style={{ color: 'var(--secondary)' }} />
            <div>
              <h3 style={{ margin: 0, fontSize: 'var(--text-headline-sm)' }}>
                {t('banking.connectedBanksTitle', 'Banca Conectada (Open Banking)')}
              </h3>
              <p style={{ margin: 0, fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)' }}>
                {t('banking.connectedBanksSubtitle', 'Sincronización desatendida mediante API oficial PSD2')}
              </p>
            </div>
          </div>

          <button
            className="btn-primary"
            onClick={() => setIsModalOpen(true)}
            style={{ gap: 8, padding: '8px 16px', fontSize: 13 }}
          >
            <Plus size={16} />
            <span>{t('banking.addBankButton', 'Conectar Entidad')}</span>
          </button>
        </div>

        {/* Sync Feedback Toast */}
        {syncFeedback && (
          <div
            style={{
              padding: '10px 14px',
              borderRadius: 'var(--radius-lg)',
              background: 'rgba(0, 229, 255, 0.1)',
              border: '1px solid var(--secondary)',
              color: 'var(--text-main)',
              fontSize: 13,
              display: 'flex',
              alignItems: 'center',
              gap: 8,
              animation: 'fadeIn 0.2s ease',
            }}
          >
            <CheckCircle2 size={16} style={{ color: 'var(--secondary)' }} />
            <span>{syncFeedback}</span>
          </div>
        )}

        {/* Connections List */}
        {isLoadingConnections ? (
          <div style={{ padding: 24, textAlign: 'center', color: 'var(--text-muted)' }}>
            <RefreshCw size={24} className="animate-spin" style={{ margin: '0 auto 8px' }} />
            <span>{t('banking.loadingConnections', 'Cargando entidades enlazadas...')}</span>
          </div>
        ) : connections.length === 0 ? (
          <div
            style={{
              padding: 32,
              textAlign: 'center',
              borderRadius: 'var(--radius-lg)',
              background: 'var(--bg-surface-container)',
              border: '1px dashed var(--border-outline-variant)',
            }}
          >
            <Landmark size={32} style={{ color: 'var(--text-dim)', margin: '0 auto 12px' }} />
            <p style={{ margin: '0 0 12px', fontSize: 14, color: 'var(--text-muted)' }}>
              {t('banking.noConnectionsMessage', 'No tienes ninguna cuenta bancaria conectada por Open Banking todavía.')}
            </p>
            <button
              className="btn-secondary"
              onClick={() => setIsModalOpen(true)}
              style={{ gap: 8, margin: '0 auto' }}
            >
              <Plus size={16} />
              <span>{t('banking.connectFirstBank', 'Conectar mi primer banco')}</span>
            </button>
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
            {connections.map((conn) => {
              const isCurrentlySyncing = syncingId === conn.id || isSyncing;
              const isExpiringSoon = conn.daysRemaining <= 15;
              const isExpired = conn.status === 'Expired' || conn.daysRemaining <= 0;

              return (
                <div
                  key={conn.id}
                  style={{
                    padding: 16,
                    borderRadius: 'var(--radius-lg)',
                    background: 'var(--bg-surface-container-high)',
                    border: '1px solid var(--border-outline-variant)',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: 12,
                  }}
                >
                  {/* Bank Header Row */}
                  <div
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'space-between',
                      flexWrap: 'wrap',
                      gap: 10,
                    }}
                  >
                    <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                      {conn.logoUrl ? (
                        <img
                          src={conn.logoUrl}
                          alt={conn.institutionName}
                          style={{
                            width: 36,
                            height: 36,
                            objectFit: 'contain',
                            borderRadius: 8,
                            background: '#ffffff',
                            padding: 3,
                          }}
                        />
                      ) : (
                        <div
                          style={{
                            width: 36,
                            height: 36,
                            borderRadius: 8,
                            background: 'var(--bg-surface-container)',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            color: 'var(--secondary)',
                          }}
                        >
                          <Landmark size={20} />
                        </div>
                      )}

                      <div>
                        <span style={{ fontSize: 15, fontWeight: 700, color: 'var(--text-main)' }}>
                          {conn.institutionName}
                        </span>
                        <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginTop: 2 }}>
                          {/* Consent Status Badge */}
                          {isExpired ? (
                            <span
                              style={{
                                fontSize: 11,
                                padding: '2px 8px',
                                borderRadius: 12,
                                background: 'rgba(239, 68, 68, 0.15)',
                                color: '#ef4444',
                                fontWeight: 600,
                                display: 'flex',
                                alignItems: 'center',
                                gap: 4,
                              }}
                            >
                              <AlertTriangle size={11} />
                              {t('banking.statusExpired', 'Consentimiento Caducado')}
                            </span>
                          ) : isExpiringSoon ? (
                            <span
                              style={{
                                fontSize: 11,
                                padding: '2px 8px',
                                borderRadius: 12,
                                background: 'rgba(245, 158, 11, 0.15)',
                                color: '#f59e0b',
                                fontWeight: 600,
                                display: 'flex',
                                alignItems: 'center',
                                gap: 4,
                              }}
                            >
                              <Clock size={11} />
                              {t('banking.statusExpiring', 'Expira en {{days}} días', {
                                days: conn.daysRemaining,
                              })}
                            </span>
                          ) : (
                            <span
                              style={{
                                fontSize: 11,
                                padding: '2px 8px',
                                borderRadius: 12,
                                background: 'rgba(16, 185, 129, 0.15)',
                                color: '#10b981',
                                fontWeight: 600,
                                display: 'flex',
                                alignItems: 'center',
                                gap: 4,
                              }}
                            >
                              <CheckCircle2 size={11} />
                              {t('banking.statusActive', 'Activo ({{days}} días restantes)', {
                                days: conn.daysRemaining,
                              })}
                            </span>
                          )}

                          {conn.lastSyncedAt && (
                            <span style={{ fontSize: 11, color: 'var(--text-dim)' }}>
                              • {t('banking.lastSynced', 'Último sync')}:{' '}
                              {new Date(conn.lastSyncedAt).toLocaleTimeString([], {
                                hour: '2-digit',
                                minute: '2-digit',
                              })}
                            </span>
                          )}
                        </div>
                      </div>
                    </div>

                    {/* Actions */}
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                      {isExpiringSoon || isExpired ? (
                        <button
                          className="btn-primary"
                          onClick={() => handleRenew(conn.institutionCode)}
                          style={{
                            padding: '6px 12px',
                            fontSize: 12,
                            gap: 6,
                            background: '#f59e0b',
                            borderColor: '#f59e0b',
                          }}
                        >
                          <ExternalLink size={13} />
                          <span>{t('banking.renewSca', 'Renovar SCA')}</span>
                        </button>
                      ) : null}

                      <button
                        className="btn-secondary"
                        onClick={() => handleSync(conn.id)}
                        disabled={isCurrentlySyncing}
                        style={{ padding: '6px 12px', fontSize: 12, gap: 6 }}
                      >
                        <RefreshCw size={13} className={isCurrentlySyncing ? 'animate-spin' : ''} />
                        <span>
                          {isCurrentlySyncing
                            ? t('banking.syncing', 'Sincronizando...')
                            : t('banking.syncNow', 'Sincronizar')}
                        </span>
                      </button>

                      <button
                        onClick={() => handleRevoke(conn.id)}
                        disabled={isRevoking}
                        title={t('banking.disconnect', 'Desconectar entidad')}
                        style={{
                          background: 'transparent',
                          border: 'none',
                          color: 'var(--text-muted)',
                          padding: 6,
                          borderRadius: 'var(--radius-md)',
                          cursor: 'pointer',
                          display: 'flex',
                          alignItems: 'center',
                        }}
                      >
                        <Trash2 size={15} />
                      </button>
                    </div>
                  </div>

                  {/* Connected Accounts Sub-list */}
                  <div
                    style={{
                      background: 'var(--bg-surface-container)',
                      borderRadius: 'var(--radius-md)',
                      padding: '8px 12px',
                      display: 'flex',
                      flexDirection: 'column',
                      gap: 6,
                    }}
                  >
                    {conn.accounts.map((acc) => (
                      <div
                        key={acc.id}
                        style={{
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'space-between',
                          fontSize: 13,
                        }}
                      >
                        <div>
                          <span style={{ fontWeight: 600, color: 'var(--text-main)' }}>
                            {acc.accountName}
                          </span>
                          {acc.iban && (
                            <span
                              style={{
                                marginLeft: 8,
                                color: 'var(--text-dim)',
                                fontSize: 12,
                                fontFamily: 'var(--font-mono)',
                              }}
                            >
                              {acc.iban}
                            </span>
                          )}
                        </div>
                        <span className="numeral" style={{ fontWeight: 700, color: 'var(--secondary)' }}>
                          {formatCurrency(acc.currentBalance)}
                        </span>
                      </div>
                    ))}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </section>

      <BankSelectorModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} />
    </>
  );
}
