# Step 08: API Gateway - YARP Routing ve Edge Politikaları

## 🎯 Amaç

Bu adımda API Gateway, Identity/Wallet/Telemetry servislerine merkezi giriş noktası olacak şekilde YARP ile yapılandırılır.

Bu adım sonunda:

- Gateway route ve cluster yapılandırması hazır olur.
- Tenant header zorunluluğu uygulanır.
- JWT bearer token gateway katmanında da doğrulanır.
- Correlation-id propagation yapılır.
- Basit tenant/IP tabanlı rate limiting devreye alınır.

---

## ✅ Önkoşullar

- Step 00, 01, 02, 03, 04, 05, 06, 07 tamamlanmış olmalı.

---

## 1) Gateway Paketleri

Dosya: gateway/ApiGateway.Yarp.csproj

Eklenen paket:

- Yarp.ReverseProxy

Bu paket route + cluster bazlı reverse proxy kabiliyetini sağlar.

---

## 2) Gateway Program Pipeline

Dosya: gateway/Program.cs

Eklenen adımlar:

1. AddReverseProxy + LoadFromConfig
2. Gateway-level JWT bearer authentication
3. Global rate limiter
4. Correlation-id middleware
5. X-Tenant-Id zorunluluğu middleware (api path'lerinde)
6. Token tenant claim ile header eşleştirmesi
7. Health endpoint
8. MapReverseProxy

### 2.1 Correlation-id davranışı

- İstekte X-Correlation-Id yoksa gateway üretir.
- Üretilen değer hem upstream request'e hem response header'a yazılır.

### 2.2 Tenant header davranışı

- /api ile başlayan tüm isteklerde X-Tenant-Id zorunludur.
- Header yoksa 400 BadRequest döner.
- /api/identity/auth/bootstrap istisna olarak tenant header olmadan çağrılabilir.

### 2.3 Gateway JWT davranışı

- /api/identity/auth/bootstrap ve /api/identity/auth/login dışındaki /api isteklerinde bearer token zorunludur.
- Token gateway üzerinde doğrulanır; geçersiz veya eksik token 401 döner.
- Token içindeki tenant_id claim ile X-Tenant-Id eşleşmezse 403 döner.

### 2.4 Rate limit davranışı

- Tenant header varsa tenant bazlı partition
- Header yoksa IP bazlı partition
- 60 istek / 60 saniye limit

---

## 3) YARP Route ve Cluster Konfigürasyonu

Dosya: gateway/appsettings.json

Tanımlanan route'lar:

1. /api/identity/{**catch-all} -> identity-cluster
2. /api/wallet/{**catch-all} -> wallet-cluster
3. /api/telemetry/{**catch-all} -> telemetry-cluster

Tanımlanan destination adresleri:

- Identity: <http://localhost:5115/>
- Wallet: <http://localhost:5136/>
- Telemetry: <http://localhost:5169/>

Transform'lar:

- PathRemovePrefix
- X-Forwarded-By header ekleme

---

## 4) Doğrulama

```powershell
dotnet build gateway/ApiGateway.Yarp.csproj
dotnet build MunicipalityTicketing.slnx
dotnet test MunicipalityTicketing.slnx
```

Gateway smoke test örnekleri:

```powershell
# gateway health
Invoke-RestMethod -Uri http://localhost:5197/health

# tenant header olmadan -> 400
Invoke-WebRequest -Uri http://localhost:5197/api/identity/tenants -Method Get

# tenant header ile routing
Invoke-RestMethod -Uri http://localhost:5197/api/identity/tenants/<guid> -Headers @{"X-Tenant-Id"="bursa"}
```

---

## 5) Tamamlanma Kontrol Listesi

- [x] YARP paketi eklendi.
- [x] Reverse proxy config appsettings'e eklendi.
- [x] Gateway route ve cluster tanımları eklendi.
- [x] Correlation-id middleware eklendi.
- [x] Tenant header zorunluluğu eklendi.
- [x] Rate limit eklendi.
- [x] Build/test doğrulandı.

---

## 6) Step 09 İçin İhtiyaç Analizi (Testing)

### 6.1 Test İhtiyaçları

1. Unit test: domain kuralları (wallet, telemetry, identity)
2. Integration test: DbContext + repository akışları
3. Gateway integration test: route + header + rate-limit davranışları
4. Event processor test: retry + idempotency + dead-letter

### 6.2 Teknik Backlog

1. WebApplicationFactory tabanlı API test altyapısı
2. Testcontainers ile SQL/Redis integration test
3. Deterministic seed verisi ve fixture yönetimi

### 6.3 Riskler ve Aksiyonlar

1. Risk: Servisler arası kontrat kırılması
Aksiyon: contract test (consumer/provider) ekle.

2. Risk: CI süresinin çok uzaması
Aksiyon: hızlı unit test ve yavaş integration test ayrımı yap.

3. Risk: Tenant izolasyon testlerinin atlanması
Aksiyon: her servis için tenant negatif test senaryoları zorunlu olsun.

---

**Durum**: ✅ Tamamlandı (Doğrulandı)
**Son Güncelleme**: 11.06.2026
**Yazar**: Özgür Can TURNA