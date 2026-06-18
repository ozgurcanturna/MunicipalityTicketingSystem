import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { fileURLToPath } from 'node:url';
export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url)),
        },
    },
    build: {
        minify: 'esbuild',
        sourcemap: false,
        rollupOptions: {
            output: {
                manualChunks: {
                    vendor: ['react', 'react-dom', 'react-router-dom'],
                    lib: ['axios', 'zustand', '@tanstack/react-query'],
                },
            },
        },
    },
    server: {
        port: 3000,
        strictPort: true,
        hmr: {
            protocol: 'ws',
            host: 'localhost',
        },
        proxy: {
            '/api': {
                target: 'http://localhost:5197',
                changeOrigin: true,
            },
            '/hubs': {
                target: 'http://localhost:5197',
                ws: true,
                changeOrigin: true,
            },
            '/signalr': {
                target: 'http://localhost:5197',
                ws: true,
                changeOrigin: true,
            },
        },
    },
});
