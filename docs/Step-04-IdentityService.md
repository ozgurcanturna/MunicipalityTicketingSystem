# Step 04: Identity Service - Tenant ve User Altyapısı

## 🎯 Amaç
Bu adımda Identity servisi, Shared Kernel ve Step 03 altyapısını kullanarak gerçek bir domain modeli ve veri erişim katmanı ile çalışır hale getilir.

Bu adım sonunda:
- Tenant ve User domain modeli oluşur.
- IdentityDbContext ve TenantRepository hazır olur.
- X-Tenant-Id header'ından tenant okuyan provider devreye alınır.
- Tenant oluşturma, tenant getirme ve tenant'a kullanıcı ekleme endpointleri çalışır.

---

## ✅ Önkoşullar
- Step 00 tamamlanmış olmalı.
- Step 01 tamamlanmış olmalı.
- Step 02 tamamlanmış olmalı.
- Step 03 tamamlanmış olmalı.

---

## 1) Identity Domain Modeli

### 1.1 Tenant Aggregate
Dosya: services/identity/Domain/Entities/Tenant.cs

Not: Kod tarafında sınıf adı MunicipalityTenant olarak tanımlanmıştır.

Sorumluluklar:
- Tenant oluşturma
- Tenant aktif/pasif yönetimi
- Tenant içine user ekleme
- Domain event tetikleme (RegisterCreated/RegisterUpdated)

### 1.2 User Entity
Dosya: services/identity/Domain/Entities/User.cs

Sorumluluklar:
- TenantId, Email, FullName yönetimi
- Login zamanı güncelleme
- Kullanıcı pasifleştirme

---

## 2) Repository ve Sözleşmeler

### 2.1 ITenantRepository
Dosya: services/identity/Application/Repositories/ITenantRepository.cs

Shared repository sözleşmesini genişletir ve tenant adına göre sorgu imkanı sağlar.

### 2.2 TenantRepository
Dosya: services/identity/Infrastructure/Repositories/TenantRepository.cs

Generic Repository altyapısını kullanır ve Tenant -> Users eager loading yapar.

---

## 3) Persistence Katmanı

### 3.1 IdentityDbContext
Dosya: services/identity/Infrastructure/Persistence/IdentityDbContext.cs

Özellikler:
- AppDbContext'ten türetilir
- Tenants ve Users DbSet tanımları içerir
- Fluent API ile tablo/indeks/ilişki konfigürasyonu yapar

Kurallar:
- Tenant.Name unique index
- User için TenantId + Email unique index
- Tenant silinirse kullanıcıları cascade silinir

---

## 4) Multi-Tenancy Entegrasyonu

### 4.1 HttpHeaderTenantProvider
Dosya: services/identity/Infrastructure/MultiTenancy/HttpHeaderTenantProvider.cs

X-Tenant-Id header değerini okuyarak SharedKernel Infrastructure içindeki ITenantProvider sözleşmesini doldurur.

Güncel davranış:
- API katmanında (Program.cs middleware) X-Tenant-Id zorunludur.
- Header yoksa servis 400 BadRequest döner.
- Provider null dönebilse de bu durum HTTP isteklerinde middleware tarafından engellenir.

---

## 5) API Sözleşmeleri ve Endpointler

### 5.1 Request/Response modelleri
Dosyalar:
- services/identity/Application/Contracts/CreateTenantRequest.cs
- services/identity/Application/Contracts/AddUserRequest.cs
- services/identity/Application/Contracts/TenantResponse.cs
- services/identity/Application/Contracts/UserResponse.cs

### 5.2 Program.cs endpointleri
Dosya: services/identity/Program.cs

Endpointler:
1. GET /
- Health mesajı döner

2. POST /tenants
- Tenant oluşturur

3. GET /tenants/{id}
- Tenant detayını döner

4. POST /tenants/{id}/users
- Tenant'a user ekler

Servis kayıtları:
- AddSharedInfrastructure<IdentityDbContext>
- ITenantProvider -> HttpHeaderTenantProvider
- ITenantRepository -> TenantRepository

---

## 6) Doğrulama

```powershell
dotnet build services/identity/Tenant.Identity.Api.csproj
dotnet build MunicipalityTicketing.slnx
dotnet test MunicipalityTicketing.slnx
```

Beklenen sonuç: build ve test adımları başarılı.

---

## 7) Tamamlanma Kontrol Listesi

- [x] Tenant aggregate ve User entity eklendi.
- [x] IdentityDbContext eklendi.
- [x] ITenantRepository ve TenantRepository eklendi.
- [x] HttpHeaderTenantProvider eklendi.
- [x] Identity API için X-Tenant-Id zorunluluğu middleware ile eklendi.
- [x] Identity Program.cs DI ve endpointlerle güncellendi.
- [x] Request/response sözleşmeleri eklendi.
- [x] Build ve test doğrulandı.

---

## 8) Step 05 İçin İhtiyaç Analizi (Wallet Service)

Step 05'e geçiş için net ihtiyaçlar:

### 8.1 Domain İhtiyaçları
1. Wallet aggregate (bakiye yönetimi)
2. Transaction entity (yükleme/harcama geçmişi)
3. Money benzeri value object (opsiyonel)

### 8.2 Altyapı İhtiyaçları
1. WalletDbContext
2. IWalletRepository ve implementasyonu
3. Tenant-aware provider'ın Wallet servisinde de kullanılması

### 8.3 API İhtiyaçları
1. Bakiye sorgulama endpointi
2. Para yükleme endpointi
3. Bilet alımı için bakiye düşme endpointi
4. İşlem geçmişi endpointi

### 8.4 Riskler ve Aksiyonlar
1. Risk: Eşzamanlı bakiye güncellemelerinde yarış durumu
Aksiyon: Optimistic concurrency veya transactional update stratejisi ekle.

2. Risk: Finansal işlemlerde idempotency eksikliği
Aksiyon: Idempotency key ve unique işlem kimliği tasarla.

3. Risk: Tenant header manipülasyonu
Aksiyon: Header + JWT claim doğrulamasını birlikte kullan.

---

**Durum**: ✅ Tamamlandı (Doğrulandı)
**Son Güncelleme**: 11.06.2026
**Yazar**: Özgür Can TURNA