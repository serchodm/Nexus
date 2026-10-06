import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import { en } from './locales/en';
import { es } from './locales/es';
import { zh } from './locales/zh';
import { hi } from './locales/hi';
import { fr } from './locales/fr';

export const SUPPORTED_LANGUAGES = [
  { code: 'en', label: 'English', flag: '🇬🇧', short: 'EN' },
  { code: 'es', label: 'Español', flag: '🇪🇸', short: 'ES' },
  { code: 'zh', label: '中文 (Mandarin)', flag: '🇨🇳', short: 'ZH' },
  { code: 'hi', label: 'हिन्दी (Hindi)', flag: '🇮🇳', short: 'HI' },
  { code: 'fr', label: 'Français', flag: '🇫🇷', short: 'FR' },
] as const;

export type SupportedLanguageCode = typeof SUPPORTED_LANGUAGES[number]['code'];

const savedLang = (typeof window !== 'undefined' && localStorage.getItem('nexus_lang')) || 'es';

i18n
  .use(initReactI18next)
  .init({
    resources: {
      en: { translation: en },
      es: { translation: es },
      zh: { translation: zh },
      hi: { translation: hi },
      fr: { translation: fr },
    },
    lng: savedLang,
    fallbackLng: 'en',
    interpolation: {
      escapeValue: false,
    },
  });

export const changeLanguage = (lang: SupportedLanguageCode) => {
  i18n.changeLanguage(lang);
  if (typeof window !== 'undefined') {
    localStorage.setItem('nexus_lang', lang);
  }
};

export default i18n;
