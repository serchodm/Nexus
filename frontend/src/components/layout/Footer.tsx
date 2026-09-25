export function Footer() {
  return (
    <footer
      style={{
        marginTop: 'auto',
        borderTop: '1px solid var(--border-glass)',
        padding: '1.5rem 2rem',
        textAlign: 'center',
        color: 'var(--text-dim)',
        fontSize: '0.85rem',
      }}
    >
      <div style={{ maxWidth: '1200px', margin: '0 auto', display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <strong>Nexus Platform</strong> — Arquitectura Limpia (.NET 10 + FastEndpoints + React 19)
        </div>
        <div style={{ display: 'flex', gap: '1.5rem' }}>
          <span>PostgreSQL 17</span>
          <span>•</span>
          <span>Docker</span>
          <span>•</span>
          <span>Vite + TypeScript</span>
        </div>
      </div>
    </footer>
  );
}
