import React, { useState } from 'react';
import { Building2, Plus, Check, Shield, Users, Briefcase, HeartHandshake } from 'lucide-react';
import type { BankAccount, CreateBankAccountRequest, HolderRole } from '../../types/account';
import { HolderRoleLabels } from '../../types/account';

interface AccountSelectorProps {
  accounts: BankAccount[];
  selectedAccountId?: string;
  onSelectAccount: (account: BankAccount) => void;
  onCreateAccount: (req: CreateBankAccountRequest) => Promise<unknown>;
  isCreating?: boolean;
}

export const AccountSelector: React.FC<AccountSelectorProps> = ({
  accounts,
  selectedAccountId,
  onSelectAccount,
  onCreateAccount,
  isCreating = false,
}) => {
  const [showModal, setShowModal] = useState(false);
  const [name, setName] = useState('');
  const [iban, setIban] = useState('');
  const [type, setType] = useState<number>(1);
  const [holderRole, setHolderRole] = useState<HolderRole>(1);
  const [ownershipPercentage, setOwnershipPercentage] = useState<number>(100);
  const [cardsInput, setCardsInput] = useState('');

  // Autodetección visual de entidad por código de 4 dígitos
  const getDetectedBank = (ibanStr: string) => {
    const clean = ibanStr.replace(/\s+/g, '').toUpperCase();
    if (clean.startsWith('ES') && clean.length >= 8) {
      const code = clean.substring(4, 8);
      if (code === '0182') return { name: 'BBVA', color: 'hsl(217, 91%, 60%)' };
      if (code === '1465') return { name: 'ING Direct', color: 'hsl(25, 95%, 53%)' };
      if (code === '0049') return { name: 'Banco Santander', color: 'hsl(354, 70%, 54%)' };
      if (code === '2100') return { name: 'CaixaBank', color: 'hsl(199, 89%, 48%)' };
      return { name: `Entidad ${code}`, color: 'var(--accent-cyan)' };
    }
    return null;
  };

  const detectedBank = getDetectedBank(iban);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    const cards = cardsInput
      .split(',')
      .map((c) => c.trim())
      .filter((c) => c.length === 4);

    await onCreateAccount({
      name: name.trim(),
      iban: iban.trim() || null,
      type: type as 1 | 2 | 3,
      holderRole,
      ownershipPercentage: holderRole === 2 ? ownershipPercentage : (holderRole === 3 || holderRole === 4) ? 0 : 100,
      linkedCardsLast4: cards.length > 0 ? cards : null,
    });

    setName('');
    setIban('');
    setCardsInput('');
    setShowModal(false);
  };

  const getRoleIcon = (role: HolderRole) => {
    switch (role) {
      case 1:
        return <Shield size={14} />;
      case 2:
        return <Users size={14} />;
      case 3:
        return <Briefcase size={14} />;
      case 4:
        return <HeartHandshake size={14} />;
    }
  };

  return (
    <div style={{ marginBottom: '2rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <div>
          <h2 style={{ fontSize: '1.25rem', fontWeight: 600, color: 'var(--text-main)', margin: 0 }}>
            Tus Cuentas Bancarias
          </h2>
          <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', margin: '0.25rem 0 0' }}>
            Selecciona la cuenta sobre la que deseas operar o subir extractos
          </p>
        </div>
        <button
          onClick={() => setShowModal(true)}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '0.5rem',
            background: 'hsla(217, 91%, 60%, 0.15)',
            color: 'var(--primary-500)',
            border: '1px solid hsla(217, 91%, 60%, 0.3)',
            borderRadius: 'var(--radius-md)',
            padding: '0.5rem 1rem',
            fontSize: '0.85rem',
            fontWeight: 600,
            cursor: 'pointer',
            transition: 'all var(--transition-fast)',
          }}
        >
          <Plus size={16} />
          Añadir Cuenta
        </button>
      </div>

      {accounts.length === 0 ? (
        <div
          style={{
            padding: '2.5rem',
            textAlign: 'center',
            background: 'var(--bg-surface)',
            border: '1px dashed var(--border-subtle)',
            borderRadius: 'var(--radius-lg)',
          }}
        >
          <Building2 size={36} style={{ color: 'var(--text-dim)', marginBottom: '0.75rem' }} />
          <p style={{ color: 'var(--text-muted)', marginBottom: '1rem' }}>
            Aún no has registrado ninguna cuenta bancaria.
          </p>
          <button
            onClick={() => setShowModal(true)}
            style={{
              background: 'var(--primary-500)',
              color: '#fff',
              border: 'none',
              padding: '0.6rem 1.25rem',
              borderRadius: 'var(--radius-md)',
              fontWeight: 600,
              cursor: 'pointer',
            }}
          >
            Registrar Primera Cuenta
          </button>
        </div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '1rem' }}>
          {accounts.map((acc) => {
            const isSelected = acc.id === selectedAccountId;
            return (
              <div
                key={acc.id}
                onClick={() => onSelectAccount(acc)}
                style={{
                  padding: '1.25rem',
                  borderRadius: 'var(--radius-lg)',
                  background: isSelected ? 'var(--bg-surface-elevated)' : 'var(--bg-surface)',
                  border: isSelected ? '2px solid var(--primary-500)' : '1px solid var(--border-subtle)',
                  cursor: 'pointer',
                  position: 'relative',
                  transition: 'all var(--transition-normal)',
                  boxShadow: isSelected ? 'var(--shadow-glow)' : 'var(--shadow-sm)',
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
                  <span
                    style={{
                      fontSize: '0.75rem',
                      fontWeight: 700,
                      textTransform: 'uppercase',
                      letterSpacing: '0.05em',
                      padding: '0.2rem 0.6rem',
                      borderRadius: 'var(--radius-full)',
                      background: 'hsla(217, 91%, 60%, 0.12)',
                      color: 'var(--primary-500)',
                    }}
                  >
                    {acc.bankName}
                  </span>

                  {isSelected && (
                    <span
                      style={{
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        width: '20px',
                        height: '20px',
                        borderRadius: '50%',
                        background: 'var(--primary-500)',
                        color: '#fff',
                      }}
                    >
                      <Check size={12} strokeWidth={3} />
                    </span>
                  )}
                </div>

                <h3 style={{ fontSize: '1.05rem', fontWeight: 600, color: 'var(--text-main)', margin: '0 0 0.25rem' }}>
                  {acc.name}
                </h3>
                <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)', margin: '0 0 1rem', fontFamily: 'monospace' }}>
                  {acc.ibanMasked || (acc.iban ? `**** ${acc.last4Digits}` : `Cuenta terminada en ${acc.last4Digits}`)}
                </p>

                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end' }}>
                  <div>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)', display: 'block' }}>Saldo actual</span>
                    <span
                      style={{
                        fontSize: '1.25rem',
                        fontWeight: 700,
                        color: (acc.currentBalance ?? 0) >= 0 ? 'var(--text-main)' : 'var(--danger)',
                      }}
                    >
                      {acc.currentBalance !== null && acc.currentBalance !== undefined
                        ? `${acc.currentBalance.toLocaleString('es-ES', { minimumFractionDigits: 2 })} €`
                        : 'Sin extracto'}
                    </span>
                  </div>

                  <span
                    style={{
                      display: 'inline-flex',
                      alignItems: 'center',
                      gap: '0.35rem',
                      fontSize: '0.75rem',
                      fontWeight: 600,
                      padding: '0.25rem 0.5rem',
                      borderRadius: 'var(--radius-sm)',
                      background: 'hsla(222, 47%, 20%, 0.8)',
                      color: 'var(--text-muted)',
                      border: '1px solid var(--border-glass)',
                    }}
                  >
                    {getRoleIcon(acc.holderRole)}
                    {acc.holderRole === 2
                      ? `Cotitular ${acc.ownershipPercentage}%`
                      : HolderRoleLabels[acc.holderRole]}
                  </span>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Modal para Crear Cuenta */}
      {showModal && (
        <div
          style={{
            position: 'fixed',
            inset: 0,
            background: 'rgba(0, 0, 0, 0.75)',
            backdropFilter: 'blur(8px)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 1000,
            padding: '1rem',
          }}
        >
          <div
            style={{
              background: 'hsl(222, 47%, 11%)',
              border: '1px solid var(--border-subtle)',
              borderRadius: 'var(--radius-xl)',
              maxWidth: '500px',
              width: '100%',
              padding: '2rem',
              boxShadow: 'var(--shadow-glass)',
            }}
          >
            <h3 style={{ fontSize: '1.25rem', fontWeight: 600, color: 'var(--text-main)', margin: '0 0 0.5rem' }}>
              Registrar Nueva Cuenta
            </h3>
            <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', margin: '0 0 1.5rem' }}>
              Autodetectaremos la entidad financiera mediante los 4 primeros dígitos de tu IBAN.
            </p>

            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.35rem' }}>
                  Nombre / Alias de la Cuenta *
                </label>
                <input
                  type="text"
                  placeholder="ej. BBVA Gastos Comunes, Ahorro Hija..."
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  required
                  style={{
                    width: '100%',
                    padding: '0.65rem 0.85rem',
                    background: 'var(--bg-surface-elevated)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-md)',
                    color: 'var(--text-main)',
                    fontSize: '0.9rem',
                    outline: 'none',
                  }}
                />
              </div>

              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.35rem' }}>
                  <label style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>IBAN Español</label>
                  {detectedBank && (
                    <span
                      style={{
                        fontSize: '0.75rem',
                        fontWeight: 700,
                        color: detectedBank.color,
                        background: 'hsla(0,0%,100%,0.06)',
                        padding: '0.15rem 0.5rem',
                        borderRadius: 'var(--radius-sm)',
                      }}
                    >
                      Entidad: {detectedBank.name}
                    </span>
                  )}
                </div>
                <input
                  type="text"
                  placeholder="ESxx 0182 xxxx..."
                  value={iban}
                  onChange={(e) => setIban(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '0.65rem 0.85rem',
                    background: 'var(--bg-surface-elevated)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-md)',
                    color: 'var(--text-main)',
                    fontSize: '0.9rem',
                    fontFamily: 'monospace',
                    outline: 'none',
                  }}
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.35rem' }}>
                    Tipo de Cuenta
                  </label>
                  <select
                    value={type}
                    onChange={(e) => setType(Number(e.target.value))}
                    style={{
                      width: '100%',
                      padding: '0.65rem 0.85rem',
                      background: 'var(--bg-surface-elevated)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-md)',
                      color: 'var(--text-main)',
                      fontSize: '0.9rem',
                      outline: 'none',
                    }}
                  >
                    <option value={1}>Corriente (Checking)</option>
                    <option value={2}>Ahorro (Savings)</option>
                    <option value={3}>Inversión (Investment)</option>
                  </select>
                </div>

                <div>
                  <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.35rem' }}>
                    Rol de Titularidad
                  </label>
                  <select
                    value={holderRole}
                    onChange={(e) => setHolderRole(Number(e.target.value) as HolderRole)}
                    style={{
                      width: '100%',
                      padding: '0.65rem 0.85rem',
                      background: 'var(--bg-surface-elevated)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-md)',
                      color: 'var(--text-main)',
                      fontSize: '0.9rem',
                      outline: 'none',
                    }}
                  >
                    <option value={1}>Titular Único (100%)</option>
                    <option value={2}>Cotitular (Compartida)</option>
                    <option value={3}>Apoderado / Autorizado (0%)</option>
                    <option value={4}>Tutor / Custodia Menor</option>
                  </select>
                </div>
              </div>

              {holderRole === 2 && (
                <div>
                  <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.35rem' }}>
                    % de Participación Patrimonial (ej. 50%)
                  </label>
                  <input
                    type="number"
                    min="1"
                    max="99"
                    value={ownershipPercentage}
                    onChange={(e) => setOwnershipPercentage(Number(e.target.value))}
                    style={{
                      width: '100%',
                      padding: '0.65rem 0.85rem',
                      background: 'var(--bg-surface-elevated)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-md)',
                      color: 'var(--text-main)',
                      fontSize: '0.9rem',
                      outline: 'none',
                    }}
                  />
                </div>
              )}

              <div>
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.35rem' }}>
                  Tarjetas Vinculadas (últimos 4 dígitos, separados por comas)
                </label>
                <input
                  type="text"
                  placeholder="ej. 6589, 4321"
                  value={cardsInput}
                  onChange={(e) => setCardsInput(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '0.65rem 0.85rem',
                    background: 'var(--bg-surface-elevated)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-md)',
                    color: 'var(--text-main)',
                    fontSize: '0.9rem',
                    outline: 'none',
                  }}
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.75rem', marginTop: '1rem' }}>
                <button
                  type="button"
                  onClick={() => setShowModal(false)}
                  style={{
                    background: 'transparent',
                    border: '1px solid var(--border-subtle)',
                    color: 'var(--text-muted)',
                    padding: '0.6rem 1.25rem',
                    borderRadius: 'var(--radius-md)',
                    fontWeight: 600,
                    cursor: 'pointer',
                  }}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={isCreating}
                  style={{
                    background: 'var(--primary-500)',
                    color: '#fff',
                    border: 'none',
                    padding: '0.6rem 1.25rem',
                    borderRadius: 'var(--radius-md)',
                    fontWeight: 600,
                    cursor: isCreating ? 'not-allowed' : 'pointer',
                  }}
                >
                  {isCreating ? 'Guardando...' : 'Crear Cuenta'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
