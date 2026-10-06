import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  SlidersHorizontal,
  RotateCcw,
  Sparkles,
  Download,
  Building,
  Car,
  Briefcase,
  Layers,
  CheckCircle2,
} from 'lucide-react';

export function SimulatorPage() {
  const { t } = useTranslation();
  const [propertyPrice, setPropertyPrice] = useState<number>(1250000);
  const [vehicleFunding, setVehicleFunding] = useState<number>(145000);
  const [careerDelta, setCareerDelta] = useState<number>(85000);
  const [includeVolatility, setIncludeVolatility] = useState<boolean>(true);
  const [savingsRate, setSavingsRate] = useState<number>(35);

  const resetDefaults = () => {
    setPropertyPrice(1250000);
    setVehicleFunding(145000);
    setCareerDelta(85000);
    setIncludeVolatility(true);
    setSavingsRate(35);
  };

  // Dynamic calculations based on sliders
  const baselineNetWorth = 4.07;
  const simulatedImpact = (careerDelta * 10 * 0.7 - propertyPrice * 0.2 - vehicleFunding + (savingsRate - 30) * 20000) / 1000000;
  const volatilityPenalty = includeVolatility ? 0.35 : 0;
  const projectedNetWorth = Math.max(1.5, baselineNetWorth + simulatedImpact - volatilityPenalty).toFixed(2);
  const percentageDelta = (((Number(projectedNetWorth) - baselineNetWorth) / baselineNetWorth) * 100).toFixed(1);

  const liquidityMonths = Math.max(6, Math.round(34.2 - (propertyPrice / 100000) + (careerDelta / 30000) + (savingsRate / 5)));
  const debtRatio = Math.min(65, Math.max(10, Math.round(18.6 + (propertyPrice / 150000) - (careerDelta / 40000))));

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-lg)' }}>
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', flexWrap: 'wrap', gap: 'var(--space-md)' }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <h2 style={{ margin: 0, fontSize: 'var(--text-headline-lg)', fontWeight: 700, letterSpacing: '-0.02em' }}>
              {t('simulator.title')}
            </h2>
            <span className="badge badge--secondary" style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
              <Sparkles size={12} /> Predictive Telemetry
            </span>
          </div>
          <p style={{ color: 'var(--text-muted)', fontSize: 'var(--text-body-md)', marginTop: 4 }}>
            {t('simulator.subtitle')}
          </p>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-sm)' }}>
          <button className="btn-secondary" onClick={resetDefaults} style={{ gap: 6 }}>
            <RotateCcw size={15} />
            {t('simulator.resetDefaults')}
          </button>
          <button className="btn-primary" style={{ gap: 6 }}>
            <Sparkles size={15} />
            {t('simulator.recalculate')}
          </button>
        </div>
      </div>

      {/* Runway Metrics Top Bar (4 Cards) */}
      <section style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))',
        gap: 'var(--space-md)',
      }}>
        {/* Metric 1: Projected 10-Yr Net Worth */}
        <div className="glass-panel" style={{
          padding: 'var(--space-md)',
          borderRadius: 'var(--radius-xl)',
          position: 'relative',
          overflow: 'hidden',
          borderLeft: '3px solid var(--secondary)',
        }}>
          <span style={{ fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)', fontWeight: 600 }}>
            {t('simulator.projected10Yr')}
          </span>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 8, margin: '8px 0 4px' }}>
            <span className="numeral" style={{ fontSize: 'var(--text-headline-lg)', fontWeight: 700, color: 'var(--text-main)' }}>
              €{projectedNetWorth}M
            </span>
            <span className={`badge ${Number(percentageDelta) >= 0 ? 'badge--success' : 'badge--danger'}`} style={{ fontSize: 11, padding: '2px 6px' }}>
              {Number(percentageDelta) >= 0 ? '+' : ''}{percentageDelta}%
            </span>
          </div>
          <span style={{ fontSize: 'var(--text-body-sm)', color: 'var(--text-dim)' }}>
            Baseline: €{baselineNetWorth}M
          </span>
        </div>

        {/* Metric 2: Liquidity Runway */}
        <div className="glass-panel" style={{
          padding: 'var(--space-md)',
          borderRadius: 'var(--radius-xl)',
          borderLeft: '3px solid var(--primary)',
        }}>
          <span style={{ fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)', fontWeight: 600 }}>
            {t('simulator.liquidityRunway')}
          </span>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 8, margin: '8px 0 4px' }}>
            <span className="numeral" style={{ fontSize: 'var(--text-headline-lg)', fontWeight: 700, color: 'var(--text-main)' }}>
              {liquidityMonths} Mos
            </span>
            <span className="badge badge--secondary" style={{ fontSize: 11, padding: '2px 6px' }}>
              Buffer Safe
            </span>
          </div>
          <span style={{ fontSize: 'var(--text-body-sm)', color: 'var(--text-dim)' }}>
            Stress tested against market drawdowns
          </span>
        </div>

        {/* Metric 3: Debt-to-Asset Ratio */}
        <div className="glass-panel" style={{
          padding: 'var(--space-md)',
          borderRadius: 'var(--radius-xl)',
          borderLeft: `3px solid ${debtRatio < 25 ? 'var(--success)' : 'var(--warning)'}`,
        }}>
          <span style={{ fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)', fontWeight: 600 }}>
            {t('simulator.debtToAsset')}
          </span>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 8, margin: '8px 0 4px' }}>
            <span className="numeral" style={{ fontSize: 'var(--text-headline-lg)', fontWeight: 700, color: 'var(--text-main)' }}>
              {debtRatio}%
            </span>
            <span className="badge badge--neutral" style={{ fontSize: 11, padding: '2px 6px' }}>
              Target &lt;25%
            </span>
          </div>
          <span style={{ fontSize: 'var(--text-body-sm)', color: 'var(--text-dim)' }}>
            Institutional leverage ratio
          </span>
        </div>

        {/* Metric 4: Alpha Generation Index */}
        <div className="glass-panel" style={{
          padding: 'var(--space-md)',
          borderRadius: 'var(--radius-xl)',
          borderLeft: '3px solid var(--tertiary)',
        }}>
          <span style={{ fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)', fontWeight: 600 }}>
            {t('simulator.alphaIndex')}
          </span>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 8, margin: '8px 0 4px' }}>
            <span className="numeral" style={{ fontSize: 'var(--text-headline-lg)', fontWeight: 700, color: 'var(--text-main)' }}>
              1.42
            </span>
            <span className="badge badge--success" style={{ fontSize: 11, padding: '2px 6px' }}>
              High Sharpe
            </span>
          </div>
          <span style={{ fontSize: 'var(--text-body-sm)', color: 'var(--text-dim)' }}>
            Risk-adjusted yield efficiency
          </span>
        </div>
      </section>

      {/* Main Grid: Parameters Sliders + Interactive SVG Projection */}
      <section style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(340px, 1fr))',
        gap: 'var(--space-lg)',
      }}>
        {/* Left Column: Sliders Panel */}
        <div className="glass-panel" style={{
          padding: 'var(--space-lg)',
          borderRadius: 'var(--radius-xl)',
          display: 'flex',
          flexDirection: 'column',
          gap: 'var(--space-lg)',
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <SlidersHorizontal size={18} style={{ color: 'var(--secondary)' }} />
              <h3 style={{ margin: 0, fontSize: 'var(--text-headline-sm)' }}>{t('simulator.parameterAdjustments')}</h3>
            </div>
            <span className="badge badge--secondary" style={{ fontSize: 10 }}>{t('simulator.liveSync')}</span>
          </div>

          {/* Slider 1: Property Purchase */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 'var(--text-body-sm)', fontWeight: 500, color: 'var(--text-main)' }}>
                <Building size={14} style={{ color: 'var(--primary)' }} />
                {t('simulator.propertyAcquisition')}
              </label>
              <span className="numeral" style={{ fontWeight: 700, color: 'var(--secondary)' }}>
                €{propertyPrice.toLocaleString('es-ES')}
              </span>
            </div>
            <input
              type="range"
              min={300000}
              max={3000000}
              step={50000}
              value={propertyPrice}
              onChange={(e) => setPropertyPrice(Number(e.target.value))}
              style={{
                width: '100%',
                accentColor: 'var(--secondary)',
                cursor: 'pointer',
              }}
            />
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 'var(--text-label-sm)', color: 'var(--text-dim)' }}>
              <span>€300K</span>
              <span>€3.0M</span>
            </div>
          </div>

          {/* Slider 2: Vehicle Funding */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 'var(--text-body-sm)', fontWeight: 500, color: 'var(--text-main)' }}>
                <Car size={14} style={{ color: 'var(--secondary)' }} />
                {t('simulator.vehicleFunding')}
              </label>
              <span className="numeral" style={{ fontWeight: 700, color: 'var(--secondary)' }}>
                €{vehicleFunding.toLocaleString('es-ES')}
              </span>
            </div>
            <input
              type="range"
              min={20000}
              max={300000}
              step={5000}
              value={vehicleFunding}
              onChange={(e) => setVehicleFunding(Number(e.target.value))}
              style={{
                width: '100%',
                accentColor: 'var(--secondary)',
                cursor: 'pointer',
              }}
            />
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 'var(--text-label-sm)', color: 'var(--text-dim)' }}>
              <span>€20K</span>
              <span>€300K</span>
            </div>
          </div>

          {/* Slider 3: Career Delta */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 'var(--text-body-sm)', fontWeight: 500, color: 'var(--text-main)' }}>
                <Briefcase size={14} style={{ color: 'var(--tertiary)' }} />
                {t('simulator.careerDelta')}
              </label>
              <span className="numeral" style={{ fontWeight: 700, color: 'var(--tertiary)' }}>
                {careerDelta >= 0 ? '+' : ''}€{careerDelta.toLocaleString('es-ES')} / yr
              </span>
            </div>
            <input
              type="range"
              min={-50000}
              max={300000}
              step={10000}
              value={careerDelta}
              onChange={(e) => setCareerDelta(Number(e.target.value))}
              style={{
                width: '100%',
                accentColor: 'var(--tertiary)',
                cursor: 'pointer',
              }}
            />
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 'var(--text-label-sm)', color: 'var(--text-dim)' }}>
              <span>-€50K</span>
              <span>+€300K</span>
            </div>
          </div>

          {/* Slider 4: Household Savings Rate */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 'var(--text-body-sm)', fontWeight: 500, color: 'var(--text-main)' }}>
                <Layers size={14} style={{ color: '#22c55e' }} />
                {t('simulator.savingsRate')}
              </label>
              <span className="numeral" style={{ fontWeight: 700, color: '#22c55e' }}>
                {savingsRate}%
              </span>
            </div>
            <input
              type="range"
              min={10}
              max={70}
              step={5}
              value={savingsRate}
              onChange={(e) => setSavingsRate(Number(e.target.value))}
              style={{
                width: '100%',
                accentColor: '#22c55e',
                cursor: 'pointer',
              }}
            />
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 'var(--text-label-sm)', color: 'var(--text-dim)' }}>
              <span>10%</span>
              <span>70%</span>
            </div>
          </div>

          {/* Toggle Volatility */}
          <div style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            paddingTop: 'var(--space-sm)',
            borderTop: '1px solid var(--border-outline-variant)',
          }}>
            <div style={{ display: 'flex', flexDirection: 'column' }}>
              <span style={{ fontSize: 'var(--text-body-sm)', fontWeight: 600, color: 'var(--text-main)' }}>
                {t('simulator.macroVolatility')}
              </span>
              <span style={{ fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)' }}>
                {t('simulator.volatilitySub')}
              </span>
            </div>
            <input
              type="checkbox"
              checked={includeVolatility}
              onChange={(e) => setIncludeVolatility(e.target.checked)}
              style={{ width: 18, height: 18, accentColor: 'var(--secondary)', cursor: 'pointer' }}
            />
          </div>
        </div>

        {/* Right Column: Comparative Asset Trajectory Chart */}
        <div className="glass-panel" style={{
          padding: 'var(--space-lg)',
          borderRadius: 'var(--radius-xl)',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'space-between',
          gap: 'var(--space-md)',
        }}>
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 8 }}>
              <div>
                <h3 style={{ margin: 0, fontSize: 'var(--text-headline-sm)' }}>{t('simulator.comparativeTrajectory')}</h3>
                <p style={{ margin: '4px 0 0', fontSize: 'var(--text-body-sm)', color: 'var(--text-muted)' }}>
                  {t('simulator.trajectorySubtitle')}
                </p>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
                <span style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 'var(--text-label-sm)', color: 'var(--text-muted)' }}>
                  <span style={{ width: 12, height: 2, background: 'var(--primary)', display: 'inline-block' }} />
                  {t('simulator.baseline')}
                </span>
                <span style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 'var(--text-label-sm)', color: 'var(--secondary)', fontWeight: 600 }}>
                  <span style={{ width: 12, height: 2, background: 'var(--secondary)', display: 'inline-block' }} />
                  {t('simulator.simulated')}
                </span>
              </div>
            </div>

            {/* SVG Visual Graph */}
            <div style={{
              position: 'relative',
              height: 220,
              width: '100%',
              margin: '24px 0 32px',
              borderBottom: '1px solid var(--border-outline-variant)',
              borderLeft: '1px solid var(--border-outline-variant)',
            }}>
              {/* Background Grid Lines */}
              <div style={{ position: 'absolute', inset: 0, display: 'flex', flexDirection: 'column', justifyContent: 'space-between', opacity: 0.15, pointerEvents: 'none' }}>
                <div style={{ borderBottom: '1px dashed var(--text-muted)' }} />
                <div style={{ borderBottom: '1px dashed var(--text-muted)' }} />
                <div style={{ borderBottom: '1px dashed var(--text-muted)' }} />
                <div style={{ borderBottom: '1px dashed var(--text-muted)' }} />
              </div>

              {/* Curve Overlay */}
              <svg viewBox="0 0 500 200" preserveAspectRatio="none" style={{ position: 'absolute', inset: 0, width: '100%', height: '100%', overflow: 'visible' }}>
                <defs>
                  <linearGradient id="simGlow" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="#4cd7f6" stopOpacity="0.25" />
                    <stop offset="100%" stopColor="#4cd7f6" stopOpacity="0.0" />
                  </linearGradient>
                </defs>

                {/* Baseline Line */}
                <path
                  d="M 0,160 Q 125,140 250,110 T 500,70"
                  fill="none"
                  stroke="#adc6ff"
                  strokeWidth="2"
                  strokeDasharray="4 4"
                  opacity="0.6"
                />

                {/* Simulated Glow Area */}
                <path
                  d={`M 0,165 Q 125,120 250,85 T 500,${Math.max(15, 70 - Number(percentageDelta) * 1.5)} L 500,200 L 0,200 Z`}
                  fill="url(#simGlow)"
                />

                {/* Simulated Curve */}
                <path
                  d={`M 0,165 Q 125,120 250,85 T 500,${Math.max(15, 70 - Number(percentageDelta) * 1.5)}`}
                  fill="none"
                  stroke="#4cd7f6"
                  strokeWidth="3"
                />

                {/* Terminal Point */}
                <circle
                  cx="500"
                  cy={Math.max(15, 70 - Number(percentageDelta) * 1.5)}
                  r="5"
                  fill="#4cd7f6"
                  stroke="#0f131d"
                  strokeWidth="2"
                />
              </svg>

              {/* X-Axis Labels */}
              <div style={{
                position: 'absolute',
                bottom: -24,
                left: 0,
                right: 0,
                display: 'flex',
                justifyContent: 'space-between',
                fontSize: 'var(--text-label-sm)',
                color: 'var(--text-dim)',
              }}>
                <span>Year 1</span>
                <span>Year 3</span>
                <span>Year 5</span>
                <span>Year 7</span>
                <span>Year 10</span>
              </div>
            </div>
          </div>

          {/* Bottom Telemetry Footer */}
          <div style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            paddingTop: 'var(--space-md)',
            borderTop: '1px solid var(--border-outline-variant)',
            flexWrap: 'wrap',
            gap: 8,
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 'var(--text-body-sm)', color: 'var(--text-muted)' }}>
              <CheckCircle2 size={16} style={{ color: 'var(--success)' }} />
              {t('simulator.confidenceInterval')}
            </div>
            <button className="btn-secondary" style={{ gap: 6 }}>
              <Download size={14} />
              {t('simulator.exportMatrix')}
            </button>
          </div>
        </div>
      </section>
    </div>
  );
}
