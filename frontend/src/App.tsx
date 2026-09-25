import { 
  CheckCircle2, 
  Clock, 
  Cpu, 
  Database, 
  KeyRound, 
  Layers, 
  RefreshCw, 
  ShieldCheck, 
  Sparkles 
} from 'lucide-react';
import { Navbar } from './components/layout/Navbar';
import { Footer } from './components/layout/Footer';
import { AuthSection } from './components/auth/AuthSection';
import { useHealthCheck } from './hooks/useHealthCheck';

export default function App() {
  const { data: health, isLoading, isError, refetch, isFetching } = useHealthCheck();

  return (
    <>
      {/* React 19: Metadatos elevados automáticamente al <head> */}
      <title>Nexus | Plataforma Clean Architecture & React 19</title>
      <meta
        name="description"
        content="Nexus - Backend de alto rendimiento con .NET 10 y Frontend desacoplado con React 19"
      />

      <Navbar />

      <main style={{ maxWidth: '1200px', margin: '0 auto', padding: '3.5rem 1.5rem', width: '100%' }}>
        {/* Hero Section */}
        <section style={{ textAlign: 'center', marginBottom: '3.5rem' }}>
          <div
            style={{
              display: 'inline-flex',
              alignItems: 'center',
              gap: '0.5rem',
              padding: '0.4rem 1rem',
              borderRadius: 'var(--radius-full)',
              background: 'hsla(217, 91%, 60%, 0.1)',
              border: '1px solid hsla(217, 91%, 60%, 0.25)',
              color: 'var(--primary-500)',
              fontSize: '0.85rem',
              fontWeight: 600,
              marginBottom: '1.5rem',
            }}
          >
            <Sparkles size={16} />
            Arquitectura Desacoplada de Alto Rendimiento
          </div>

          <h1
            style={{
              fontSize: 'clamp(2.5rem, 5vw, 4rem)',
              lineHeight: 1.1,
              marginBottom: '1.25rem',
              background: 'linear-gradient(135deg, #ffffff 30%, var(--text-muted) 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
            }}
          >
            Plataforma Nexus
          </h1>

          <p
            style={{
              fontSize: '1.15rem',
              color: 'var(--text-muted)',
              maxWidth: '680px',
              margin: '0 auto 2.5rem auto',
              lineHeight: 1.6,
            }}
          >
            Ecosistema empresarial construido con <strong>.NET 10</strong>, <strong>Clean Architecture</strong>, 
            <strong> FastEndpoints</strong> y un cliente frontend moderno desacoplado en <strong>React 19 + TypeScript</strong>.
          </p>

          <div style={{ display: 'flex', justifyContent: 'center', gap: '1rem', flexWrap: 'wrap' }}>
            <button
              type="button"
              className="btn-primary"
              onClick={() => refetch()}
              disabled={isFetching}
            >
              <RefreshCw size={17} className={isFetching ? 'spin-icon' : undefined} />
              {isFetching ? 'Consultando API...' : 'Verificar Conexión Backend'}
            </button>

            <a
              href="http://localhost:5276/swagger"
              target="_blank"
              rel="noopener noreferrer"
              className="btn-secondary"
            >
              <Layers size={17} />
              Explorar Endpoints OpenAPI
            </a>
          </div>
        </section>

        {/* Live Status Card */}
        <section
          className="glass-card"
          style={{
            padding: '2rem',
            marginBottom: '3.5rem',
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))',
            gap: '1.75rem',
          }}
        >
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', color: 'var(--text-dim)', fontSize: '0.85rem', marginBottom: '0.5rem' }}>
              <Cpu size={16} />
              <span>ESTADO DE NEXUS API</span>
            </div>
            <div style={{ fontSize: '1.35rem', fontWeight: 700, color: health?.status === 'Healthy' ? 'var(--success)' : 'var(--text-main)' }}>
              {isLoading ? 'Comprobando...' : isError ? 'Sin Conexión' : health?.status}
            </div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '0.25rem' }}>
              Endpoint: <code style={{ color: 'var(--accent-cyan)' }}>/api/health</code>
            </div>
          </div>

          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', color: 'var(--text-dim)', fontSize: '0.85rem', marginBottom: '0.5rem' }}>
              <Database size={16} />
              <span>PERSISTENCIA</span>
            </div>
            <div style={{ fontSize: '1.35rem', fontWeight: 700, color: 'var(--text-main)' }}>
              PostgreSQL 17 (Docker)
            </div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '0.25rem' }}>
              Base: <code style={{ color: 'var(--accent-cyan)' }}>nexus_db</code> • Snake Case
            </div>
          </div>

          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', color: 'var(--text-dim)', fontSize: '0.85rem', marginBottom: '0.5rem' }}>
              <Clock size={16} />
              <span>MARCA TEMPORAL (UTC)</span>
            </div>
            <div style={{ fontSize: '1.1rem', fontWeight: 600, color: 'var(--text-main)' }}>
              {health?.timestamp ? new Date(health.timestamp).toLocaleTimeString() : '—'}
            </div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '0.25rem' }}>
              {health?.timestamp ? new Date(health.timestamp).toLocaleDateString() : 'Sin sincronizar'}
            </div>
          </div>

          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', color: 'var(--text-dim)', fontSize: '0.85rem', marginBottom: '0.5rem' }}>
              <ShieldCheck size={16} />
              <span>SEGURIDAD EN MEMORIA</span>
            </div>
            <div style={{ fontSize: '1.35rem', fontWeight: 700, color: 'var(--primary-500)' }}>
              Cero XSS Storage
            </div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '0.25rem' }}>
              Cookie HttpOnly + Access Token RAM
            </div>
          </div>
        </section>

        {/* Sección Interactiva de Autenticación & MFA */}
        <AuthSection />

        {/* Roadmap & Phases Grid */}
        <section style={{ marginBottom: '3.5rem' }}>
          <h2 style={{ fontSize: '1.75rem', marginBottom: '1.75rem', display: 'flex', alignItems: 'center', gap: '0.6rem' }}>
            <Layers size={22} color="var(--primary-500)" />
            Fases del Proyecto
          </h2>

          <div
            style={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))',
              gap: '1.5rem',
            }}
          >
            {/* Card Fase 1 */}
            <div className="glass-card" style={{ padding: '1.75rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1rem' }}>
                <span style={{ fontSize: '0.8rem', fontWeight: 700, color: 'var(--success)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                  Fase 1 • Completada
                </span>
                <CheckCircle2 size={20} color="var(--success)" />
              </div>
              <h3 style={{ fontSize: '1.25rem', marginBottom: '0.5rem' }}>Infraestructura & Solución</h3>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: '1rem' }}>
                Clean Architecture en .NET 10, FastEndpoints, Swagger OpenAPI y orquestación con Docker Compose (PostgreSQL 17).
              </p>
              <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
                <span className="badge-healthy">.NET 10</span>
                <span className="badge-healthy">PostgreSQL 17</span>
                <span className="badge-healthy">FastEndpoints</span>
              </div>
            </div>

            {/* Card Fase 2 */}
            <div className="glass-card" style={{ padding: '1.75rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1rem' }}>
                <span style={{ fontSize: '0.8rem', fontWeight: 700, color: 'var(--success)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                  Fase 2 • Completada
                </span>
                <CheckCircle2 size={20} color="var(--success)" />
              </div>
              <h3 style={{ fontSize: '1.25rem', marginBottom: '0.5rem' }}>Dominio Desacoplado</h3>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: '1rem' }}>
                Entidades e interfaces transversales (IEntity, IAuditableEntity, ISoftDeletable, IUserRepository, IUnitOfWork). Migración aplicada.
              </p>
              <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
                <span className="badge-healthy">Interfaces</span>
                <span className="badge-healthy">EF Core 10</span>
                <span className="badge-healthy">Soft Delete</span>
              </div>
            </div>

            {/* Card Fase 3 */}
            <div
              className="glass-card"
              style={{
                padding: '1.75rem',
                borderColor: 'hsla(152, 69%, 45%, 0.4)',
                background: 'linear-gradient(180deg, var(--bg-surface), hsla(152, 69%, 45%, 0.05))',
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1rem' }}>
                <span style={{ fontSize: '0.8rem', fontWeight: 700, color: 'var(--success)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                  Fase 3 • Operativa
                </span>
                <KeyRound size={20} color="var(--success)" />
              </div>
              <h3 style={{ fontSize: '1.25rem', marginBottom: '0.5rem' }}>Autenticación & MFA</h3>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: '1rem' }}>
                Google OAuth (claim inmutable sub), Silent Refresh con Cookie HttpOnly y verificación TOTP RFC 6238 anti-replay.
              </p>
              <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
                <span className="badge-healthy">Google OAuth</span>
                <span className="badge-healthy">MFA TOTP</span>
                <span className="badge-healthy">HttpOnly Cookie</span>
              </div>
            </div>
          </div>
        </section>
      </main>

      <Footer />
    </>
  );
}
