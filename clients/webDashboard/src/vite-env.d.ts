/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL: string
  readonly VITE_API_BASE_URL: string
  readonly VITE_SIGNALR_URL: string
  readonly VITE_APP_NAME: string
  readonly VITE_APP_VERSION: string
  readonly VITE_TOKEN_EXPIRY: string
  readonly VITE_REFRESH_TOKEN_EXPIRY: string
  readonly VITE_ENABLE_REALTIME_UPDATES: string
  readonly VITE_ENABLE_DEBUG_MODE: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
