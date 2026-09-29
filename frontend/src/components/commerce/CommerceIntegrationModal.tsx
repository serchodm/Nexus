import React, { useState } from 'react';
import { 
  X, 
  Upload, 
  ShoppingBag, 
  CreditCard, 
  CheckCircle, 
  AlertCircle, 
  RefreshCw, 
  FileText,
  Key,
  ShieldCheck
} from 'lucide-react';
import { useCommerce } from '../../hooks/useCommerce';
import type { ImportCommerceReportResult } from '../../types/commerce';

interface CommerceIntegrationModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const CommerceIntegrationModal: React.FC<CommerceIntegrationModalProps> = ({
  isOpen,
  onClose,
}) => {
  const [activeTab, setActiveTab] = useState<'amazon' | 'paypal' | 'paypal-api'>('amazon');
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [importResult, setImportResult] = useState<ImportCommerceReportResult | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Formulario API PayPal
  const [payPalEmail, setPayPalEmail] = useState('');
  const [payPalToken, setPayPalToken] = useState('');

  const {
    importAmazon,
    importPayPal,
    connectPayPal,
    syncPayPal,
  } = useCommerce();

  if (!isOpen) return null;

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setSelectedFile(e.target.files[0]);
      setImportResult(null);
      setErrorMessage(null);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) return;
    setErrorMessage(null);
    setImportResult(null);

    try {
      if (activeTab === 'amazon') {
        const res = await importAmazon.mutateAsync(selectedFile);
        setImportResult(res);
      } else if (activeTab === 'paypal') {
        const res = await importPayPal.mutateAsync(selectedFile);
        setImportResult(res);
      }
    } catch (err: any) {
      setErrorMessage(err.message || 'Error al procesar el archivo');
    }
  };

  const handleConnectPayPal = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!payPalEmail || !payPalToken) return;
    setErrorMessage(null);
    setImportResult(null);

    try {
      await connectPayPal.mutateAsync({
        accountEmail: payPalEmail,
        accessToken: payPalToken,
      });
      const syncRes = await syncPayPal.mutateAsync({});
      setImportResult(syncRes);
    } catch (err: any) {
      setErrorMessage(err.message || 'Error al vincular y sincronizar la cuenta de PayPal');
    }
  };

  const isPending = importAmazon.isPending || importPayPal.isPending || connectPayPal.isPending || syncPayPal.isPending;

  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        zIndex: 50,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'rgba(0, 0, 0, 0.75)',
        backdropFilter: 'blur(8px)',
        padding: '1rem',
      }}
    >
      <div
        style={{
          width: '100%',
          maxWidth: '680px',
          background: 'var(--bg-card, #111827)',
          border: '1px solid var(--border-glass, rgba(255, 255, 255, 0.1))',
          borderRadius: 'var(--radius-xl, 1rem)',
          boxShadow: 'var(--shadow-xl, 0 20px 25px -5px rgba(0, 0, 0, 0.5))',
          overflow: 'hidden',
          display: 'flex',
          flexDirection: 'column',
          maxHeight: '90vh',
        }}
      >
        {/* Cabecera Modal */}
        <div
          style={{
            padding: '1.25rem 1.5rem',
            borderBottom: '1px solid var(--border-glass, rgba(255, 255, 255, 0.08))',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            background: 'hsla(222, 47%, 12%, 0.5)',
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <div
              style={{
                width: '2.5rem',
                height: '2.5rem',
                borderRadius: '0.5rem',
                background: 'linear-gradient(135deg, #f59e0b 0%, #3b82f6 100%)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#fff',
              }}
            >
              <ShoppingBag size={20} />
            </div>
            <div>
              <h2 style={{ fontSize: '1.15rem', fontWeight: 700, color: 'var(--text-main, #f3f4f6)', margin: 0 }}>
                Conectar Amazon & PayPal
              </h2>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-dim, #9ca3af)', margin: '0.15rem 0 0' }}>
                Reconciliación y desglose automático de compras en partidas bancarias
              </p>
            </div>
          </div>

          <button
            onClick={onClose}
            style={{
              background: 'transparent',
              border: 'none',
              color: 'var(--text-muted, #6b7280)',
              cursor: 'pointer',
              padding: '0.4rem',
              borderRadius: '0.375rem',
            }}
          >
            <X size={20} />
          </button>
        </div>

        {/* Pestañas de selección */}
        <div
          style={{
            display: 'flex',
            borderBottom: '1px solid var(--border-glass, rgba(255, 255, 255, 0.08))',
            background: 'hsla(222, 47%, 10%, 0.3)',
            padding: '0.25rem 1.5rem 0',
            gap: '1rem',
          }}
        >
          <button
            onClick={() => {
              setActiveTab('amazon');
              setSelectedFile(null);
              setImportResult(null);
            }}
            style={{
              padding: '0.75rem 0.5rem',
              background: 'transparent',
              border: 'none',
              borderBottom: activeTab === 'amazon' ? '2px solid #f59e0b' : '2px solid transparent',
              color: activeTab === 'amazon' ? '#f59e0b' : 'var(--text-muted, #9ca3af)',
              fontWeight: activeTab === 'amazon' ? 700 : 500,
              fontSize: '0.9rem',
              cursor: 'pointer',
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
            }}
          >
            <ShoppingBag size={16} />
            Amazon (Reporte CSV)
          </button>

          <button
            onClick={() => {
              setActiveTab('paypal');
              setSelectedFile(null);
              setImportResult(null);
            }}
            style={{
              padding: '0.75rem 0.5rem',
              background: 'transparent',
              border: 'none',
              borderBottom: activeTab === 'paypal' ? '2px solid #38bdf8' : '2px solid transparent',
              color: activeTab === 'paypal' ? '#38bdf8' : 'var(--text-muted, #9ca3af)',
              fontWeight: activeTab === 'paypal' ? 700 : 500,
              fontSize: '0.9rem',
              cursor: 'pointer',
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
            }}
          >
            <CreditCard size={16} />
            PayPal (Actividad CSV)
          </button>

          <button
            onClick={() => {
              setActiveTab('paypal-api');
              setSelectedFile(null);
              setImportResult(null);
            }}
            style={{
              padding: '0.75rem 0.5rem',
              background: 'transparent',
              border: 'none',
              borderBottom: activeTab === 'paypal-api' ? '2px solid #38bdf8' : '2px solid transparent',
              color: activeTab === 'paypal-api' ? '#38bdf8' : 'var(--text-muted, #9ca3af)',
              fontWeight: activeTab === 'paypal-api' ? 700 : 500,
              fontSize: '0.9rem',
              cursor: 'pointer',
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
            }}
          >
            <Key size={16} />
            PayPal (API Directa)
          </button>
        </div>

        {/* Contenido Modal */}
        <div style={{ padding: '1.5rem', overflowY: 'auto' }}>
          {activeTab === 'amazon' && (
            <div>
              <div
                style={{
                  background: 'hsla(38, 92%, 50%, 0.08)',
                  border: '1px solid hsla(38, 92%, 50%, 0.2)',
                  borderRadius: '0.75rem',
                  padding: '1rem',
                  marginBottom: '1.25rem',
                  display: 'flex',
                  gap: '0.75rem',
                }}
              >
                <div style={{ color: '#f59e0b', marginTop: '0.1rem' }}>
                  <ShieldCheck size={20} />
                </div>
                <div style={{ fontSize: '0.85rem', color: 'var(--text-dim, #d1d5db)', lineHeight: 1.5 }}>
                  <strong style={{ color: '#fbbf24', display: 'block', marginBottom: '0.25rem' }}>
                    100% Privado y Oficial (Sin extensiones de navegador)
                  </strong>
                  Para conciliar tus pedidos, descarga tu informe oficial de pedidos en{' '}
                  <span style={{ color: '#fff', fontWeight: 600 }}>Amazon &gt; Mi Cuenta &gt; Informes de pedidos</span>{' '}
                  y súbelo aquí. Nexus extraerá los artículos exactos respetando la paridad con tu banco.
                </div>
              </div>

              {/* Dropzone */}
              <label
                style={{
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  border: '2px dashed var(--border-glass, rgba(255, 255, 255, 0.15))',
                  borderRadius: '0.75rem',
                  padding: '2rem 1.5rem',
                  cursor: 'pointer',
                  background: selectedFile ? 'hsla(222, 47%, 16%, 0.4)' : 'transparent',
                  transition: 'border-color 0.2s, background-color 0.2s',
                }}
              >
                <input
                  type="file"
                  accept=".csv,.txt"
                  onChange={handleFileChange}
                  style={{ display: 'none' }}
                />
                <Upload size={32} style={{ color: '#f59e0b', marginBottom: '0.75rem' }} />
                <span style={{ fontWeight: 600, color: 'var(--text-main, #f3f4f6)', fontSize: '0.95rem' }}>
                  {selectedFile ? selectedFile.name : 'Haz clic o arrastra tu archivo CSV de Amazon'}
                </span>
                <span style={{ fontSize: '0.78rem', color: 'var(--text-dim, #9ca3af)', marginTop: '0.35rem' }}>
                  Soporta formatos oficiales en Español e Inglés (.csv)
                </span>
              </label>

              {selectedFile && (
                <div style={{ marginTop: '1.25rem', display: 'flex', justifyContent: 'flex-end' }}>
                  <button
                    onClick={handleUpload}
                    disabled={isPending}
                    style={{
                      display: 'inline-flex',
                      alignItems: 'center',
                      gap: '0.5rem',
                      background: 'linear-gradient(135deg, #d97706 0%, #b45309 100%)',
                      border: 'none',
                      borderRadius: '0.5rem',
                      color: '#fff',
                      padding: '0.65rem 1.25rem',
                      fontWeight: 600,
                      cursor: isPending ? 'not-allowed' : 'pointer',
                      opacity: isPending ? 0.7 : 1,
                    }}
                  >
                    {isPending ? <RefreshCw className="spin" size={16} /> : <FileText size={16} />}
                    {isPending ? 'Procesando pedidos...' : 'Importar y Reconciliar Pedidos'}
                  </button>
                </div>
              )}
            </div>
          )}

          {activeTab === 'paypal' && (
            <div>
              <div
                style={{
                  background: 'hsla(200, 98%, 39%, 0.08)',
                  border: '1px solid hsla(200, 98%, 39%, 0.2)',
                  borderRadius: '0.75rem',
                  padding: '1rem',
                  marginBottom: '1.25rem',
                  display: 'flex',
                  gap: '0.75rem',
                }}
              >
                <div style={{ color: '#38bdf8', marginTop: '0.1rem' }}>
                  <CreditCard size={20} />
                </div>
                <div style={{ fontSize: '0.85rem', color: 'var(--text-dim, #d1d5db)', lineHeight: 1.5 }}>
                  <strong style={{ color: '#38bdf8', display: 'block', marginBottom: '0.25rem' }}>
                    Extracto de Actividad de PayPal
                  </strong>
                  Descarga tu archivo de actividad en formato CSV desde tu cuenta de PayPal para desenmascarar los cobros
                  opacos de tu tarjeta (como Spotify, Uber, Steam, etc.).
                </div>
              </div>

              <label
                style={{
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  border: '2px dashed var(--border-glass, rgba(255, 255, 255, 0.15))',
                  borderRadius: '0.75rem',
                  padding: '2rem 1.5rem',
                  cursor: 'pointer',
                  background: selectedFile ? 'hsla(222, 47%, 16%, 0.4)' : 'transparent',
                }}
              >
                <input
                  type="file"
                  accept=".csv,.txt"
                  onChange={handleFileChange}
                  style={{ display: 'none' }}
                />
                <Upload size={32} style={{ color: '#38bdf8', marginBottom: '0.75rem' }} />
                <span style={{ fontWeight: 600, color: 'var(--text-main, #f3f4f6)', fontSize: '0.95rem' }}>
                  {selectedFile ? selectedFile.name : 'Haz clic o arrastra tu extracto CSV de PayPal'}
                </span>
                <span style={{ fontSize: '0.78rem', color: 'var(--text-dim, #9ca3af)', marginTop: '0.35rem' }}>
                  Archivos de extracto de actividad (.csv)
                </span>
              </label>

              {selectedFile && (
                <div style={{ marginTop: '1.25rem', display: 'flex', justifyContent: 'flex-end' }}>
                  <button
                    onClick={handleUpload}
                    disabled={isPending}
                    style={{
                      display: 'inline-flex',
                      alignItems: 'center',
                      gap: '0.5rem',
                      background: 'linear-gradient(135deg, #0284c7 0%, #0369a1 100%)',
                      border: 'none',
                      borderRadius: '0.5rem',
                      color: '#fff',
                      padding: '0.65rem 1.25rem',
                      fontWeight: 600,
                      cursor: isPending ? 'not-allowed' : 'pointer',
                      opacity: isPending ? 0.7 : 1,
                    }}
                  >
                    {isPending ? <RefreshCw className="spin" size={16} /> : <FileText size={16} />}
                    {isPending ? 'Procesando actividad...' : 'Importar y Reconciliar Actividad'}
                  </button>
                </div>
              )}
            </div>
          )}

          {activeTab === 'paypal-api' && (
            <form onSubmit={handleConnectPayPal}>
              <div
                style={{
                  background: 'hsla(217, 91%, 60%, 0.08)',
                  border: '1px solid hsla(217, 91%, 60%, 0.2)',
                  borderRadius: '0.75rem',
                  padding: '1rem',
                  marginBottom: '1.25rem',
                }}
              >
                <div style={{ fontSize: '0.85rem', color: 'var(--text-dim, #d1d5db)', lineHeight: 1.5 }}>
                  <strong style={{ color: '#60a5fa', display: 'block', marginBottom: '0.25rem' }}>
                    Sincronización Automática Cifrada (AES-256-GCM)
                  </strong>
                  Conecta tus credenciales de API para mantener sincronizada la actividad de PayPal sin necesidad de subir
                  ficheros manualmente.
                </div>
              </div>

              <div style={{ marginBottom: '1rem' }}>
                <label style={{ display: 'block', fontSize: '0.82rem', color: 'var(--text-dim, #9ca3af)', marginBottom: '0.35rem' }}>
                  Email de la cuenta de PayPal
                </label>
                <input
                  type="email"
                  value={payPalEmail}
                  onChange={(e) => setPayPalEmail(e.target.value)}
                  placeholder="ejemplo@paypal.com"
                  required
                  style={{
                    width: '100%',
                    padding: '0.65rem 0.85rem',
                    background: 'hsla(222, 47%, 16%, 0.4)',
                    border: '1px solid var(--border-glass, rgba(255, 255, 255, 0.1))',
                    borderRadius: '0.5rem',
                    color: '#fff',
                    fontSize: '0.9rem',
                  }}
                />
              </div>

              <div style={{ marginBottom: '1.25rem' }}>
                <label style={{ display: 'block', fontSize: '0.82rem', color: 'var(--text-dim, #9ca3af)', marginBottom: '0.35rem' }}>
                  Token de Acceso / Client Secret
                </label>
                <input
                  type="password"
                  value={payPalToken}
                  onChange={(e) => setPayPalToken(e.target.value)}
                  placeholder="Token de acceso o clave secreta"
                  required
                  style={{
                    width: '100%',
                    padding: '0.65rem 0.85rem',
                    background: 'hsla(222, 47%, 16%, 0.4)',
                    border: '1px solid var(--border-glass, rgba(255, 255, 255, 0.1))',
                    borderRadius: '0.5rem',
                    color: '#fff',
                    fontSize: '0.9rem',
                  }}
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
                <button
                  type="submit"
                  disabled={isPending}
                  style={{
                    display: 'inline-flex',
                    alignItems: 'center',
                    gap: '0.5rem',
                    background: 'linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%)',
                    border: 'none',
                    borderRadius: '0.5rem',
                    color: '#fff',
                    padding: '0.65rem 1.25rem',
                    fontWeight: 600,
                    cursor: isPending ? 'not-allowed' : 'pointer',
                    opacity: isPending ? 0.7 : 1,
                  }}
                >
                  {isPending ? <RefreshCw className="spin" size={16} /> : <Key size={16} />}
                  {isPending ? 'Conectando...' : 'Conectar y Sincronizar'}
                </button>
              </div>
            </form>
          )}

          {/* Resultado o Mensajes */}
          {errorMessage && (
            <div
              style={{
                marginTop: '1.25rem',
                padding: '0.85rem 1rem',
                borderRadius: '0.5rem',
                background: 'hsla(0, 84%, 60%, 0.12)',
                border: '1px solid hsla(0, 84%, 60%, 0.3)',
                color: '#f87171',
                fontSize: '0.85rem',
                display: 'flex',
                alignItems: 'center',
                gap: '0.5rem',
              }}
            >
              <AlertCircle size={18} />
              <span>{errorMessage}</span>
            </div>
          )}

          {importResult && (
            <div
              style={{
                marginTop: '1.25rem',
                padding: '1rem',
                borderRadius: '0.75rem',
                background: 'hsla(142, 71%, 45%, 0.12)',
                border: '1px solid hsla(142, 71%, 45%, 0.3)',
                color: '#4ade80',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontWeight: 600, marginBottom: '0.5rem' }}>
                <CheckCircle size={18} />
                <span>Importación y Reconciliación completadas</span>
              </div>
              <div style={{ fontSize: '0.82rem', color: 'var(--text-dim, #d1d5db)', lineHeight: 1.6 }}>
                <div>• Órdenes analizadas: <strong>{importResult.totalOrdersImported}</strong></div>
                <div>• Nuevas órdenes incorporadas: <strong>{importResult.newOrdersAdded}</strong></div>
                <div>• Pedidos duplicados omitidos: <strong>{importResult.skippedDuplicates}</strong></div>
                <div>• Importe total: <strong>{importResult.totalImportedAmount.toFixed(2)} €</strong></div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
