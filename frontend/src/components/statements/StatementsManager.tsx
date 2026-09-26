import React, { useState } from 'react';
import { RotateCcw, AlertCircle, CheckCircle2, Wallet, ArrowDownRight, ArrowUpRight } from 'lucide-react';
import { useAccounts } from '../../hooks/useAccounts';
import { useStatements } from '../../hooks/useStatements';
import { AccountSelector } from './AccountSelector';
import { StatementDropzone } from './StatementDropzone';
import { StatementPreviewModal } from './StatementPreviewModal';
import type { BankAccount } from '../../types/account';
import type { StatementPreview } from '../../types/statement';

export const StatementsManager: React.FC = () => {
  const { accounts, totalNetWorthShare, createAccount, isCreating } = useAccounts();
  const [selectedAccount, setSelectedAccount] = useState<BankAccount | undefined>(accounts[0]);
  const [previewData, setPreviewData] = useState<StatementPreview | null>(null);
  const [successBanner, setSuccessBanner] = useState<string | null>(null);

  // Seleccionar automáticamente la primera cuenta disponible si aún no hay ninguna
  const activeAccount = selectedAccount ?? accounts[0];

  const {
    useTransactions,
    previewStatement,
    isPreviewing,
    previewError,
    confirmImport,
    isConfirming,
    rollbackImport,
    isRollingBack,
  } = useStatements(activeAccount?.id);

  const { data: txData, isLoading: isLoadingTx } = useTransactions(1, 50);

  const handleFileSelect = async (file: File) => {
    if (!activeAccount) return;
    try {
      setSuccessBanner(null);
      const preview = await previewStatement({
        accountId: activeAccount.id,
        file,
      });
      setPreviewData(preview);
    } catch {
      // previewError capturado por React Query
    }
  };

  const handleConfirmImport = async () => {
    if (!activeAccount || !previewData) return;
    try {
      const summary = await confirmImport({
        accountId: activeAccount.id,
        previewToken: previewData.previewToken,
      });
      setPreviewData(null);
      setSuccessBanner(
        `¡Extracto importado con éxito! Se registraron ${summary.importedRecords} transacciones (${summary.skippedDuplicates} duplicados omitidos). Saldo final: ${summary.finalAccountBalance.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €`
      );
    } catch {
      // capturado
    }
  };

  const handleRollback = async (importId: string) => {
    if (window.confirm('¿Estás seguro de que deseas deshacer esta importación? Se eliminarán todas las transacciones del lote y el saldo se restaurará automáticamente.')) {
      try {
        await rollbackImport(importId);
        setSuccessBanner('Importación revertida con éxito. El saldo y los movimientos se han restaurado.');
      } catch (err: unknown) {
        alert(err instanceof Error ? err.message : 'Error al deshacer importación');
      }
    }
  };

  return (
    <div style={{ marginTop: '2.5rem' }}>
      {/* Banner de Patrimonio Consolidado */}
      <div
        style={{
          background: 'linear-gradient(135deg, hsla(222, 47%, 16%, 0.8), hsla(217, 91%, 60%, 0.15))',
          border: '1px solid var(--border-subtle)',
          borderRadius: 'var(--radius-xl)',
          padding: '1.5rem 2rem',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: '2rem',
          boxShadow: 'var(--shadow-glass)',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          <div
            style={{
              width: '48px',
              height: '48px',
              borderRadius: 'var(--radius-lg)',
              background: 'hsla(217, 91%, 60%, 0.2)',
              color: 'var(--primary-500)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <Wallet size={24} />
          </div>
          <div>
            <span style={{ fontSize: '0.8rem', color: 'var(--text-dim)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
              Tu Patrimonio Neto Ponderado (RN-12)
            </span>
            <div style={{ fontSize: '1.75rem', fontWeight: 800, color: 'var(--text-main)' }}>
              {totalNetWorthShare.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €
            </div>
          </div>
        </div>

        <div style={{ textAlign: 'right', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
          Ponderación automática según rol de titularidad
          <br />
          <span style={{ color: 'var(--accent-cyan)' }}>Titular: 100% | Cotitular: % asignado | Apoderado: 0%</span>
        </div>
      </div>

      {/* Selector de Cuentas */}
      <AccountSelector
        accounts={accounts}
        selectedAccountId={activeAccount?.id}
        onSelectAccount={(acc) => {
          setSelectedAccount(acc);
          setSuccessBanner(null);
        }}
        onCreateAccount={createAccount}
        isCreating={isCreating}
      />

      {/* Zona de Carga de Extractos */}
      {activeAccount && (
        <StatementDropzone
          onFileSelect={handleFileSelect}
          isLoading={isPreviewing}
          selectedAccountName={activeAccount.name}
        />
      )}

      {/* Mensajes de Feedback */}
      {previewError && (
        <div
          style={{
            padding: '1rem',
            borderRadius: 'var(--radius-md)',
            background: 'var(--danger-bg)',
            border: '1px solid var(--danger)',
            color: 'var(--danger)',
            display: 'flex',
            alignItems: 'center',
            gap: '0.5rem',
            marginBottom: '1.5rem',
          }}
        >
          <AlertCircle size={18} />
          <span>{previewError.message}</span>
        </div>
      )}

      {successBanner && (
        <div
          style={{
            padding: '1rem',
            borderRadius: 'var(--radius-md)',
            background: 'var(--success-bg)',
            border: '1px solid var(--success)',
            color: 'var(--success)',
            display: 'flex',
            alignItems: 'center',
            gap: '0.5rem',
            marginBottom: '1.5rem',
          }}
        >
          <CheckCircle2 size={18} />
          <span>{successBanner}</span>
        </div>
      )}

      {/* Modal de Previsualización */}
      {previewData && activeAccount && (
        <StatementPreviewModal
          preview={previewData}
          accountName={activeAccount.name}
          onConfirm={handleConfirmImport}
          onCancel={() => setPreviewData(null)}
          isConfirming={isConfirming}
        />
      )}

      {/* Tabla de Movimientos de la Cuenta */}
      {activeAccount && (
        <div
          style={{
            background: 'var(--bg-surface)',
            border: '1px solid var(--border-subtle)',
            borderRadius: 'var(--radius-xl)',
            padding: '1.5rem',
            boxShadow: 'var(--shadow-sm)',
          }}
        >
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <h3 style={{ fontSize: '1.1rem', fontWeight: 600, color: 'var(--text-main)', margin: 0 }}>
              Movimientos Registrados en {activeAccount.name}
            </h3>
            <span style={{ fontSize: '0.8rem', color: 'var(--text-dim)' }}>
              {txData?.totalCount ?? 0} operaciones registradas
            </span>
          </div>

          {isLoadingTx ? (
            <p style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem' }}>Cargando movimientos...</p>
          ) : !txData?.items || txData.items.length === 0 ? (
            <div style={{ textAlign: 'center', padding: '3rem 1rem', color: 'var(--text-muted)' }}>
              <p>Aún no hay extractos importados para esta cuenta.</p>
              <p style={{ fontSize: '0.85rem', color: 'var(--text-dim)' }}>
                Arrastra un fichero arriba para comenzar.
              </p>
            </div>
          ) : (
            <div style={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.85rem' }}>
                <thead>
                  <tr style={{ borderBottom: '1px solid var(--border-subtle)', color: 'var(--text-dim)' }}>
                    <th style={{ padding: '0.75rem 1rem' }}>Fecha</th>
                    <th style={{ padding: '0.75rem 1rem' }}>Concepto</th>
                    <th style={{ padding: '0.75rem 1rem' }}>Categoría Banco</th>
                    <th style={{ padding: '0.75rem 1rem', textAlign: 'right' }}>Importe</th>
                    <th style={{ padding: '0.75rem 1rem', textAlign: 'right' }}>Saldo Disponible</th>
                    <th style={{ padding: '0.75rem 1rem', textAlign: 'center' }}>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {txData.items.map((tx) => (
                    <tr
                      key={tx.id}
                      style={{
                        borderBottom: '1px solid var(--border-glass)',
                        transition: 'background var(--transition-fast)',
                      }}
                    >
                      <td style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)', fontFamily: 'monospace' }}>
                        {tx.operationDate}
                      </td>
                      <td style={{ padding: '0.75rem 1rem', color: 'var(--text-main)', fontWeight: 500 }}>
                        {tx.description}
                        {tx.observations && (
                          <span style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-dim)', marginTop: '0.2rem' }}>
                            {tx.observations}
                          </span>
                        )}
                      </td>
                      <td style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)' }}>
                        {tx.bankCategory ? (
                          <span
                            style={{
                              fontSize: '0.75rem',
                              padding: '0.2rem 0.5rem',
                              borderRadius: 'var(--radius-sm)',
                              background: 'hsla(0,0%,100%,0.06)',
                              color: 'var(--text-muted)',
                            }}
                          >
                            {tx.bankCategory} {tx.bankSubcategory ? `• ${tx.bankSubcategory}` : ''}
                          </span>
                        ) : (
                          '-'
                        )}
                      </td>
                      <td
                        style={{
                          padding: '0.75rem 1rem',
                          textAlign: 'right',
                          fontWeight: 700,
                          color: tx.amount < 0 ? 'var(--danger)' : 'var(--success)',
                        }}
                      >
                        <span style={{ display: 'inline-flex', alignItems: 'center', gap: '0.2rem' }}>
                          {tx.amount < 0 ? <ArrowDownRight size={14} /> : <ArrowUpRight size={14} />}
                          {tx.amount > 0 ? `+${tx.amount.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €` : `${tx.amount.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €`}
                        </span>
                      </td>
                      <td style={{ padding: '0.75rem 1rem', textAlign: 'right', color: 'var(--text-muted)', fontFamily: 'monospace' }}>
                        {tx.balanceAfter !== null && tx.balanceAfter !== undefined
                          ? `${tx.balanceAfter.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €`
                          : '-'}
                      </td>
                      <td style={{ padding: '0.75rem 1rem', textAlign: 'center' }}>
                        <button
                          onClick={() => handleRollback(tx.statementImportId)}
                          disabled={isRollingBack}
                          title="Deshacer el lote de importación correspondiente a este movimiento"
                          style={{
                            background: 'transparent',
                            border: '1px solid var(--border-subtle)',
                            borderRadius: 'var(--radius-sm)',
                            color: 'var(--text-dim)',
                            padding: '0.3rem 0.5rem',
                            cursor: isRollingBack ? 'not-allowed' : 'pointer',
                            display: 'inline-flex',
                            alignItems: 'center',
                            gap: '0.25rem',
                            fontSize: '0.75rem',
                          }}
                        >
                          <RotateCcw size={12} />
                          Deshacer Lote
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
