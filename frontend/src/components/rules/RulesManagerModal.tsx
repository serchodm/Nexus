import React, { useState } from 'react';
import { X, Sliders, Play, Plus, Trash2, CheckCircle2, BookmarkPlus } from 'lucide-react';
import { useRules } from '../../hooks/useRules';
import type { CategoryTree } from '../../types/category';

interface RulesManagerModalProps {
  categories: CategoryTree[];
  onClose: () => void;
}

export const RulesManagerModal: React.FC<RulesManagerModalProps> = ({
  categories,
  onClose,
}) => {
  const {
    rules,
    isLoading,
    createRule,
    isCreating,
    deleteRule,
    isDeleting,
    applyRetroactive,
    isApplyingRetroactive,
  } = useRules();

  const [pattern, setPattern] = useState('');
  const [matchType, setMatchType] = useState<1 | 2 | 3>(1); // Contains
  const [selectedCat, setSelectedCat] = useState<string>(() => {
    const first = categories[0];
    return first?.id ?? '';
  });
  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const handleCreateRule = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!pattern.trim()) {
      setErrorMsg('Debes especificar un patrón para la regla.');
      return;
    }

    const [catId, subcatId] = selectedCat.split(':');

    try {
      setErrorMsg(null);
      await createRule({
        name: `Regla para ${pattern.trim()}`,
        pattern: pattern.trim(),
        matchType,
        targetCategoryId: catId,
        targetSubcategoryId: subcatId || null,
        priority: 100,
      });

      setPattern('');
      setSuccessMsg('Regla creada con éxito.');
    } catch (err: unknown) {
      setErrorMsg(err instanceof Error ? err.message : 'Error al crear la regla.');
    }
  };

  const handleApplyRetroactive = async () => {
    try {
      setErrorMsg(null);
      const res = await applyRetroactive({ overrideUserManual: false });
      setSuccessMsg(
        `Reglas aplicadas: ${res.totalUpdated} movimientos actualizados de ${res.totalProcessed} analizados (prevalencia manual respetada).`
      );
    } catch (err: unknown) {
      setErrorMsg(err instanceof Error ? err.message : 'Error al aplicar reglas de forma retroactiva.');
    }
  };

  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.75)',
        backdropFilter: 'blur(8px)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: '1.5rem',
        zIndex: 100,
      }}
      onClick={onClose}
    >
      <div
        className="glass-card"
        style={{
          width: '100%',
          maxWidth: '720px',
          maxHeight: '90vh',
          display: 'flex',
          flexDirection: 'column',
          boxShadow: 'var(--shadow-glass)',
          overflow: 'hidden',
        }}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div
          style={{
            padding: '1.25rem 1.75rem',
            borderBottom: '1px solid var(--border-subtle)',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <div
              style={{
                width: '36px',
                height: '36px',
                borderRadius: 'var(--radius-md)',
                background: 'hsla(217, 91%, 60%, 0.15)',
                color: 'var(--primary-500)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <Sliders size={18} />
            </div>
            <div>
              <h3 style={{ fontSize: '1.15rem', fontWeight: 600, color: 'var(--text-main)', margin: 0 }}>
                Motor de Reglas de Categorización
              </h3>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-dim)', margin: 0 }}>
                Automatiza comercios habituales y ejecuta reaplicación retroactiva
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            style={{
              background: 'transparent',
              border: 'none',
              color: 'var(--text-muted)',
              cursor: 'pointer',
              padding: '0.3rem',
            }}
          >
            <X size={18} />
          </button>
        </div>

        {/* Content */}
        <div style={{ padding: '1.5rem 1.75rem', overflowY: 'auto', flex: 1 }}>
          {successMsg && (
            <div
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '0.5rem',
                padding: '0.75rem 1rem',
                borderRadius: 'var(--radius-md)',
                background: 'var(--success-bg)',
                border: '1px solid var(--success)',
                color: 'var(--success)',
                fontSize: '0.85rem',
                marginBottom: '1rem',
              }}
            >
              <CheckCircle2 size={16} />
              <span>{successMsg}</span>
            </div>
          )}

          {errorMsg && (
            <div
              style={{
                padding: '0.75rem 1rem',
                borderRadius: 'var(--radius-md)',
                background: 'var(--danger-bg)',
                border: '1px solid var(--danger)',
                color: 'var(--danger)',
                fontSize: '0.85rem',
                marginBottom: '1rem',
              }}
            >
              {errorMsg}
            </div>
          )}

          {/* Formulario Nueva Regla */}
          <div
            style={{
              background: 'hsla(222, 47%, 16%, 0.5)',
              border: '1px solid var(--border-subtle)',
              borderRadius: 'var(--radius-lg)',
              padding: '1.25rem',
              marginBottom: '1.5rem',
            }}
          >
            <h4 style={{ fontSize: '0.9rem', color: 'var(--text-main)', marginBottom: '0.75rem', display: 'flex', alignItems: 'center', gap: '0.4rem' }}>
              <BookmarkPlus size={16} color="var(--primary-500)" />
              Crear Nueva Regla de Comercio
            </h4>
            <form onSubmit={handleCreateRule} style={{ display: 'grid', gridTemplateColumns: '1.5fr 1fr 1.5fr auto', gap: '0.75rem', alignItems: 'flex-end' }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.72rem', color: 'var(--text-dim)', marginBottom: '0.25rem' }}>
                  Patrón / Comercio
                </label>
                <input
                  type="text"
                  placeholder="Ej. Mercadona, Spotify..."
                  value={pattern}
                  onChange={(e) => setPattern(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '0.5rem 0.75rem',
                    background: 'var(--bg-app)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: 'var(--text-main)',
                    fontSize: '0.82rem',
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.72rem', color: 'var(--text-dim)', marginBottom: '0.25rem' }}>
                  Coincidencia
                </label>
                <select
                  value={matchType}
                  onChange={(e) => setMatchType(parseInt(e.target.value, 10) as 1 | 2 | 3)}
                  style={{
                    width: '100%',
                    padding: '0.5rem',
                    background: 'var(--bg-app)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: 'var(--text-main)',
                    fontSize: '0.82rem',
                  }}
                >
                  <option value={1}>Contiene</option>
                  <option value={2}>Exacta</option>
                </select>
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.72rem', color: 'var(--text-dim)', marginBottom: '0.25rem' }}>
                  Asignar a Categoría
                </label>
                <select
                  value={selectedCat}
                  onChange={(e) => setSelectedCat(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '0.5rem',
                    background: 'var(--bg-app)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: 'var(--text-main)',
                    fontSize: '0.82rem',
                  }}
                >
                  {categories.map((cat) => (
                    <optgroup key={cat.id} label={cat.name}>
                      <option value={cat.id}>{cat.name} (General)</option>
                      {cat.subcategories.map((sub) => (
                        <option key={sub.id} value={`${cat.id}:${sub.id}`}>
                          {cat.name} • {sub.name}
                        </option>
                      ))}
                    </optgroup>
                  ))}
                </select>
              </div>

              <button
                type="submit"
                disabled={isCreating}
                className="btn-primary"
                style={{ padding: '0.5rem 1rem', fontSize: '0.82rem' }}
              >
                <Plus size={15} />
                {isCreating ? 'Guardando...' : 'Crear'}
              </button>
            </form>
          </div>

          {/* Listado de Reglas Activas */}
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
            <h4 style={{ fontSize: '0.92rem', color: 'var(--text-main)', margin: 0 }}>
              Reglas Activas ({rules.length})
            </h4>

            <button
              type="button"
              onClick={handleApplyRetroactive}
              disabled={isApplyingRetroactive}
              className="btn-secondary"
              style={{
                fontSize: '0.78rem',
                padding: '0.35rem 0.75rem',
                borderColor: 'var(--primary-500)',
                color: 'var(--primary-500)',
              }}
            >
              <Play size={13} />
              {isApplyingRetroactive ? 'Procesando...' : 'Reaplicar a Histórico'}
            </button>
          </div>

          {isLoading ? (
            <p style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '1rem' }}>Cargando reglas...</p>
          ) : rules.length === 0 ? (
            <p style={{ textAlign: 'center', color: 'var(--text-dim)', padding: '1.5rem', fontSize: '0.85rem' }}>
              No tienes reglas creadas todavía. Crea una arriba o categoriza movimientos activando «Crear regla».
            </p>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
              {rules.map((rule) => (
                <div
                  key={rule.id}
                  style={{
                    background: 'hsla(222, 47%, 14%, 0.6)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-md)',
                    padding: '0.75rem 1rem',
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                  }}
                >
                  <div>
                    <span style={{ fontWeight: 600, color: 'var(--text-main)', fontSize: '0.88rem' }}>
                      «{rule.pattern}»
                    </span>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)', marginLeft: '0.5rem' }}>
                      ({rule.matchType === 1 ? 'Contiene' : 'Exacta'})
                    </span>
                    <div style={{ fontSize: '0.78rem', color: 'var(--primary-500)', marginTop: '0.2rem' }}>
                      ➜ {rule.targetCategoryName} {rule.targetSubcategoryName ? `• ${rule.targetSubcategoryName}` : ''}
                    </div>
                  </div>

                  <button
                    onClick={() => deleteRule(rule.id)}
                    disabled={isDeleting}
                    style={{
                      background: 'transparent',
                      border: 'none',
                      color: 'var(--danger)',
                      cursor: 'pointer',
                      padding: '0.3rem',
                    }}
                    title="Eliminar regla"
                  >
                    <Trash2 size={16} />
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Footer */}
        <div
          style={{
            padding: '1rem 1.75rem',
            borderTop: '1px solid var(--border-subtle)',
            display: 'flex',
            justifyContent: 'flex-end',
          }}
        >
          <button type="button" onClick={onClose} className="btn-secondary">
            Cerrar
          </button>
        </div>
      </div>
    </div>
  );
};
