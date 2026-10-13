import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import basicSsl from '@vitejs/plugin-basic-ssl';

const target = process.env.STOCKRAY_BACKEND_URL || 'https://localhost:7165';
const paths = ['/public', '/account', '/antiforgery', '/register', '/login', '/logout', '/AddSymbol', '/RemoveSymbol', '/GetSymbols', '/GetAllSymbols', '/sym-notif', '/Styles', '/Assets'];

export default defineConfig({
  plugins: [react(), basicSsl()],
  base: '/',
  server: {
    port: 5173,
    strictPort: true,
    proxy: Object.fromEntries(paths.map(path => [path, {
      target,
      secure: false,
      ws: path === '/sym-notif',
      // Uppercase /Login/ and /Register/ are client routes, not API requests.
      bypass(req) {
        if (req.method === 'GET' && /^\/(Login|Register)(\/|$)/.test(req.url || '')) return '/index.html';
      }
    }]))
  },
  build: {
    outDir: 'dist',
    assetsDir: 'spa/assets',
    manifest: true
  }
});
