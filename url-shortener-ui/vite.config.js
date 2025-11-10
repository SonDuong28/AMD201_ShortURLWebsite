import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    port: 5173,
    strictPort: true,
    historyApiFallback: true,
    // Dev-time proxy to avoid CORS in development. Requests starting with /api
    // will be forwarded to your backend server. Change target if your backend
    // runs on a different port (e.g. 5000).
    proxy: {
      '/api': {
        target: 'http://localhost:5114',
        changeOrigin: true,
        secure: false,
        ws: false
      }
    }
  },
})
