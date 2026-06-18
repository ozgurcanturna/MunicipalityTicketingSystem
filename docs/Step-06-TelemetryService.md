# Step 06: Telemetry Service - Yolculuk ve Konum Takibi

## 🎯 Amaç

Bu adımda Telemetry servisi, araç yolculuklarını tenant bazlı olarak takip edecek şekilde tamamlanır.

Bu adım sonunda:

- JourneySession aggregate ve JourneyCheckpoint entity modeli hazır olur.
- TelemetryDbContext ve JourneyRepository hazır olur.
- Konum güncelleme, check-in/check-out ve aktif yolculuk sorgu endpointleri çalışır.

---

## ✅ Önkoşullar

- Step 00, 01, 02, 03, 04, 05 tamamlanmış olmalı.

---

## 1) Domain Modeli

### 1.1 JourneySession Aggregate

Dosya: services/telemetry/Domain/Entities/JourneySession.cs

Sorumluluklar:

- Yolculuk başlatma
- Konum güncelleme
- Check-in/check-out sayımı
- Yolculuk tamamlama

İş kuralları:

- Tamamlanmış yolculukta işlem yapılamaz.
- CardId ve StopCode boş olamaz.
- PassengerCount negatife düşmez.

### 1.2 JourneyCheckpoint Entity

Dosya: services/telemetry/Domain/Entities/JourneyCheckpoint.cs

Yolculuk sırasındaki olay kayıtlarını temsil eder:

- LOCATION_UPDATED
- CHECK_IN
- CHECK_OUT

### 1.3 Checkpoint tip sabitleri

Dosya: services/telemetry/Domain/Entities/JourneyCheckpointType.cs

---

## 2) Repository Katmanı

### 2.1 IJourneyRepository

Dosya: services/telemetry/Application/Repositories/IJourneyRepository.cs

Ek sorgu:

- Araç bazlı aktif yolculuk bulma

### 2.2 JourneyRepository

Dosya: services/telemetry/Infrastructure/Repositories/JourneyRepository.cs

Özellikler:

- Journey + Checkpoints eager loading
- GetById için include
- GetActiveByVehicleId için include

---

## 3) Persistence Katmanı

### 3.1 TelemetryDbContext

Dosya: services/telemetry/Infrastructure/Persistence/TelemetryDbContext.cs

Kurallar:

- JourneySessions tablosu
- JourneyCheckpoints tablosu
- TenantId + VehicleId + IsActive index
- JourneySession -> JourneyCheckpoint cascade delete
- Checkpoint için OccurredAt index

---

## 4) Multi-Tenancy

### 4.1 HttpHeaderTenantProvider

Dosya: services/telemetry/Infrastructure/MultiTenancy/HttpHeaderTenantProvider.cs

X-Tenant-Id header bilgisini okuyup ITenantProvider sözleşmesine map eder.

Güncel davranış:

- Telemetry API katmanında X-Tenant-Id header zorunludur.
- Header eksik ise servis 400 BadRequest döner.

---

## 5) API Sözleşmeleri ve Endpointler

### 5.1 Contract dosyaları

Dosyalar:

- services/telemetry/Application/Contracts/StartJourneyRequest.cs
- services/telemetry/Application/Contracts/UpdateLocationRequest.cs
- services/telemetry/Application/Contracts/CheckInRequest.cs
- services/telemetry/Application/Contracts/CheckOutRequest.cs
- services/telemetry/Application/Contracts/JourneySessionResponse.cs
- services/telemetry/Application/Contracts/JourneyCheckpointResponse.cs

### 5.2 Program.cs endpointleri

Dosya: services/telemetry/Program.cs

Endpointler:

1. GET /
2. POST /journeys/start
3. POST /journeys/{id}/locations
4. POST /journeys/{id}/checkin
5. POST /journeys/{id}/checkout
6. POST /journeys/{id}/complete
7. GET /journeys/{id}
8. GET /journeys/active/{vehicleId}

Servis kayıtları:

- AddSharedInfrastructure<TelemetryDbContext>
- ITenantProvider -> HttpHeaderTenantProvider
- IJourneyRepository -> JourneyRepository

---

## 6) Doğrulama

```powershell
dotnet build services/telemetry/Journey.Telemetry.Api.csproj
dotnet build MunicipalityTicketing.slnx
dotnet test MunicipalityTicketing.slnx
```

Beklenen sonuç: tüm build/test adımları başarılı.

---

## 7) Tamamlanma Kontrol Listesi

- [x] JourneySession ve JourneyCheckpoint domain modeli eklendi.
- [x] TelemetryDbContext eklendi.
- [x] IJourneyRepository ve JourneyRepository eklendi.
- [x] Multi-tenant provider eklendi.
- [x] Telemetry API için X-Tenant-Id zorunluluğu middleware ile eklendi.
- [x] Telemetry endpointleri eklendi.
- [x] Build/test doğrulaması tamamlandı.

---

## 8) Step 07 İçin İhtiyaç Analizi (Event Processor)

### 8.1 İhtiyaçlar

1. Event consumer altyapısı
2. Outbox veya durable queue stratejisi
3. Retry ve dead-letter mekanizması
4. Telemetry/Wallet/Identity olaylarının işlenme sözleşmeleri

### 8.2 Teknik Backlog

1. Worker içinde message handler pipeline
2. Tenant context propagation
3. Idempotent event processing
4. İşlem hatası için poison event politikası

### 8.3 Riskler ve Aksiyonlar

1. Risk: Event tekrar işlenmesi
Aksiyon: Event idempotency tablosu ekle.

2. Risk: Mesaj gecikmesi ve sıralama problemi
Aksiyon: Partition key ve ordering stratejisi belirle.

3. Risk: Tenant context kaybı
Aksiyon: Her event payload'ına TenantId zorunlu alanı koy.

---

**Durum**: ✅ Tamamlandı (Doğrulandı)
**Son Güncelleme**: 11.06.2026
**Yazar**: Özgür Can TURNA
