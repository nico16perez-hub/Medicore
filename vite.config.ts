import { defineConfig } from 'vitest/config';
import vue from '@vitejs/plugin-vue';
import path from 'path';

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src')
    }
  },
  server: {
    watch: {
      ignored: ['**/Medicore.Api/bin/**', '**/Medicore.Api/obj/**']
    }
  },
  test: {
    environment: 'jsdom',
    globals: true
  }
});
