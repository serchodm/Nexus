import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Settings,
  Users,
  Shield,
  KeyRound,
  Landmark,
  Save,
  CheckCircle2,
  Trash2,
  Lock,
  Globe,
} from 'lucide-react';
import { SUPPORTED_LANGUAGES, changeLanguage, type SupportedLanguageCode } from '../i18n';
import { LanguageSwitcher } from '../components/common/LanguageSwitcher';
import { useSharedSpace } from '../hooks/useSharedSpace';
import { ConnectedBanksCard } from '../components/banking/ConnectedBanksCard';

export function SettingsPage() {
  const { t, i18n } = useTranslation();
  const { summary, updateRatio } = useSharedSpace();

  // Extract initial percentage from defaultSplitRatio (e.g. "55/45")
  const initialRatio = Number(summary.defaultSplitRatio.split('/')[0]) || 55;
  const [splitRatio, setSplitRatio] = useState<number>(initialRatio);
  const [userIncome, setUserIncome] = useState<number>(2750);
  const [partnerIncome, setPartnerIncome] = useState<number>(2250);
  const [currency, setCurrency] = useState<string>('EUR');
  const [terminalMode, setTerminalMode] = useState<boolean>(true);
  const [autoSettle, setAutoSettle] = useState<boolean>(true);
  const [mfaEnabled, setMfaEnabled] = useState<boolean>(true);
  const [isSaved, setIsSaved] = useState<boolean>(false);

  // Recalculate split ratio from declared incomes
  const handleIncomeChange = (newUserInc: number, newPartnerInc: number) => {
    setUserIncome(newUserInc);
    setPartnerIncome(newPartnerInc);
    const total = newUserInc + newPartnerInc;
    if (total > 0) {
      const calculated = Math.round((newUserInc / total) * 100);
      setSplitRatio(Math.min(70, Math.max(30, calculated)));
    }
  };

  const handleSave = async () => {
    try {
      await updateRatio({
        sharedSpaceId: summary.id,
        ruleType: 'FixedRatio',
        ownerPercentage: splitRatio,
        ownerIncome: userIncome,
        memberIncome: partnerIncome,
      });
      setIsSaved(true);
      setTimeout(() => setIsSaved(false), 2500);
    } catch (err) {
      console.error('Failed to update split ratio:', err);
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-lg)', maxWidth: 1000, margin: '0 auto', width: '100%' }}>
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 'var(--space-md)' }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Settings size={24} style={{ color: 'var(--secondary)' }} />
            <h2 style={{ margin: 0, fontSize: 'var(--text-headline-lg)', fontWeight: 700 }}>{t('settings.title')}</h2>
          </div>
          <p style={{ color: 'var(--text-muted)', fontSize: 'var(--text-body-md)', marginTop: 4 }}>
            {t('settings.subtitle')}
          </p>
        </div>

        <button className="btn-primary" onClick={handleSave} style={{ gap: 8, padding: '10px 20px' }}>
          {isSaved ? <CheckCircle2 size={16} /> : <Save size={16} />}
          <span>{isSaved ? t('settings.saved') : t('settings.saveChanges')}</span>
        </button>
      </div>

      {/* Section 1: Household & Fair-Share Split Engine */}
      <section className="glass-panel" style={{
        padding: 'var(--space-lg)',
        borderRadius: 'var(--radius-xl)',
        display: 'flex',
        flexDirection: 'column',
        gap: 'var(--space-md)',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, borderBottom: '1px solid var(--border-outline-variant)', paddingBottom: 'var(--space-sm)' }}>
          <Users size={18} style={{ color: 'var(--secondary)' }} />
          <h3 style={{ margin: 0, fontSize: 'var(--text-headline-sm)' }}>{t('settings.householdEngine')}</h3>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
              <label style={{ fontSize: 'var(--text-body-sm)', fontWeight: 600, color: 'var(--text-main)' }}>
                {t('settings.defaultSplitRatio')}
              </label>
              <span className="numeral" style={{ fontWeight: 700, color: 'var(--secondary)', fontSize: 'var(--text-body-md)' }}>
                {splitRatio}% / {100 - splitRatio}%
              </span>
            </div>
            <input
              type="range"
              min={30}
              max={70}
              step={5}
              value={splitRatio}
              onChange={(e) => setSplitRatio(Number(e.target.value))}
              style={{ width: '100%', accentColor: 'var(--secondary)', cursor: 'pointer' }}
            />
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 'var(--text-label-sm)', color: 'var(--text-dim)', marginTop: 4 }}>
              <span>30% / 70%</span>
              <span>50/50</span>
              <span>70% / 30%</span>
            </div>

            {/* Proportional Fair-Share Calculator */}
            <div style={{
              marginTop: 16,
              padding: 14,
              borderRadius: 'var(--radius-lg)',
              background: 'var(--bg-surface-container-high)',
              border: '1px solid var(--border-outline-variant)',
            }}>
              <span style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-main)', display: 'block', marginBottom: 8 }}>
                Fair-Share Proporcional según Ingresos Netos Mensuales
              </span>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label style={{ fontSize: 11, color: 'var(--text-muted)', display: 'block', marginBottom: 4 }}>
                    Sergio (Tú)
                  </label>
                  <div style={{ position: 'relative' }}>
                    <input
                      type="number"
                      step={50}
                      value={userIncome}
                      onChange={(e) => handleIncomeChange(Number(e.target.value), partnerIncome)}
                      style={{
                        width: '100%',
                        background: 'var(--bg-surface-container)',
                        border: '1px solid var(--border-outline-variant)',
                        borderRadius: 'var(--radius-md)',
                        padding: '8px 12px',
                        color: 'var(--text-main)',
                        fontSize: 13,
                        fontFamily: 'var(--font-mono)',
                      }}
                    />
                    <span style={{ position: 'absolute', right: 10, top: '50%', transform: 'translateY(-50%)', color: 'var(--text-dim)', fontSize: 12 }}>€/mes</span>
                  </div>
                </div>
                <div>
                  <label style={{ fontSize: 11, color: 'var(--text-muted)', display: 'block', marginBottom: 4 }}>
                    Elena (Pareja)
                  </label>
                  <div style={{ position: 'relative' }}>
                    <input
                      type="number"
                      step={50}
                      value={partnerIncome}
                      onChange={(e) => handleIncomeChange(userIncome, Number(e.target.value))}
                      style={{
                        width: '100%',
                        background: 'var(--bg-surface-container)',
                        border: '1px solid var(--border-outline-variant)',
                        borderRadius: 'var(--radius-md)',
                        padding: '8px 12px',
                        color: 'var(--text-main)',
                        fontSize: 13,
                        fontFamily: 'var(--font-mono)',
                      }}
                    />
                    <span style={{ position: 'absolute', right: 10, top: '50%', transform: 'translateY(-50%)', color: 'var(--text-dim)', fontSize: 12 }}>€/mes</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            paddingTop: 'var(--space-sm)',
            borderTop: '1px solid var(--border-outline-variant)',
          }}>
            <div>
              <div style={{ fontWeight: 600, color: 'var(--text-main)', fontSize: 'var(--text-body-sm)' }}>
                {t('settings.autoSettle')}
              </div>
              <div style={{ color: 'var(--text-muted)', fontSize: 'var(--text-body-sm)' }}>
                {t('settings.autoSettleSub')}
              </div>
            </div>
            <input
              type="checkbox"
              checked={autoSettle}
              onChange={(e) => setAutoSettle(e.target.checked)}
              style={{ width: 18, height: 18, accentColor: 'var(--secondary)', cursor: 'pointer' }}
            />
          </div>
        </div>
      </section>

      {/* Section 2: Open Banking Connected Banks */}
      <ConnectedBanksCard />

      {/* Section 3: Display & Internationalization (5 World Languages) */}
      <section className="glass-panel" style={{
        padding: 'var(--space-lg)',
        borderRadius: 'var(--radius-xl)',
        display: 'flex',
        flexDirection: 'column',
        gap: 'var(--space-md)',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', borderBottom: '1px solid var(--border-outline-variant)', paddingBottom: 'var(--space-sm)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Globe size={18} style={{ color: 'var(--secondary)' }} />
            <h3 style={{ margin: 0, fontSize: 'var(--text-headline-sm)' }}>{t('settings.displayTitle')}</h3>
          </div>
          <LanguageSwitcher />
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: 'var(--space-lg)' }}>
          {/* Language Selector */}
          <div>
            <label style={{ display: 'block', fontSize: 'var(--text-body-sm)', fontWeight: 600, color: 'var(--text-main)', marginBottom: 6 }}>
              {t('settings.language')}
            </label>
            <select
              value={i18n.language}
              onChange={(e) => changeLanguage(e.target.value as SupportedLanguageCode)}
              style={{
                width: '100%',
                background: 'var(--bg-surface-container-high)',
                border: '1px solid var(--border-outline-variant)',
                borderRadius: 'var(--radius-lg)',
                padding: '10px 12px',
                color: 'var(--text-main)',
                fontSize: 'var(--text-body-sm)',
                cursor: 'pointer',
              }}
            >
              {SUPPORTED_LANGUAGES.map((lang) => (
                <option key={lang.code} value={lang.code}>
                  {lang.flag} {lang.label} ({lang.short})
                </option>
              ))}
            </select>
          </div>

          {/* Currency Selector */}
          <div>
            <label style={{ display: 'block', fontSize: 'var(--text-body-sm)', fontWeight: 600, color: 'var(--text-main)', marginBottom: 6 }}>
              {t('settings.currency')}
            </label>
            <select
              value={currency}
              onChange={(e) => setCurrency(e.target.value)}
              style={{
                width: '100%',
                background: 'var(--bg-surface-container-high)',
                border: '1px solid var(--border-outline-variant)',
                borderRadius: 'var(--radius-lg)',
                padding: '10px 12px',
                color: 'var(--text-main)',
                fontSize: 'var(--text-body-sm)',
                cursor: 'pointer',
              }}
            >
              <option value="EUR">EUR (€) - Eurozone</option>
              <option value="USD">USD ($) - US Dollar</option>
              <option value="GBP">GBP (£) - British Pound</option>
              <option value="CNY">CNY (¥) - Chinese Yuan</option>
              <option value="INR">INR (₹) - Indian Rupee</option>
            </select>
          </div>

          {/* High Contrast Terminal Mode */}
          <div style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <span style={{ fontSize: 'var(--text-body-sm)', fontWeight: 600, color: 'var(--text-main)' }}>
                  {t('settings.terminalMode')}
                </span>
                <p style={{ margin: 0, fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)' }}>
                  {t('settings.terminalSub')}
                </p>
              </div>
              <input
                type="checkbox"
                checked={terminalMode}
                onChange={(e) => setTerminalMode(e.target.checked)}
                style={{ width: 18, height: 18, accentColor: 'var(--secondary)', cursor: 'pointer' }}
              />
            </div>
          </div>
        </div>
      </section>

      {/* Section 3: Connected Banking & Custodians */}
      <section className="glass-panel" style={{
        padding: 'var(--space-lg)',
        borderRadius: 'var(--radius-xl)',
        display: 'flex',
        flexDirection: 'column',
        gap: 'var(--space-md)',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', borderBottom: '1px solid var(--border-outline-variant)', paddingBottom: 'var(--space-sm)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Landmark size={18} style={{ color: 'var(--primary)' }} />
            <h3 style={{ margin: 0, fontSize: 'var(--text-headline-sm)' }}>{t('settings.bankingPSD2')}</h3>
          </div>
          <button className="btn-secondary" style={{ padding: '4px 10px', fontSize: 'var(--text-label-sm)' }}>
            {t('settings.linkBank')}
          </button>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: 'var(--space-md)' }}>
          {[
            { name: 'BBVA Spain', type: 'Checking & Payroll', status: 'Connected • PSD2 Sync Active' },
            { name: 'Trade Republic', type: 'High-Yield Liquid Vault (3.75%)', status: 'Connected • Daily Telemetry' },
            { name: 'MyInvestor', type: 'Index Funds UCITS (VWCE)', status: 'Connected • Daily Net Asset Value' },
            { name: 'ING Direct', type: 'Orange Savings Account', status: 'Connected • Auto-Refreshed' },
          ].map((bank) => (
            <div key={bank.name} style={{
              background: 'var(--bg-surface-container-high)',
              border: '1px solid var(--border-outline-variant)',
              borderRadius: 'var(--radius-lg)',
              padding: 'var(--space-md)',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
            }}>
              <div>
                <div style={{ fontWeight: 600, color: 'var(--text-main)', fontSize: 'var(--text-body-sm)' }}>{bank.name}</div>
                <div style={{ fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)' }}>{bank.type}</div>
                <span className="badge badge--success" style={{ fontSize: 10, marginTop: 6, display: 'inline-block' }}>
                  {bank.status}
                </span>
              </div>
              <button className="btn-ghost" title="Disconnect" style={{ color: 'var(--text-dim)' }}>
                <Trash2 size={16} />
              </button>
            </div>
          ))}
        </div>
      </section>

      {/* Section 4: Institutional Security & Cryptography */}
      <section className="glass-panel" style={{
        padding: 'var(--space-lg)',
        borderRadius: 'var(--radius-xl)',
        display: 'flex',
        flexDirection: 'column',
        gap: 'var(--space-md)',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, borderBottom: '1px solid var(--border-outline-variant)', paddingBottom: 'var(--space-sm)' }}>
          <Shield size={18} style={{ color: '#22c55e' }} />
          <h3 style={{ margin: 0, fontSize: 'var(--text-headline-sm)' }}>{t('settings.securityTitle')}</h3>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-md)' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div>
              <div style={{ fontWeight: 600, color: 'var(--text-main)', fontSize: 'var(--text-body-sm)', display: 'flex', alignItems: 'center', gap: 6 }}>
                <KeyRound size={15} style={{ color: 'var(--secondary)' }} />
                {t('settings.biometricMfa')}
              </div>
              <div style={{ color: 'var(--text-muted)', fontSize: 'var(--text-body-sm)' }}>
                {t('settings.biometricSub')}
              </div>
            </div>
            <input
              type="checkbox"
              checked={mfaEnabled}
              onChange={(e) => setMfaEnabled(e.target.checked)}
              style={{ width: 18, height: 18, accentColor: 'var(--secondary)', cursor: 'pointer' }}
            />
          </div>

          <div style={{
            padding: 'var(--space-md)',
            borderRadius: 'var(--radius-lg)',
            background: 'var(--bg-surface-container-low)',
            border: '1px solid var(--border-outline-variant)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
          }}>
            <div>
              <div style={{ fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)', textTransform: 'uppercase', letterSpacing: '0.04em' }}>
                {t('settings.keyFingerprint')}
              </div>
              <div className="numeral" style={{ fontSize: 'var(--text-body-sm)', color: 'var(--text-main)', marginTop: 2 }}>
                SHA256: 7f8c:4b19:9f2e:3d0a:88c4:12ae:d894:bf63:e812
              </div>
            </div>
            <span className="badge badge--secondary" style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
              <Lock size={12} /> Verified
            </span>
          </div>
        </div>
      </section>
    </div>
  );
}
