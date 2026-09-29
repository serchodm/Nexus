import React, { useState } from 'react';
import { 
  AlertCircle, 
  CheckCircle2, 
  Wallet, 
  Search, 
  Sliders, 
  Repeat, 
  Layers,
  ShoppingBag,
  Sparkles
} from 'lucide-react';
import { useAccounts } from '../../hooks/useAccounts';
import { useStatements } from '../../hooks/useStatements';
import { useCategories } from '../../hooks/useCategories';
import { useTransactions } from '../../hooks/useTransactions';
import { useCommerce } from '../../hooks/useCommerce';
import { AccountSelector } from './AccountSelector';
import { StatementDropzone } from './StatementDropzone';
import { StatementPreviewModal } from './StatementPreviewModal';
import { TransactionRow } from '../transactions/TransactionRow';
import { SplitTransactionModal } from '../transactions/SplitTransactionModal';
import { CategorizeModal } from '../transactions/CategorizeModal';
import { RulesManagerModal } from '../rules/RulesManagerModal';
import { CommerceIntegrationModal } from '../commerce/CommerceIntegrationModal';
import { ReconciliationReviewModal } from '../commerce/ReconciliationReviewModal';
import type { BankAccount } from '../../types/account';
import type { StatementPreview } from '../../types/statement';
import type { TransactionDetail, SplitItemInput, CategorizeTransactionRequest } from '../../types/transaction';
import type { ReconciliationCandidate } from '../../types/commerce';

export const StatementsManager: React.FC = () => {
  const { accounts, totalNetWorthShare, createAccount, isCreating } = useAccounts();
  const [selectedAccount, setSelectedAccount] = useState<BankAccount | undefined>(accounts[0]);
  const [previewData, setPreviewData] = useState<StatementPreview | null>(null);
  const [successBanner, setSuccessBanner] = useState<string | null>(null);

  // Filtros de transacciones enriquecidas
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategoryFilter, setSelectedCategoryFilter] = useState<string>('');
  const [onlyRecurring, setOnlyRecurring] = useState<boolean | undefined>(undefined);
  const [onlySplits, setOnlySplits] = useState<boolean | undefined>(undefined);
  const [onlyReconcilable, setOnlyReconcilable] = useState<boolean>(false);

  // Estados de modales
  const [splitTx, setSplitTx] = useState<TransactionDetail | null>(null);
  const [categorizeTx, setCategorizeTx] = useState<TransactionDetail | null>(null);
  const [isRulesModalOpen, setIsRulesModalOpen] = useState(false);
  const [isCommerceModalOpen, setIsCommerceModalOpen] = useState(false);
  const [reviewCandidate, setReviewCandidate] = useState<ReconciliationCandidate | null>(null);

  const {
    reconciliations,
  } = useCommerce();

  // Seleccionar automáticamente la primera cuenta disponible si aún no hay ninguna
  const activeAccount = selectedAccount ?? accounts[0];

  const {
    previewStatement,
    isPreviewing,
    previewError,
    confirmImport,
    isConfirming,
  } = useStatements(activeAccount?.id);

  const { categoriesTree } = useCategories();

  const {
    transactions,
    totalCount,
    isLoading: isLoadingTx,
    categorizeTransaction,
    isCategorizing,
    splitTransaction,
    isSplitting,
    resetSplits,
    isResettingSplits,
  } = useTransactions({
    bankAccountId: activeAccount?.id,
    searchTerm: searchTerm.trim() || undefined,
    categoryId: selectedCategoryFilter || undefined,
    isRecurring: onlyRecurring,
    hasSplits: onlySplits,
    pageSize: 100,
  });

  const displayedTransactions = onlyReconcilable
    ? transactions.filter((tx) => reconciliations.some((r) => r.bankTransactionId === tx.id))
    : transactions;

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
        `¡Extracto importado con éxito! Se registraron ${summary.importedRecords} transacciones (${summary.skippedDuplicates} duplicados omitidos). El motor categorizó automáticamente comercios, suscripciones y Bizums.`
      );
    } catch {
      // capturado
    }
  };


  const handleSaveSplit = async (items: SplitItemInput[]) => {
    if (!splitTx) return;
    await splitTransaction({
      id: splitTx.id,
      data: { items },
    });
    setSuccessBanner('Desglose de transacción guardado con éxito respetando el invariante contable.');
  };

  const handleResetSplits = async () => {
    if (!splitTx) return;
    await resetSplits(splitTx.id);
    setSuccessBanner('Desglose eliminado con éxito.');
  };

  const handleSaveCategorize = async (data: CategorizeTransactionRequest) => {
    if (!categorizeTx) return;
    await categorizeTransaction({
      id: categorizeTx.id,
      data,
    });
    setSuccessBanner('Categorización manual aplicada con éxito.');
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
              Patrimonio Neto Ponderado (Mi Participación)
            </span>
            <div style={{ fontSize: '1.75rem', fontWeight: 800, color: 'var(--text-main)', letterSpacing: '-0.02em' }}>
              {totalNetWorthShare.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €
            </div>
          </div>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <button
            onClick={() => setIsCommerceModalOpen(true)}
            className="btn-secondary"
            style={{
              display: 'inline-flex',
              alignItems: 'center',
              gap: '0.5rem',
              fontSize: '0.85rem',
              position: 'relative',
              borderColor: reconciliations.length > 0 ? 'hsla(38, 92%, 50%, 0.5)' : undefined,
            }}
          >
            <ShoppingBag size={16} color={reconciliations.length > 0 ? '#fbbf24' : 'currentColor'} />
            <span>Amazon & PayPal</span>
            {reconciliations.length > 0 && (
              <span
                title={`${reconciliations.length} pedidos listos para reconciliar`}
                style={{
                  background: 'linear-gradient(135deg, #f59e0b 0%, #3b82f6 100%)',
                  color: '#ffffff',
                  borderRadius: '9999px',
                  padding: '0.1rem 0.5rem',
                  fontSize: '0.7rem',
                  fontWeight: 800,
                  boxShadow: '0 2px 4px rgba(0,0,0,0.3)',
                }}
              >
                {reconciliations.length}
              </span>
            )}
          </button>

          <button
            onClick={() => setIsRulesModalOpen(true)}
            className="btn-secondary"
            style={{ display: 'inline-flex', alignItems: 'center', gap: '0.45rem', fontSize: '0.85rem' }}
          >
            <Sliders size={16} />
            Reglas & Automatización
          </button>
        </div>
      </div>

      {/* Banner de Éxito / Feedback */}
      {successBanner && (
        <div
          style={{
            background: 'var(--success-bg)',
            border: '1px solid var(--success)',
            color: 'var(--success)',
            borderRadius: 'var(--radius-md)',
            padding: '1rem 1.25rem',
            display: 'flex',
            alignItems: 'center',
            gap: '0.75rem',
            marginBottom: '1.5rem',
            fontSize: '0.9rem',
          }}
        >
          <CheckCircle2 size={18} />
          <div style={{ flex: 1 }}>{successBanner}</div>
          <button
            onClick={() => setSuccessBanner(null)}
            style={{ background: 'transparent', border: 'none', color: 'var(--success)', cursor: 'pointer' }}
          >
            ✕
          </button>
        </div>
      )}

      {/* Selector de Cuentas */}
      <div style={{ marginBottom: '2rem' }}>
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
      </div>

      {/* Zona de Arrastre / Ingesta de Ficheros */}
      {activeAccount && (
        <div style={{ marginBottom: '2.5rem' }}>
          <StatementDropzone
            selectedAccountName={activeAccount.name}
            onFileSelect={handleFileSelect}
            isLoading={isPreviewing}
          />
          {previewError && (
            <div
              style={{
                marginTop: '1rem',
                padding: '0.75rem 1rem',
                background: 'var(--danger-bg)',
                border: '1px solid var(--danger)',
                borderRadius: 'var(--radius-sm)',
                color: 'var(--danger)',
                display: 'flex',
                alignItems: 'center',
                gap: '0.5rem',
                fontSize: '0.85rem',
              }}
            >
              <AlertCircle size={16} />
              <span>{previewError instanceof Error ? previewError.message : 'Error al procesar el extracto bancario.'}</span>
            </div>
          )}
        </div>
      )}

      {/* Modal de Previsualización */}
      {previewData && (
        <StatementPreviewModal
          preview={previewData}
          accountName={activeAccount?.name ?? ''}
          onCancel={() => setPreviewData(null)}
          onConfirm={handleConfirmImport}
          isConfirming={isConfirming}
        />
      )}

      {/* Modales de Slice 002 */}
      {splitTx && (
        <SplitTransactionModal
          transaction={splitTx}
          categories={categoriesTree}
          onClose={() => setSplitTx(null)}
          onSave={handleSaveSplit}
          onResetSplits={splitTx.hasSplits ? handleResetSplits : undefined}
          isSaving={isSplitting}
          isResetting={isResettingSplits}
        />
      )}

      {categorizeTx && (
        <CategorizeModal
          transaction={categorizeTx}
          categories={categoriesTree}
          onClose={() => setCategorizeTx(null)}
          onSave={handleSaveCategorize}
          isSaving={isCategorizing}
        />
      )}

      {isRulesModalOpen && (
        <RulesManagerModal
          categories={categoriesTree}
          onClose={() => setIsRulesModalOpen(false)}
        />
      )}

      {/* Modales de Slice 003: Commerce & Reconciliación */}
      <CommerceIntegrationModal
        isOpen={isCommerceModalOpen}
        onClose={() => setIsCommerceModalOpen(false)}
      />

      {reviewCandidate && (
        <ReconciliationReviewModal
          isOpen={!!reviewCandidate}
          candidate={reviewCandidate}
          onClose={() => setReviewCandidate(null)}
          onReconciliationAccepted={() => {
            setSuccessBanner('¡Pedido reconciliado con éxito! Se desglosó el movimiento contable respetando los artículos del pedido.');
          }}
        />
      )}

      {/* Histórico Enriquecido de Movimientos */}
      {activeAccount && (
        <div
          className="glass-card"
          style={{
            padding: '1.75rem',
            boxShadow: 'var(--shadow-sm)',
          }}
        >
          {/* Header con Buscador y Filtros */}
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem', flexWrap: 'wrap', gap: '1rem' }}>
            <div>
              <h3 style={{ fontSize: '1.2rem', fontWeight: 600, color: 'var(--text-main)', margin: 0 }}>
                Movimientos Enriquecidos • {activeAccount.name}
              </h3>
              <span style={{ fontSize: '0.8rem', color: 'var(--text-dim)' }}>
                {totalCount} movimientos encontrados • Merchant Intelligence & Desgloses
              </span>
            </div>

            {/* Barra de Filtros */}
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', flexWrap: 'wrap' }}>
              {/* Buscador de Comercio o Concepto */}
              <div
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.4rem',
                  padding: '0.4rem 0.75rem',
                  background: 'var(--bg-app)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-md)',
                }}
              >
                <Search size={14} color="var(--text-dim)" />
                <input
                  type="text"
                  placeholder="Buscar comercio o concepto..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  style={{
                    background: 'transparent',
                    border: 'none',
                    outline: 'none',
                    color: 'var(--text-main)',
                    fontSize: '0.82rem',
                    width: '180px',
                  }}
                />
              </div>

              {/* Selector de Categoría */}
              <select
                value={selectedCategoryFilter}
                onChange={(e) => setSelectedCategoryFilter(e.target.value)}
                style={{
                  padding: '0.45rem 0.75rem',
                  background: 'var(--bg-app)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-md)',
                  color: 'var(--text-main)',
                  fontSize: '0.82rem',
                }}
              >
                <option value="">Todas las categorías</option>
                {categoriesTree.map((cat) => (
                  <option key={cat.id} value={cat.id}>
                    {cat.name}
                  </option>
                ))}
              </select>

              {/* Filtro Suscripciones */}
              <button
                type="button"
                onClick={() => setOnlyRecurring(onlyRecurring ? undefined : true)}
                style={{
                  padding: '0.45rem 0.75rem',
                  borderRadius: 'var(--radius-md)',
                  background: onlyRecurring ? 'hsla(263, 70%, 50%, 0.2)' : 'var(--bg-app)',
                  border: `1px solid ${onlyRecurring ? 'var(--primary-500)' : 'var(--border-subtle)'}`,
                  color: onlyRecurring ? 'var(--primary-500)' : 'var(--text-dim)',
                  fontSize: '0.8rem',
                  fontWeight: 600,
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.35rem',
                  cursor: 'pointer',
                }}
              >
                <Repeat size={13} />
                Suscripciones
              </button>

              {/* Filtro Splits */}
              <button
                type="button"
                onClick={() => setOnlySplits(onlySplits ? undefined : true)}
                style={{
                  padding: '0.45rem 0.75rem',
                  borderRadius: 'var(--radius-md)',
                  background: onlySplits ? 'hsla(217, 91%, 60%, 0.2)' : 'var(--bg-app)',
                  border: `1px solid ${onlySplits ? 'var(--primary-500)' : 'var(--border-subtle)'}`,
                  color: onlySplits ? 'var(--primary-500)' : 'var(--text-dim)',
                  fontSize: '0.8rem',
                  fontWeight: 600,
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.35rem',
                  cursor: 'pointer',
                }}
              >
                <Layers size={13} />
                Solo Splits
              </button>

              {/* Filtro Reconciliables */}
              <button
                type="button"
                onClick={() => setOnlyReconcilable(!onlyReconcilable)}
                style={{
                  padding: '0.45rem 0.75rem',
                  borderRadius: 'var(--radius-md)',
                  background: onlyReconcilable ? 'hsla(38, 92%, 50%, 0.2)' : 'var(--bg-app)',
                  border: `1px solid ${onlyReconcilable ? 'hsla(38, 92%, 50%, 0.5)' : 'var(--border-subtle)'}`,
                  color: onlyReconcilable ? '#fbbf24' : 'var(--text-dim)',
                  fontSize: '0.8rem',
                  fontWeight: 600,
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.35rem',
                  cursor: 'pointer',
                }}
              >
                <Sparkles size={13} />
                <span>Reconciliables</span>
                {reconciliations.length > 0 && (
                  <span
                    style={{
                      background: 'hsla(38, 92%, 50%, 0.3)',
                      color: '#fbbf24',
                      borderRadius: 'var(--radius-sm)',
                      padding: '0.05rem 0.35rem',
                      fontSize: '0.68rem',
                      fontWeight: 700,
                    }}
                  >
                    {reconciliations.length}
                  </span>
                )}
              </button>
            </div>
          </div>

          {isLoadingTx ? (
            <p style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem' }}>Cargando movimientos enriquecidos...</p>
          ) : !transactions || transactions.length === 0 ? (
            <div style={{ textAlign: 'center', padding: '3rem 1rem', color: 'var(--text-muted)' }}>
              <p>No se encontraron movimientos con los filtros aplicados.</p>
              <p style={{ fontSize: '0.85rem', color: 'var(--text-dim)' }}>
                Arrastra un extracto bancario para importar nuevos datos.
              </p>
            </div>
          ) : (
            <div style={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.85rem' }}>
                <thead>
                  <tr style={{ borderBottom: '1px solid var(--border-subtle)', color: 'var(--text-dim)' }}>
                    <th style={{ padding: '0.75rem 1rem' }}>Fecha</th>
                    <th style={{ padding: '0.75rem 1rem' }}>Comercio Limpio & Concepto</th>
                    <th style={{ padding: '0.75rem 1rem' }}>Categoría / Desglose</th>
                    <th style={{ padding: '0.75rem 1rem', textAlign: 'right' }}>Importe</th>
                    <th style={{ padding: '0.75rem 1rem', textAlign: 'right' }}>Saldo Disponible</th>
                    <th style={{ padding: '0.75rem 1rem', textAlign: 'center' }}>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {displayedTransactions.map((tx) => {
                    const candidate = reconciliations.find((r) => r.bankTransactionId === tx.id);
                    return (
                      <TransactionRow
                        key={tx.id}
                        transaction={tx}
                        onOpenCategorize={(item) => setCategorizeTx(item)}
                        onOpenSplit={(item) => setSplitTx(item)}
                        reconciliationCandidate={candidate}
                        onReviewReconciliation={(cand) => setReviewCandidate(cand)}
                      />
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
