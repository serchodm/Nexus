import { Brain, PiggyBank, TrendingUp, AlertTriangle, ChevronRight, Send } from 'lucide-react';
import { useState } from 'react';

const AURA_INSIGHTS = [
  {
    id: 1,
    icon: PiggyBank,
    iconColor: 'var(--secondary)',
    iconBg: 'var(--secondary-bg)',
    iconBorder: 'var(--secondary-border)',
    title: 'Yield Leak Detected',
    titleColor: 'var(--secondary)',
    body: '€8,420 sitting at 0% in BBVA checking exceeds your emergency buffer by €2,420.',
    action: 'Optimize idle cash →',
    confidence: 'Confidence: 94%',
    tag: 'Opportunity',
    tagColor: 'var(--secondary)',
  },
  {
    id: 2,
    icon: AlertTriangle,
    iconColor: 'var(--warning)',
    iconBg: 'var(--warning-bg)',
    iconBorder: 'rgba(245, 158, 11, 0.3)',
    title: 'Subscription Drift Alert',
    titleColor: 'var(--warning)',
    body: 'Netflix increased from €12.99 → €17.99 silently. HBO Max unused for 3 months (€0 sessions).',
    action: 'Review subscriptions →',
    confidence: 'Monthly waste: €30.98',
    tag: 'Warning',
    tagColor: 'var(--warning)',
  },
  {
    id: 3,
    icon: TrendingUp,
    iconColor: 'var(--success)',
    iconBg: 'var(--success-bg)',
    iconBorder: 'var(--success-border)',
    title: 'Savings Goal Pace',
    titleColor: 'var(--success)',
    body: '"Emergency Fund" is on track. At current pace, you\'ll reach €18,000 target by March 2027.',
    action: 'View savings goals →',
    confidence: 'Ahead of schedule: +12%',
    tag: 'On Track',
    tagColor: 'var(--success)',
  },
];

export function AuraDrawer() {
  const [query, setQuery] = useState('');

  return (
    <aside className="app-shell__aura-drawer">
      {/* Header */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
        <div style={{
          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
          paddingBottom: 'var(--space-sm)',
          borderBottom: '1px solid rgba(66, 71, 84, 0.5)',
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <div style={{
              position: 'relative', width: 28, height: 28,
              borderRadius: 'var(--radius-md)',
              background: 'var(--tertiary-bg)',
              border: '1px solid var(--tertiary-border)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              color: 'var(--tertiary)',
            }}>
              <Brain size={16} />
              <span style={{
                position: 'absolute', top: -4, right: -4,
                display: 'flex', width: 8, height: 8,
              }}>
                <span style={{
                  position: 'absolute', width: '100%', height: '100%',
                  borderRadius: '50%', background: 'var(--secondary)',
                  opacity: 0.75,
                  animation: 'ping 1.5s cubic-bezier(0, 0, 0.2, 1) infinite',
                }} />
                <span style={{
                  position: 'relative', width: 8, height: 8,
                  borderRadius: '50%', background: 'var(--secondary)',
                }} />
              </span>
            </div>
            <div>
              <h4 style={{ fontSize: 'var(--text-label-md)', fontWeight: 600, margin: 0, lineHeight: 1.2 }}>
                Nexus Aura Copilot
              </h4>
              <span className="numeral" style={{ fontSize: 10, color: 'var(--tertiary)', letterSpacing: '0.04em' }}>
                Financial Intelligence Engine
              </span>
            </div>
          </div>
        </div>

        {/* Insight Cards */}
        <div style={{
          display: 'flex', flexDirection: 'column', gap: 'var(--space-sm)',
          overflowY: 'auto',
          maxHeight: 'calc(100vh - 230px)',
          paddingRight: 4,
        }}>
          {AURA_INSIGHTS.map((insight) => {
            const Icon = insight.icon;
            return (
              <div key={insight.id} className="aura-card glass-panel" style={{
                padding: 'var(--space-sm)',
                borderRadius: 'var(--radius-lg)',
              }}>
                <div style={{ display: 'flex', alignItems: 'flex-start', gap: 10 }}>
                  <div style={{
                    padding: 6, borderRadius: 'var(--radius-sm)',
                    background: insight.iconBg,
                    color: insight.iconColor,
                    border: `1px solid ${insight.iconBorder}`,
                    flexShrink: 0,
                  }}>
                    <Icon size={16} />
                  </div>
                  <div style={{ flex: 1 }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 4 }}>
                      <span style={{
                        fontSize: 'var(--text-label-sm)', fontWeight: 600,
                        color: insight.titleColor,
                      }}>{insight.title}</span>
                      <span style={{
                        fontSize: 10, fontWeight: 600,
                        padding: '1px 6px',
                        borderRadius: 'var(--radius-xs)',
                        background: insight.iconBg,
                        color: insight.tagColor,
                        border: `1px solid ${insight.iconBorder}`,
                      }}>{insight.tag}</span>
                    </div>
                    <p style={{ fontSize: 'var(--text-body-sm)', color: 'var(--text-on-surface-variant)', lineHeight: 1.5, margin: 0 }}>
                      {insight.body}
                    </p>
                    <div style={{
                      marginTop: 8, display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                      fontSize: 'var(--text-label-sm)',
                    }}>
                      <button style={{
                        background: 'none', border: 'none', color: insight.iconColor,
                        fontWeight: 600, fontSize: 'var(--text-label-sm)',
                        cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 4,
                        padding: 0,
                      }}>
                        {insight.action}
                        <ChevronRight size={12} />
                      </button>
                      <span className="numeral" style={{ color: 'var(--text-muted)', fontSize: 10 }}>
                        {insight.confidence}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Bottom: NLQ Input */}
      <div style={{
        borderTop: '1px solid rgba(66, 71, 84, 0.5)',
        paddingTop: 'var(--space-sm)',
        marginTop: 'auto',
      }}>
        <div style={{ position: 'relative' }}>
          <input
            className="input"
            placeholder="Ask Aura anything about your finances..."
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            style={{
              paddingRight: '2.5rem',
              background: 'var(--bg-surface-container)',
              border: '1px solid var(--tertiary-border)',
              fontSize: 'var(--text-body-sm)',
            }}
          />
          <button style={{
            position: 'absolute', right: 8, top: '50%', transform: 'translateY(-50%)',
            background: 'none', border: 'none',
            color: query ? 'var(--tertiary)' : 'var(--text-dim)',
            cursor: 'pointer',
            padding: 4,
          }}>
            <Send size={14} />
          </button>
        </div>
        <p style={{ fontSize: 10, color: 'var(--text-dim)', textAlign: 'center', marginTop: 6 }}>
          Aura never recommends paid products. 100% impartial.
        </p>
      </div>
    </aside>
  );
}
