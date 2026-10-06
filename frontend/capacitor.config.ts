import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.nexus.app',
  appName: 'Nexus',
  webDir: 'dist',
  server: {
    androidScheme: 'https',
  },
  plugins: {
    App: {
      deepLinking: {
        scheme: 'nexus',
      },
    },
  },
};

export default config;
