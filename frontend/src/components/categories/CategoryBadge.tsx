import React from 'react';
import {
  Utensils,
  ShoppingCart,
  Coffee,
  Bike,
  Home,
  Zap,
  Droplet,
  Wifi,
  Hammer,
  Car,
  Bus,
  Navigation,
  CircleParking,
  Wrench,
  Tv,
  Play,
  Cloud,
  Dumbbell,
  Sparkles,
  Plane,
  Film,
  HeartPulse,
  Wallet,
  Receipt,
  Dog,
  Tag,
  type LucideIcon,
} from 'lucide-react';

const ICON_MAP: Record<string, LucideIcon> = {
  utensils: Utensils,
  'shopping-cart': ShoppingCart,
  coffee: Coffee,
  bike: Bike,
  home: Home,
  zap: Zap,
  droplet: Droplet,
  wifi: Wifi,
  hammer: Hammer,
  car: Car,
  bus: Bus,
  navigation: Navigation,
  'circle-parking': CircleParking,
  wrench: Wrench,
  tv: Tv,
  play: Play,
  cloud: Cloud,
  dumbbell: Dumbbell,
  sparkles: Sparkles,
  plane: Plane,
  film: Film,
  'heart-pulse': HeartPulse,
  wallet: Wallet,
  receipt: Receipt,
  dog: Dog,
  tag: Tag,
};

interface CategoryBadgeProps {
  categoryName?: string | null;
  subcategoryName?: string | null;
  colorHex?: string | null;
  icon?: string | null;
  size?: 'sm' | 'md';
}

export const CategoryBadge: React.FC<CategoryBadgeProps> = ({
  categoryName,
  subcategoryName,
  colorHex = '#64748B',
  icon = 'tag',
  size = 'md',
}) => {
  if (!categoryName) {
    return (
      <span
        style={{
          display: 'inline-flex',
          alignItems: 'center',
          gap: '0.35rem',
          padding: size === 'sm' ? '0.15rem 0.45rem' : '0.25rem 0.65rem',
          borderRadius: 'var(--radius-full)',
          background: 'hsla(0, 0%, 100%, 0.05)',
          color: 'var(--text-dim)',
          fontSize: size === 'sm' ? '0.7rem' : '0.75rem',
          border: '1px dashed var(--border-subtle)',
        }}
      >
        <Tag size={size === 'sm' ? 12 : 14} />
        Sin Categoría
      </span>
    );
  }

  const IconComponent = (icon && ICON_MAP[icon.toLowerCase()]) || Tag;
  const baseColor = colorHex || '#3B82F6';

  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: '0.4rem',
        padding: size === 'sm' ? '0.2rem 0.5rem' : '0.3rem 0.75rem',
        borderRadius: 'var(--radius-full)',
        backgroundColor: `${baseColor}18`, // 10% opacidad en hex
        border: `1px solid ${baseColor}40`,
        color: baseColor,
        fontWeight: 600,
        fontSize: size === 'sm' ? '0.72rem' : '0.8rem',
        whiteSpace: 'nowrap',
        boxShadow: `0 0 12px -2px ${baseColor}20`,
      }}
    >
      <IconComponent size={size === 'sm' ? 13 : 15} />
      <span>
        {categoryName}
        {subcategoryName && (
          <span style={{ opacity: 0.85, fontWeight: 500, marginLeft: '0.25rem' }}>
            • {subcategoryName}
          </span>
        )}
      </span>
    </span>
  );
};
