import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { loadEnv } from 'vite';
import { defineConfig } from 'vitest/config';

const DEFAULT_API_PROXY_TARGET = 'http://localhost:5297';
const API_PROXY_PREFIX = '/api';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const apiProxyTarget = env.VITE_API_PROXY_TARGET ?? DEFAULT_API_PROXY_TARGET;

  const apiProxy = {
    target: apiProxyTarget,
    changeOrigin: true,
    secure: false,
    rewrite: (path: string) => path.replace(new RegExp(`^${API_PROXY_PREFIX}`), ''),
  };

  return {
    plugins: [react(), tailwindcss()],
    server: {
      port: 5173,
      proxy: { [API_PROXY_PREFIX]: apiProxy },
    },
    preview: {
      port: 4173,
      proxy: { [API_PROXY_PREFIX]: apiProxy },
    },
    test: {
      environment: 'jsdom',
      globals: true,
      setupFiles: ['./src/test/setup.ts'],
      restoreMocks: true,
      unstubGlobals: true,
      coverage: {
        provider: 'v8',
        enabled: true,
        reporter: ['text-summary', 'lcov'],
        include: ['src/**/*.{ts,tsx}'],
        exclude: ['src/main.tsx', 'src/test/**', 'src/**/*.test.{ts,tsx}'],
        thresholds: {
          statements: 80,
          branches: 80,
          functions: 80,
          lines: 80,
        },
      },
    },
  };
});
