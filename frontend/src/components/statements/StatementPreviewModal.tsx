import React from 'react';
import { AlertTriangle, CheckCircle, ArrowRight, TrendingUp, TrendingDown, Calendar, Layers } from 'lucide-react';
import type { StatementPreview } from '../../types/statement';

interface StatementPreviewModalProps {
  preview: StatementPreview;
  accountName: string;
  onConfirm: () => void;
  onCancel: () => void;
  isConfirming: boolean;
}

export const StatementPreviewModal: React.FC<StatementPreviewModalProps> = ({
  preview,
  accountName,
  onConfirm,
  onCancel,
  isConfirming,
}) => {
  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        background: 'rgba(0, 0, 0, 0.8)',
        backdropFilter: 'blur(10px)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 1100,
        padding: '1.5rem',
      }}
    >
      <div
        style={{
          background: 'hsl(222, 47%, 11%)',
          border: '1px solid var(--border-subtle)',
          borderRadius: 'var(--radius-xl)',
          maxWidth: '850px',
          width: '100%',
          maxHeight: '90vh',
          display: 'flex',
          flexDirection: 'column',
          boxShadow: 'var(--shadow-glass)',
          overflow: 'hidden',
        }}
      >
        {/* Header */}
        <div
          style={{
            padding: '1.5rem 2rem',
            borderBottom: '1px solid var(--border-subtle)',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            background: 'var(--bg-surface-elevated)',
          }}
        >
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.25rem' }}>
              <span
                style={{
                  fontSize: '0.75rem',
                  fontWeight: 700,
                  textTransform: 'uppercase',
                  padding: '0.2rem 0.6rem',
                  borderRadius: 'var(--radius-full)',
                  background: 'hsla(217, 91%, 60%, 0.15)',
                  color: 'var(--primary-500)',
                }}
              >
                {preview.detectedBankName} Detectado
              </span>
              <h2 style={{ fontSize: '1.25rem', fontWeight: 700, color: 'var(--text-main)', margin: 0 }}>
                Vista Previa del Extracto
              </h2>
            </div>
            <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', margin: 0 }}>
              Destino: <strong>{accountName}</strong> (0 escrituras en base de datos hasta que confirmes)
            </p>
          </div>
        </div>

        {/* Content Body (Scrollable) */}
        <div style={{ padding: '2rem', overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
          {/* Alertas de Coherencia (RN-09 y RN-10) */}
          {preview.warnings && preview.warnings.length > 0 && (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              {preview.warnings.map((w, idx) => (
                <div
                  key={idx}
                  style={{
                    padding: '1rem',
                    borderRadius: 'var(--radius-md)',
                    background: 'var(--warning-bg)',
                    border: '1px solid var(--warning)',
                    display: 'flex',
                    alignItems: 'flex-start',
                    gap: '0.75rem',
                  }}
                >
                  <AlertTriangle size={20} style={{ color: 'var(--warning)', flexShrink: 0, marginTop: '2px' }} />
                  <div>
                    <strong style={{ display: 'block', color: 'var(--warning)', fontSize: '0.85rem', marginBottom: '0.25rem' }}>
                      Atención: Salvaguarda de Seguridad Detectada
                    </strong>
                    <span style={{ fontSize: '0.82rem', color: 'var(--text-main)', lineHeight: 1.4 }}>
                      {w}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Tarjetas de Métricas de Extracto */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: '1rem' }}>
            <div
              style={{
                padding: '1rem',
                borderRadius: 'var(--radius-lg)',
                background: 'var(--bg-surface)',
                border: '1px solid var(--border-subtle)',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--text-dim)', fontSize: '0.75rem', marginBottom: '0.5rem' }}>
                <Layers size={14} />
                <span>Evolución de Saldo</span>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <span style={{ fontSize: '1rem', fontWeight: 600, color: 'var(--text-muted)' }}>
                  {preview.startingBalance.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €
                </span>
                <ArrowRight size={14} style={{ color: 'var(--text-dim)' }} />
                <span style={{ fontSize: '1.2rem', fontWeight: 700, color: 'var(--text-main)' }}>
                  {preview.endingBalance.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €
                </span>
              </div>
            </div>

            <div
              style={{
                padding: '1rem',
                borderRadius: 'var(--radius-lg)',
                background: 'var(--bg-surface)',
                border: '1px solid var(--border-subtle)',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--text-dim)', fontSize: '0.75rem', marginBottom: '0.5rem' }}>
                <Calendar size={14} />
                <span>Período ({preview.totalRecords} movs)</span>
              </div>
              <span style={{ fontSize: '0.95rem', fontWeight: 600, color: 'var(--text-main)' }}>
                {preview.startDate} ➔ {preview.endDate}
              </span>
            </div>

            <div
              style={{
                padding: '1rem',
                borderRadius: 'var(--radius-lg)',
                background: 'var(--bg-surface)',
                border: '1px solid var(--border-subtle)',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--success)', fontSize: '0.75rem', marginBottom: '0.5rem' }}>
                <TrendingUp size={14} />
                <span>Total Ingresos</span>
              </div>
              <span style={{ fontSize: '1.15rem', fontWeight: 700, color: 'var(--success)' }}>
                +{preview.totalIncome.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €
              </span>
            </div>

            <div
              style={{
                padding: '1rem',
                borderRadius: 'var(--radius-lg)',
                background: 'var(--bg-surface)',
                border: '1px solid var(--border-subtle)',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--danger)', fontSize: '0.75rem', marginBottom: '0.5rem' }}>
                <TrendingDown size={14} />
                <span>Total Gastos</span>
              </div>
              <span style={{ fontSize: '1.15rem', fontWeight: 700, color: 'var(--danger)' }}>
                -{preview.totalExpense.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €
              </span>
            </div>
          </div>

          {/* Tabla de Primeros Movimientos */}
          <div>
            <h4 style={{ fontSize: '0.9rem', fontWeight: 600, color: 'var(--text-muted)', margin: '0 0 0.75rem' }}>
              Primeras transacciones detectadas (PANs enmascarados y datos normalizados):
            </h4>
            <div
              style={{
                border: '1px solid var(--border-subtle)',
                borderRadius: 'var(--radius-md)',
                overflow: 'hidden',
                background: 'var(--bg-surface)',
              }}
            >
              <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.85rem' }}>
                <thead>
                  <tr style={{ background: 'var(--bg-surface-elevated)', borderBottom: '1px solid var(--border-subtle)', color: 'var(--text-dim)' }}>
                    <th style={{ padding: '0.65rem 1rem' }}>Fecha</th>
                    <th style={{ padding: '0.65rem 1rem' }}>Concepto</th>
                    <th style={{ padding: '0.65rem 1rem', textAlign: 'right' }}>Importe</th>
                    <th style={{ padding: '0.65rem 1rem', textAlign: 'right' }}>Saldo Posterior</th>
                  </tr>
                </thead>
                <tbody>
                  {preview.previewRows.map((row, idx) => (
                    <tr
                      key={idx}
                      style={{
                        borderBottom: idx < preview.previewRows.length - 1 ? '1px solid var(--border-glass)' : 'none',
                      }}
                    >
                      <td style={{ padding: '0.65rem 1rem', color: 'var(--text-muted)', fontFamily: 'monospace' }}>
                        {row.operationDate}
                      </td>
                      <td style={{ padding: '0.65rem 1rem', color: 'var(--text-main)', fontWeight: 500 }}>
                        {row.description}
                        {row.bankCategory && (
                          <span
                            style={{
                              marginLeft: '0.5rem',
                              fontSize: '0.7rem',
                              padding: '0.1rem 0.4rem',
                              borderRadius: 'var(--radius-sm)',
                              background: 'hsla(0,0%,100%,0.06)',
                              color: 'var(--text-dim)',
                            }}
                          >
                            {row.bankCategory}
                          </span>
                        )}
                      </td>
                      <td
                        style={{
                          padding: '0.65rem 1rem',
                          textAlign: 'right',
                          fontWeight: 700,
                          color: row.amount < 0 ? 'var(--danger)' : 'var(--success)',
                        }}
                      >
                        {row.amount > 0 ? `+${row.amount.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €` : `${row.amount.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €`}
                      </td>
                      <td style={{ padding: '0.65rem 1rem', textAlign: 'right', color: 'var(--text-muted)', fontFamily: 'monospace' }}>
                        {row.balanceAfter !== null && row.balanceAfter !== undefined
                          ? `${row.balanceAfter.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €`
                          : '-'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>

        {/* Footer Actions */}
        <div
          style={{
            padding: '1.25rem 2rem',
            borderTop: '1px solid var(--border-subtle)',
            display: 'flex',
            justifyContent: 'flex-end',
            gap: '1rem',
            background: 'var(--bg-surface-elevated)',
          }}
        >
          <button
            onClick={onCancel}
            disabled={isConfirming}
            style={{
              padding: '0.65rem 1.25rem',
              borderRadius: 'var(--radius-md)',
              border: '1px solid var(--border-subtle)',
              background: 'transparent',
              color: 'var(--text-muted)',
              fontWeight: 600,
              cursor: isConfirming ? 'not-allowed' : 'pointer',
            }}
          >
            Cancelar
          </button>

          <button
            onClick={onConfirm}
            disabled={isConfirming}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '0.5rem',
              padding: '0.65rem 1.5rem',
              borderRadius: 'var(--radius-md)',
              border: 'none',
              background: 'var(--primary-500)',
              color: '#fff',
              fontWeight: 600,
              cursor: isConfirming ? 'not-allowed' : 'pointer',
              boxShadow: 'var(--shadow-glow)',
            }}
          >
            <CheckCircle size={18} />
            {isConfirming ? 'Importando Lote...' : 'Confirmar e Importar (1 Clic)'}
          </button>
        </div>
      </div>
    </div>
  );
};
