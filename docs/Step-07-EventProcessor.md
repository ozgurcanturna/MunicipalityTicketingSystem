# Step 07: Event Processor - Event Consumer ve Retry Pipeline

## 🎯 Amaç
Bu adımda Event Processor worker, gelen integration event'leri idempotent ve retry destekli şekilde işleyecek temel mimariye taşınır.

Bu adım sonunda:
- In-memory event queue altyapısı hazır olur.
- Event handler çözümleme (resolver) mekanizması kurulur.
- Retry + dead-letter akışı çalışır.
- İşlenmiş event idempotency takibi yapılır.

---

## ✅ Önkoşullar
- Step 00, 01, 02, 03, 04, 05, 06 tamamlanmış olmalı.

---

## 1) Program.cs Servis Kayıtları

Dosya: workers/event-processor/Program.cs

Eklenenler:
1. EventProcessor options binding
2. Queue, processed store, dead-letter store kayıtları
3. Handler resolver kaydı
4. Event handler kayıtları

Kritik kayıtlar:
- IEventQueue -> InMemoryEventQueue
- IProcessedEventStore -> InMemoryProcessedEventStore
- IDeadLetterStore -> InMemoryDeadLetterStore
- IEventHandlerResolver -> EventHandlerResolver

---

## 2) Event Sözleşmeleri ve Queue

### 2.1 IntegrationEvent modeli
Dosya: workers/event-processor/Events/IntegrationEvent.cs

Alanlar:
- EventId
- TenantId
- EventType
- Payload
- OccurredAt
- CorrelationId

### 2.2 IEventQueue
Dosya: workers/event-processor/Events/IEventQueue.cs

Metotlar:
- EnqueueAsync
- DequeueAsync

### 2.3 InMemoryEventQueue
Dosya: workers/event-processor/Events/InMemoryEventQueue.cs

Channel tabanlı kuyruk implementasyonu.

---

## 3) Handler Pipeline

### 3.1 Handler sözleşmesi
Dosya: workers/event-processor/Processing/IIntegrationEventHandler.cs

### 3.2 Handler resolver
Dosya: workers/event-processor/Processing/IEventHandlerResolver.cs
Dosya: workers/event-processor/Processing/EventHandlerResolver.cs

Resolver, EventType -> Handler map'i oluşturur.

### 3.3 Örnek handlerlar
Dosyalar:
- workers/event-processor/Processing/IdentityTenantCreatedEventHandler.cs
- workers/event-processor/Processing/WalletDebitedEventHandler.cs
- workers/event-processor/Processing/JourneyCompletedEventHandler.cs

Not: Bu adımda handlerlar log tabanlıdır; gerçek business işlemleri Step 08+ içinde genişletilir.

---

## 4) Idempotency ve Dead-Letter

### 4.1 Processed event store
Dosya: workers/event-processor/Storage/IProcessedEventStore.cs
Dosya: workers/event-processor/Storage/InMemoryProcessedEventStore.cs

Amaç:
- Aynı EventId ikinci kez geldiyse atlamak

### 4.2 Dead-letter store
Dosya: workers/event-processor/Storage/IDeadLetterStore.cs
Dosya: workers/event-processor/Storage/InMemoryDeadLetterStore.cs

Amaç:
- Handler bulunamayan veya retry limiti aşan event'i dead-letter kuyruğuna taşımak

---

## 5) Worker Akışı

Dosya: workers/event-processor/Worker.cs

Akış:
1. Opsiyonel demo event seed
2. Queue'dan event çekme
3. İdempotency kontrolü
4. Handler resolve etme
5. Retry ile işleme
6. Başarısız event'i dead-letter store'a yazma

Retry davranışı:
- MaxRetryCount kadar dener
- BaseRetryDelayMs * attempt gecikmesi uygular

---

## 6) Konfigürasyon

### 6.1 EventProcessorOptions
Dosya: workers/event-processor/Configuration/EventProcessorOptions.cs

Alanlar:
- MaxRetryCount
- BaseRetryDelayMs
- SeedDemoEvents

### 6.2 appsettings
Dosya: workers/event-processor/appsettings.json

Eklenen bölüm:
```json
"EventProcessor": {
  "MaxRetryCount": 3,
  "BaseRetryDelayMs": 250,
  "SeedDemoEvents": true
}
```

---

## 7) Doğrulama

```powershell
dotnet build workers/event-processor/Journey.EventProcessor.Worker.csproj
dotnet build MunicipalityTicketing.slnx
dotnet test MunicipalityTicketing.slnx
```

Beklenen sonuç: build/test başarılı.

---

## 8) Tamamlanma Kontrol Listesi

- [x] Event queue altyapısı eklendi.
- [x] Handler resolver ve handlerlar eklendi.
- [x] Retry pipeline eklendi.
- [x] Idempotency store eklendi.
- [x] Dead-letter store eklendi.
- [x] EventProcessor options eklendi.
- [x] Worker akışı bu parçalarla entegre edildi.

---

## 9) Step 08 İçin İhtiyaç Analizi (API Gateway)

### 9.1 İhtiyaçlar
1. Gateway route tanımları (identity/wallet/telemetry)
2. Tenant ve auth header forwarding
3. Basit rate limiting stratejisi
4. Correlation-id propagation

### 9.2 Teknik Backlog
1. YARP route + cluster config
2. Health endpoint routing
3. Request/response logging middleware
4. Gateway seviyesinde hata sözleşmesi

### 9.3 Riskler ve Aksiyonlar
1. Risk: Yanlış route konfigürasyonu ile servis erişilemezliği
Aksiyon: route integration testleri eklenmeli.

2. Risk: Tenant header kaybı
Aksiyon: gateway middleware ile header zorunluluğu koy.

3. Risk: Tek gateway instance darboğazı
Aksiyon: yatay ölçek + shared config stratejisi planla.

---

**Durum**: ✅ Tamamlandı (Doğrulandı)
**Son Güncelleme**: 11.06.2026
**Yazar**: Özgür Can TURNA