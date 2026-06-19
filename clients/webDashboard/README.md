# Municipality Ticketing Web Dashboard

Belediye Otobüs Bilet Sistemi - React 19 + TypeScript + Vite yönetim paneli.

## Mevcut MVP Durumu

Web Dashboard şu anda temel React/Vite uygulamasıdır:

- Login sayfası
- Protected routing
- Layout/sidebar/header yapısı
- Dashboard, journeys, buses, reports, users, settings placeholder sayfaları
- Axios tabanlı API client taslağı
- SignalR başlangıç entegrasyonu
- Playwright Chromium E2E testleri

Henüz tamamlanmamış hedefler:

- shadcn/ui kurulumu
- React Hook Form + Zod validation
- httpOnly cookie tabanlı token storage
- refresh token akışı
- gerçek dashboard API entegrasyonu
- gerçek kullanıcı/bilet/rota/filo modülleri
- production-ready RBAC route koruması

## Teknoloji Stack

- React 19
- TypeScript 5.6+
- Vite 6
- React Router 7
- TanStack Query 5
- Zustand
- Axios
- SignalR
- Tailwind CSS 4
- Recharts
- Lucide React
- Playwright

## Kurulum

### Gereksinimler

- Node.js 20+
- npm

### Adımlar

```bash
npm install
```

### Ortam değişkenleri

Proje kökünde `.env` dosyası oluştur:

```env
VITE_API_URL=http://localhost:5197
VITE_SIGNALR_URL=http://localhost:5197/hubs/telemetry
```

Not: `VITE_API_URL` API Gateway adresidir.

## Komutlar

```bash
# Geliştirme sunucusu
npm run dev

# Production build
npm run build

# Lint
npm run lint

# Playwright Chromium E2E testleri
npm test -- --project=chromium
```

## Backend bağlantısı

Dashboard, varsayılan olarak API Gateway'e bağlanır:

```text
http://localhost:5197
```

Gateway çalışmıyorsa login backend çağrısı başarısız olur ve uygulama mevcut demo/local auth akışına düşer.

## Dokümanlar

- `PROJECT_ANALYSIS.md`: Mevcut dashboard mimarisi, eksikler ve öncelikli düzeltmeler
- `docs/Step-13-WebDashboard.md`: Web Dashboard hedefleri, mevcut MVP ve roadmap
