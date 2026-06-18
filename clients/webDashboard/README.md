# Municipality Ticketing Web Dashboard

Belediye Otobüs Bilet Sistemi - Yönetim Paneli

## Teknoloji Stack

- **React 19** - UI Framework
- **TypeScript 5.6+** - Type Safety
- **Vite 6** - Build Tool
- **Zustand** - Client State Management
- **TanStack Query 5** - Server State & Caching
- **React Router 7** - Client-side Routing
- **shadcn/ui** - Component Library
- **Tailwind CSS 4** - Styling
- **Axios** - HTTP Client
- **Recharts** - Charts & Analytics
- **SignalR** - Real-time Updates

## Kurulum

### Gereksinimler

- Node.js 20+ 
- npm veya yarn

### Adımlar

```bash
# Bağımlılıkları yükle
npm install

# Ortam değişkenlerini yapılandır
cp .env.example .env
# .env dosyasını düzenle

# Geliştirme sunucusunu başlat
npm run dev
```

## Proje Yapısı

```
src/
├── components/       # Paylaşılan UI bileşenleri
│   ├── ui/          # shadcn/ui bileşenleri
│   └── layout/      # Layout bileşenleri
├── features/         # Özellik bazlı modüller
│   ├── auth/        # Kimlik doğrulama
│   ├── dashboard/   # Ana panel
│   ├── users/       # Kullanıcı yönetimi
│   ├── tickets/     # Bilet yönetimi
│   ├── routes/      # Rota yönetimi
│   ├── fleet/       # Filo yönetimi
│   └── reports/     # Raporlar
├── hooks/           # Özel React hook'ları
├── stores/          # Zustand store'lar
├── services/        # API servisleri
├── types/           # TypeScript tipleri
├── utils/           # Yardımcı fonksiyonlar
├── lib/             # Kütüphane yapılandırmaları
├── App.tsx          # Ana uygulama
└── main.tsx         # Giriş noktası
```

## Komutlar

```bash
# Geliştirme sunucusu
npm run dev

# Production build
npm run build

# Preview build
npm run preview

# Lint
npm run lint
```

## API Entegrasyonu

Proje, .NET backend ile YARP Gateway üzerinden iletişim kurar:

- **API Base URL**: `/api` (Vite proxy ile `/api/*` istekleri backend'e yönlendirilir)
- **SignalR**: `/signalr` (real-time güncellemeler için)
- **Authentication**: JWT token + Refresh token
- **Multi-tenant**: Tenant ID header ile

## Geliştirme İlkeleri

- React 19 best practices
- TypeScript strict mode
- Feature-based organization
- DDD + CQRS pattern (backend ile uyumlu)
- Responsive design
- Accessibility (WCAG 2.1 AA)

## Dokümantasyon

- [React 19 Best Practices](../../docs/React19-BestPractices.md)
- [React 19 API Reference](../../docs/React19-API-Reference.md)
- [Web Dashboard Planlama](../../docs/Step-13-WebDashboard.md)
- [Project Skills](../../docs/skills.md)

## Lisans

MIT
