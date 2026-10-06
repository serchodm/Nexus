import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Brain,
  Send,
  Zap,
  ArrowRight,
  ShieldCheck,
  RefreshCw,
} from 'lucide-react';

interface ChatMessage {
  id: string;
  sender: 'aura' | 'user';
  timestamp: string;
  text: string;
  actionCard?: {
    title: string;
    description: string;
    impact: string;
    buttonText: string;
    type: 'yield' | 'split' | 'rebalance';
  };
}

const INITIAL_MESSAGES: ChatMessage[] = [
  {
    id: 'm-1',
    sender: 'aura',
    timestamp: 'Today, 14:15',
    text: 'Good afternoon, Sergio. I have analyzed your unified accounts and shared household cash flows. I found 2 proactive yield optimizations and a pending shared split settlement.',
    actionCard: {
      title: 'Yield Leak Detected: Idle Liquid Checking',
      description: 'You have €18,420 sitting in BBVA Checking at 0.00% APY. Moving €12,000 into Trade Republic High-Yield Vault (3.75% APY) generates guaranteed passive return.',
      impact: '+€450.00 / yr guaranteed',
      buttonText: 'Simulate High-Yield Transfer',
      type: 'yield',
    },
  },
  {
    id: 'm-2',
    sender: 'aura',
    timestamp: 'Today, 14:16',
    text: 'Your partner has also logged 2 new shared utility receipts. Your net balance in the Fair-Share space is positive: you are owed €142.50.',
    actionCard: {
      title: 'Fair-Share Balance: In Your Favor',
      description: 'Based on your 55/45 progressive income split ratio, send a 1-tap Bizum request or offset next month groceries.',
      impact: '€142.50 Receivable',
      buttonText: 'Request Settlement via Bizum',
      type: 'split',
    },
  },
];

export function AuraPage() {
  const { t } = useTranslation();
  const [messages, setMessages] = useState<ChatMessage[]>(INITIAL_MESSAGES);
  const [inputText, setInputText] = useState('');
  const [isTyping, setIsTyping] = useState(false);

  const SUGGESTED_PROMPTS = [
    t('aura.prompts.property'),
    t('aura.prompts.invest'),
    t('aura.prompts.subscriptions'),
    t('aura.prompts.split'),
  ];

  const handleSend = () => {
    if (!inputText.trim()) return;

    const userMsg: ChatMessage = {
      id: `u-${Date.now()}`,
      sender: 'user',
      timestamp: 'Just now',
      text: inputText,
    };

    setMessages((prev) => [...prev, userMsg]);
    setInputText('');
    setIsTyping(true);

    // Simulate Aura AI Response
    setTimeout(() => {
      let responseText = 'Telemetry processed. ';
      let actionCard = undefined;

      const lower = userMsg.text.toLowerCase();
      if (lower.includes('property') || lower.includes('mortgage') || lower.includes('house') || lower.includes('vivienda') || lower.includes('房产') || lower.includes('संपत्ति')) {
        responseText = 'Based on your liquid reserves of €34,820 and joint annual savings pace, acquiring a €1.2M property with an 80% LTV mortgage requires €240,000 in equity. Your current runway would require 22 months of targeted capital accumulation or reallocating from equity holdings.';
        actionCard = {
          title: 'What-If Simulation Ready',
          description: 'Loaded property scenario with 2.15% fixed mortgage rate and current inflation projections.',
          impact: '22 Mos Accumulation Runway',
          buttonText: 'Open Scenario in Simulator',
          type: 'rebalance' as const,
        };
      } else if (lower.includes('invest') || lower.includes('portfolio') || lower.includes('stock') || lower.includes('invertir') || lower.includes('投资') || lower.includes('निवेश')) {
        responseText = 'After safeguarding your 6-month emergency reserve (€18,000), your investable surplus this month is €1,850. Aura recommends allocating 70% to Vanguard All-World (VWCE) and 30% to high-yield bonds.';
        actionCard = {
          title: 'Automated DCA Allocation',
          description: 'Pre-filled trade order for MyInvestor custodian with zero commission.',
          impact: '+€1,850 Monthly DCA',
          buttonText: 'Review DCA Order',
          type: 'yield' as const,
        };
      } else {
        responseText = `Understood. Analyzing telemetry for: "${userMsg.text}". All connected accounts (BBVA, ING, Revolut, MyInvestor) are synced with zero-knowledge cryptographic verification.`;
      }

      const auraReply: ChatMessage = {
        id: `a-${Date.now()}`,
        sender: 'aura',
        timestamp: 'Just now',
        text: responseText,
        actionCard,
      };

      setMessages((prev) => [...prev, auraReply]);
      setIsTyping(false);
    }, 800);
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%', gap: 'var(--space-md)' }}>
      {/* Top Header */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 'var(--space-md)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <div style={{
            width: 44,
            height: 44,
            borderRadius: 12,
            background: 'linear-gradient(135deg, rgba(76, 215, 246, 0.2), rgba(208, 188, 255, 0.2))',
            border: '1px solid rgba(76, 215, 246, 0.4)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'var(--secondary)',
            boxShadow: '0 0 16px rgba(76, 215, 246, 0.25)',
          }}>
            <Brain size={24} />
          </div>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <h2 style={{ margin: 0, fontSize: 'var(--text-headline-md)', fontWeight: 700 }}>{t('aura.title')}</h2>
              <span className="badge badge--secondary" style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
                <Zap size={12} /> {t('aura.neuralActive')}
              </span>
            </div>
            <p style={{ margin: '2px 0 0', fontSize: 'var(--text-body-sm)', color: 'var(--text-muted)' }}>
              {t('aura.subtitle')}
            </p>
          </div>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <span className="badge badge--success" style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
            <ShieldCheck size={12} /> {t('aura.zeroKnowledge')}
          </span>
          <button className="btn-secondary" onClick={() => setMessages(INITIAL_MESSAGES)} style={{ gap: 6 }}>
            <RefreshCw size={14} /> {t('aura.clearThread')}
          </button>
        </div>
      </div>

      {/* Main Conversation Stream */}
      <div className="glass-panel" style={{
        flex: 1,
        minHeight: 460,
        borderRadius: 'var(--radius-xl)',
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
      }}>
        {/* Messages Scroll Area */}
        <div style={{
          flex: 1,
          overflowY: 'auto',
          padding: 'var(--space-lg)',
          display: 'flex',
          flexDirection: 'column',
          gap: 'var(--space-md)',
        }}>
          {messages.map((msg) => (
            <div
              key={msg.id}
              style={{
                display: 'flex',
                gap: 12,
                alignSelf: msg.sender === 'user' ? 'flex-end' : 'flex-start',
                maxWidth: msg.sender === 'user' ? '70%' : '80%',
              }}
            >
              {msg.sender === 'aura' && (
                <div style={{
                  width: 32,
                  height: 32,
                  borderRadius: 8,
                  background: 'var(--bg-surface-container-highest)',
                  border: '1px solid var(--border-outline-variant)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'var(--secondary)',
                  flexShrink: 0,
                  marginTop: 2,
                }}>
                  <Brain size={16} />
                </div>
              )}

              <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
                <div style={{
                  background: msg.sender === 'user' ? 'var(--primary)' : 'var(--bg-surface-container-high)',
                  color: msg.sender === 'user' ? 'var(--bg-surface-dim)' : 'var(--text-main)',
                  padding: '12px 16px',
                  borderRadius: msg.sender === 'user' ? '16px 16px 4px 16px' : '16px 16px 16px 4px',
                  border: msg.sender === 'user' ? 'none' : '1px solid var(--border-outline-variant)',
                  fontSize: 'var(--text-body-md)',
                  lineHeight: '1.5',
                  fontWeight: msg.sender === 'user' ? 500 : 400,
                  boxShadow: msg.sender === 'user' ? '0 2px 8px rgba(77, 142, 255, 0.25)' : 'none',
                }}>
                  {msg.text}
                </div>

                {/* Optional Action Card */}
                {msg.actionCard && (
                  <div style={{
                    background: 'var(--bg-surface-container-lowest)',
                    border: '1px solid var(--border-outline-variant)',
                    borderLeft: `3px solid ${msg.actionCard.type === 'yield' ? 'var(--secondary)' : msg.actionCard.type === 'split' ? '#22c55e' : 'var(--tertiary)'}`,
                    borderRadius: 'var(--radius-lg)',
                    padding: 'var(--space-md)',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: 8,
                    marginTop: 4,
                  }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <span style={{ fontWeight: 600, fontSize: 'var(--text-body-sm)', color: 'var(--text-main)' }}>
                        {msg.actionCard.title}
                      </span>
                      <span className="badge badge--success" style={{ fontSize: 11 }}>
                        {msg.actionCard.impact}
                      </span>
                    </div>
                    <p style={{ margin: 0, fontSize: 'var(--text-body-sm)', color: 'var(--text-muted)' }}>
                      {msg.actionCard.description}
                    </p>
                    <button
                      className="btn-primary"
                      style={{ alignSelf: 'flex-start', marginTop: 4, padding: '6px 14px', fontSize: 'var(--text-label-md)', gap: 6 }}
                    >
                      {msg.actionCard.buttonText}
                      <ArrowRight size={14} />
                    </button>
                  </div>
                )}

                <span style={{
                  fontSize: 11,
                  color: 'var(--text-dim)',
                  alignSelf: msg.sender === 'user' ? 'flex-end' : 'flex-start',
                  paddingLeft: 4,
                }}>
                  {msg.timestamp}
                </span>
              </div>
            </div>
          ))}

          {isTyping && (
            <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
              <div style={{
                width: 32,
                height: 32,
                borderRadius: 8,
                background: 'var(--bg-surface-container-highest)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'var(--secondary)',
              }}>
                <Brain size={16} />
              </div>
              <div style={{
                background: 'var(--bg-surface-container-high)',
                padding: '10px 16px',
                borderRadius: '16px',
                display: 'flex',
                gap: 6,
                alignItems: 'center',
              }}>
                <span style={{ width: 6, height: 6, borderRadius: '50%', background: 'var(--secondary)', animation: 'pulse 1s infinite' }} />
                <span style={{ width: 6, height: 6, borderRadius: '50%', background: 'var(--secondary)', animation: 'pulse 1s infinite 0.2s' }} />
                <span style={{ width: 6, height: 6, borderRadius: '50%', background: 'var(--secondary)', animation: 'pulse 1s infinite 0.4s' }} />
              </div>
            </div>
          )}
        </div>

        {/* Suggested Prompts Pills */}
        <div style={{
          padding: '8px var(--space-lg)',
          borderTop: '1px solid var(--border-outline-variant)',
          display: 'flex',
          gap: 8,
          overflowX: 'auto',
          background: 'var(--bg-surface-container-low)',
        }}>
          {SUGGESTED_PROMPTS.map((prompt) => (
            <button
              key={prompt}
              onClick={() => {
                setInputText(prompt);
              }}
              style={{
                background: 'var(--bg-surface-container-high)',
                border: '1px solid var(--border-outline-variant)',
                color: 'var(--text-muted)',
                borderRadius: 'var(--radius-full)',
                padding: '4px 12px',
                fontSize: 'var(--text-label-sm)',
                whiteSpace: 'nowrap',
                cursor: 'pointer',
                transition: 'all 0.15s ease',
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.color = 'var(--secondary)';
                e.currentTarget.style.borderColor = 'var(--secondary)';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.color = 'var(--text-muted)';
                e.currentTarget.style.borderColor = 'var(--border-outline-variant)';
              }}
            >
              {prompt}
            </button>
          ))}
        </div>

        {/* Input Bar */}
        <div style={{
          padding: 'var(--space-md) var(--space-lg)',
          borderTop: '1px solid var(--border-outline-variant)',
          background: 'var(--bg-surface-container-lowest)',
          display: 'flex',
          alignItems: 'center',
          gap: 12,
        }}>
          <input
            className="input"
            placeholder={t('aura.askPlaceholder')}
            value={inputText}
            onChange={(e) => setInputText(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter') handleSend();
            }}
            style={{
              flex: 1,
              background: 'var(--bg-surface-container-high)',
              border: '1px solid var(--border-outline-variant)',
              borderRadius: 'var(--radius-lg)',
              padding: '10px 14px',
              fontSize: 'var(--text-body-md)',
              color: 'var(--text-main)',
            }}
          />
          <button
            className="btn-primary"
            onClick={handleSend}
            disabled={!inputText.trim()}
            style={{
              padding: '10px 18px',
              borderRadius: 'var(--radius-lg)',
              gap: 8,
              opacity: !inputText.trim() ? 0.6 : 1,
            }}
          >
            <span>{t('aura.askAura')}</span>
            <Send size={15} />
          </button>
        </div>
      </div>
    </div>
  );
}
