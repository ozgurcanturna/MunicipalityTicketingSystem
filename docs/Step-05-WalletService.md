# Step 05: Wallet Service - Bakiye ve İşlem Yönetimi

## 🎯 Amaç

Bu adımda Wallet servisi, tenant bazlı cüzdan yönetimi yapacak şekilde tamamlanır.

Bu adım sonunda:

- Wallet aggregate ve transaction entity modeli hazır olur.
- WalletDbContext ve WalletRepository hazır olur.
- Wallet API endpointleri (oluşturma, bakiye yükleme, harcama, geçmiş) çalışır.
- X-Tenant-Id header üzerinden tenant-aware altyapı devrede olur.

---

## ✅ Önkoşullar

- Step 00, Step 01, Step 02, Step 03, Step 04 tamamlanmış olmalı.

---

## 1) Domain Modeli

### 1.1 WalletAccount Aggregate

Dosya: services/wallet/Domain/Entities/WalletAccount.cs

Sorumluluklar:

- Wallet oluşturma
- Bakiye yükleme (TopUp)
- Bakiye düşme (Spend)
- İşlem listesi yönetimi

İş kuralları:

- Amount > 0 olmalı
- Spend işleminde bakiye yetersizse hata dönmeli
- Reference boş olamaz

### 1.2 WalletTransaction Entity

Dosya: services/wallet/Domain/Entities/WalletTransaction.cs

Her cüzdan hareketini temsil eder.

### 1.3 WalletTransactionType sabitleri

Dosya: services/wallet/Domain/Entities/WalletTransactionType.cs

Tipler:

- TOP_UP
- SPEND

---

## 2) Repository Sözleşmesi ve Implementasyonu

### 2.1 IWalletRepository

Dosya: services/wallet/Application/Repositories/IWalletRepository.cs

Ek sorgu:

- TenantId üzerinden wallet bulma

### 2.2 WalletRepository

Dosya: services/wallet/Infrastructure/Repositories/WalletRepository.cs

Özellikler:

- Wallet + Transactions eager loading
- GetById için include
- GetByTenantId için include

---

## 3) Persistence Katmanı

### 3.1 WalletDbContext

Dosya: services/wallet/Infrastructure/Persistence/WalletDbContext.cs

Kurallar:

- Wallet.TenantId unique index
- WalletTransaction için WalletId index
- WalletTransaction için OccurredAt index
- Amount alanları decimal(18,2)
- Wallet -> Transactions cascade delete

---

## 4) Multi-Tenancy Provider

### 4.1 HttpHeaderTenantProvider

Dosya: services/wallet/Infrastructure/MultiTenancy/HttpHeaderTenantProvider.cs

X-Tenant-Id header değerini okuyup ITenantProvider sözleşmesini doldurur.

Güncel davranış:

- Wallet API katmanında X-Tenant-Id header zorunludur.
- Header eksik ise servis 400 BadRequest döner.

---

## 5) API Sözleşmeleri ve Endpointler

### 5.1 Contract dosyaları

Dosyalar:

- services/wallet/Application/Contracts/CreateWalletRequest.cs
- services/wallet/Application/Contracts/TopUpWalletRequest.cs
- services/wallet/Application/Contracts/SpendRequest.cs
- services/wallet/Application/Contracts/WalletResponse.cs
- services/wallet/Application/Contracts/WalletTransactionResponse.cs

### 5.2 Program.cs endpointleri

Dosya: services/wallet/Program.cs

Endpointler:

1. GET /
2. POST /wallets
3. GET /wallets/{id}
4. POST /wallets/{id}/topups
5. POST /wallets/{id}/spend
6. GET /wallets/{id}/transactions

DI kayıtları:

- AddSharedInfrastructure<WalletDbContext>
- ITenantProvider -> HttpHeaderTenantProvider
- IWalletRepository -> WalletRepository

---

## 6) Doğrulama

```powershell
dotnet build services/wallet/Ticketing.Wallet.Api.csproj
dotnet build MunicipalityTicketing.slnx
dotnet test MunicipalityTicketing.slnx
```

Beklenen sonuç: tüm build/test adımları başarılı.

---

## 7) Tamamlanma Kontrol Listesi

- [x] Wallet domain modeli eklendi.
- [x] WalletDbContext eklendi.
- [x] IWalletRepository ve WalletRepository eklendi.
- [x] Wallet API endpointleri eklendi.
- [x] Multi-tenant provider eklendi.
- [x] Wallet API için X-Tenant-Id zorunluluğu middleware ile eklendi.
- [x] Build ve test doğrulaması yapıldı.

---

## 8) Step 06 İçin İhtiyaç Analizi (Telemetry Service)

### 8.1 Domain İhtiyaçları

1. Journey aggregate
2. VehicleLocation ve Checkpoint event modeli
3. Rota ve durak ilişkileri

### 8.2 Altyapı İhtiyaçları

1. TelemetryDbContext
2. Geospatial veri saklama stratejisi
3. Event stream/ingestion modeli

### 8.3 API İhtiyaçları

1. Konum güncelleme endpointi
2. Check-in/check-out endpointleri
3. Aktif yolculuk sorgulama endpointleri

### 8.4 Riskler ve Aksiyonlar

1. Risk: Yüksek frekansta konum yazımı DB'yi zorlayabilir
Aksiyon: Batch insert veya queue tabanlı ingest planı.

2. Risk: Zaman damgası tutarsızlıkları
Aksiyon: UTC zorunluluğu ve ingestion anında normalize etme.

3. Risk: Tenant izolasyonunun sorgularda atlanması
Aksiyon: Telemetry sorgularında tenant filtresini zorunlu hale getirme.

---

**Durum**: ✅ Tamamlandı (Doğrulandı)
**Son Güncelleme**: 11.06.2026
**Yazar**: Özgür Can TURNA
