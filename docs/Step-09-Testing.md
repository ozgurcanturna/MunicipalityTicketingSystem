# Step 09: Testing - Unit ve Integration Test Altyapısı

## 🎯 Amaç
Bu adımda proje, sadece derlenen bir kod tabanı olmaktan çıkarılıp iş kurallarını doğrulayan bir test katmanına taşınır.

Bu adım sonunda:
- Unit testler domain davranışlarını kapsar.
- Integration testler altyapı bileşenlerinin birlikte çalışma davranışını doğrular.
- Test projeleri gerçek servis projelerine bağlı hale gelir.

---

## ✅ Önkoşullar
- Step 00-08 tamamlanmış olmalı.

---

## 1) Test Projelerini Servislere Bağlama

### 1.1 Unit test proje referansları
Dosya: tests/MunicipalityTicketing.UnitTests/MunicipalityTicketing.UnitTests.csproj

Eklenen referanslar:
- services/identity/Tenant.Identity.Api.csproj
- services/wallet/Ticketing.Wallet.Api.csproj
- services/telemetry/Journey.Telemetry.Api.csproj

### 1.2 Integration test proje referansları
Dosya: tests/MunicipalityTicketing.IntegrationTests/MunicipalityTicketing.IntegrationTests.csproj

Eklenen referanslar:
- core/SharedKernel/Infrastructure/SharedKernel.Infrastructure.csproj
- workers/event-processor/Journey.EventProcessor.Worker.csproj

---

## 2) Unit Test Kapsamı

### 2.1 Identity domain testleri
Dosya: tests/MunicipalityTicketing.UnitTests/Identity/MunicipalityTenantTests.cs

Senaryolar:
1. Tenant adı boşsa create hata vermeli
2. Aynı email ikinci kez eklenirse hata vermeli
3. Pasif tenant'a kullanıcı eklenememeli

### 2.2 Wallet domain testleri
Dosya: tests/MunicipalityTicketing.UnitTests/Wallet/WalletAccountTests.cs

Senaryolar:
1. TopUp bakiyeyi artırmalı ve transaction oluşturmalı
2. Yetersiz bakiye spend işleminde hata üretmeli
3. Yeterli bakiye spend işleminde bakiye düşmeli

### 2.3 Telemetry domain testleri
Dosya: tests/MunicipalityTicketing.UnitTests/Telemetry/JourneySessionTests.cs

Senaryolar:
1. Check-in/check-out passenger count davranışı
2. Boş source ile location update hatası
3. Complete sonrası journey durumu

---

## 3) Integration Test Kapsamı

### 3.1 Tenant connection resolver testleri
Dosya: tests/MunicipalityTicketing.IntegrationTests/Infrastructure/TenantConnectionStringResolverTests.cs

Senaryolar:
1. Tenant-specific connection varsa onu döndürmeli
2. Yoksa default bağlantıya fallback etmeli

### 3.2 Event handler resolver testleri
Dosya: tests/MunicipalityTicketing.IntegrationTests/EventProcessor/EventHandlerResolverTests.cs

Senaryolar:
1. EventType case-insensitive çözümleme
2. Bilinmeyen event type için null dönüş

---

## 4) Doğrulama

```powershell
dotnet build tests/MunicipalityTicketing.UnitTests/MunicipalityTicketing.UnitTests.csproj
dotnet build tests/MunicipalityTicketing.IntegrationTests/MunicipalityTicketing.IntegrationTests.csproj
dotnet test MunicipalityTicketing.slnx
```

Beklenen sonuç:
- Tüm test projeleri build olmalı
- Testler başarılı olmalı

---

## 5) Tamamlanma Kontrol Listesi

- [x] Unit testler gerçek domain senaryolarına göre yazıldı.
- [x] Integration testler altyapı davranışını doğruluyor.
- [x] Test projeleri hedef servislere referanslandı.
- [x] Şablon test dosyaları yerine anlamlı testler eklendi.
- [x] Build/test doğrulaması tamamlandı.

---

## 6) Step 10 İçin İhtiyaç Analizi (Simulation Clients)

### 6.1 İhtiyaçlar
1. Yük testi için eşzamanlı istemci üretimi
2. Tenant bazlı trafik profilleri
3. API Gateway üzerinden uçtan uca senaryo koşumu

### 6.2 Teknik Backlog
1. tools/simulator içinde senaryo motoru
2. Rastgele ama deterministik test verisi üretimi
3. Sonuç metriklerinin raporlanması (latency, success rate)

### 6.3 Riskler ve Aksiyonlar
1. Risk: Simülasyon gerçek üretim yükünü yansıtmayabilir
Aksiyon: Trafik modelini gerçek belediye kullanımına göre kalibre et.

2. Risk: Test ortamı dar boğazları yanlış yorumlatabilir
Aksiyon: Her test koşusuna çevresel metrikleri ekle.

3. Risk: Tenant dağılımı dengesiz kalabilir
Aksiyon: Tenant bazlı ağırlıklı yük profili uygula.

---

**Durum**: ✅ Tamamlandı (Doğrulandı)
**Son Güncelleme**: 11.06.2026
**Yazar**: Özgür Can TURNA