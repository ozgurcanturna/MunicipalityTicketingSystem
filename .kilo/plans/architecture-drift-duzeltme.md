# Architecture Drift Düzeltme Planı

## Amaç

Planlanan hedef mimari ile mevcut MVP kodu arasındaki drift’i gidermek. İlk aşamada dokümantasyonu mevcut gerçeklikle uyumlu hale getirmek; ikinci aşamada hedef mimariye geçiş için uygulanabilir roadmap ve doğrulama adımları tanımlamak.

## Kapsam

Bu plan iki parçalıdır:

1. **Doküman düzeltme**: README, Step dokümanları ve geliştirme kurallarını mevcut kodla tutarlı hale getirmek.
2. **Uygulama roadmap’i**: Per-tenant DB, Ticketing Service, gerçek event bus, observability ve production hardening için task list’i çıkarmak.

Kaynak kod değişiklikleri bu plan dosyasında görev olarak tanımlanır; bu oturumda uygulanmaz.

## Mevcut bulgular

### Doğru / kısmen doğru olanlar

- `.NET 10` proje yapısı ve `MunicipalityTicketing.slnx` mevcut.
- Identity, Wallet, Telemetry API’leri ve YARP gateway mevcut.
- JWT auth, BCrypt password hashing, temel RBAC ve demo tenant/user seed mevcut.
- Gateway’de `X-Tenant-Id` header kontrolü ve token claim eşleşmesi var.
- `TenantConnectionStringResolver` per-tenant connection string çözümlemesini destekliyor.
- Redis ve OpenTelemetry trace exporter kısmi olarak entegre.
- Docker Compose’da PostgreSQL, Redis, RabbitMQ ve OpenTelemetry Collector container’ları tanımlı.

### Stale / yanlış olanlar

- README’de event processor’ın Brighter & Darker + RabbitMQ transport ile çalıştığı yazıyor; mevcut worker `InMemoryEventQueue`, `InMemoryProcessedEventStore`, `InMemoryDeadLetterStore` kullanıyor.
- `docs/Step-11-DockerDeployment.md` içinde RabbitMQ transport Docker’da çalışır ifadesi, aynı dokümandaki in-memory worker açıklamasıyla çelişiyor.
- Database-per-tenant hedefi dokümanlarda ana karar; Docker Compose ise servisleri tek `MunicipalityTicketing.Default` veritabanına bağlar.
- Ticketing Service, QR, iade/iptal ve ticket lifecycle mevcut değil.
- CQRS hedef dokümanda var; kodda command/query handler ayrımı yok.
- Serilog, Prometheus, Grafana, Jaeger, FluentValidation, AutoMapper, Polly, CI/CD, zero-downtime deployment ve feature flag hedefleri dokümante ama uygulanmış değil.
- Web Dashboard MVP placeholder seviyesinde; gerçek API entegrasyonu, RBAC route koruması, httpOnly cookie token storage ve feature modülleri eksik.

## Planlanan işler

### 1. Doküman drift’ini düzelt

1. `README.md` içinde “Mimari” bölümünü iki alt başlığa ayır:
   - **Current MVP Architecture**
   - **Target Architecture**
2. `README.md` içinde şu ifadeleri düzelt:
   - Event processor’ın RabbitMQ/Brighter & Darker ile çalıştığı iddiası.
   - Serilog/OpenTelemetry/Prometheus/Grafana zincirinin tam entegre olduğu izlenimi.
   - Database-per-tenant’ın Docker ortamında aktif olduğu izlenimi.
3. `docs/Step-00-Planlama.md` başına şu notu ekle:
   - Bu doküman hedef mimariyi tanımlar; mevcut MVP ile birebir aynı değildir.
4. `docs/Step-11-DockerDeployment.md` içindeki çelişkili RabbitMQ satırını düzelt:
   - Docker Compose RabbitMQ container’ı başlatır.
   - Event Processor MVP’de RabbitMQ transport kullanmaz.
5. `docs/skills.md` içinde teknoloji/standartları iki kategoriye ayır:
   - Mevcut MVP’de kullanılanlar
   - Hedef/roadmap’te uygulanacaklar
6. Yeni bir kısa doküman ekle:
   - `docs/Architecture-Current-MVP.md`
   - Mevcut servisler, veri akışı, auth/tenant modeli, eksik hedef bileşenler ve bilinen riskler.

### 2. Hedef mimari roadmap’i oluştur

1. **Per-tenant DB geçişi**
   - Tenant registry / provisioning stratejisini tanımla.
   - Docker Compose’da demo tenant DB’lerini aktif et veya local için ayrı profile koy.
   - Seeder’ı tenant-specific DB’lere göre düzelt.
   - Migration ve database creation stratejisini netleştir.
2. **Ticketing Service ekleme**
   - Domain: `Ticket`, `TicketStatus`, QR payload, refund/cancel rules.
   - Application: create/validate/refund/cancel command ve query modelleri.
   - Infrastructure: EF Core DbContext, repository, outbox tablosu.
   - API: gateway route `/api/ticketing/*`.
   - Tests: domain invariant, API contract, tenant isolation.
3. **Event bus gerçekleme**
   - Brighter & Darker veya alternatif RabbitMQ transport kararını netleştir.
   - Publisher servislerde outbox pattern ekle.
   - Consumer’da persistent inbox/idempotency ve DLQ retry stratejisi ekle.
   - Schema versioning ve contract testleri ekle.
4. **CQRS ayrımı**
   - Command/query sınıflarını Application katmanına taşı.
   - Minimal API endpoint’lerini command/query handler’lara bağla.
   - Mevcut domain invariant’ları koru.
5. **Observability hardening**
   - Serilog structured logging ekle.
   - OpenTelemetry metrics pipeline ekle.
   - Prometheus ve Grafana container’larını compose’a ekle.
   - Jaeger/Tempo backend eksikliğini gider.
   - Trace correlation ve baggage propagasyonunu doğrula.
6. **Security hardening**
   - Refresh token flow.
   - JWT secret/key rotation stratejisi.
   - httpOnly cookie token storage için backend/frontend tasarımı.
   - Gateway CORS policy.
   - Global exception middleware ve RFC 7807 problem details.
   - FluentValidation request validation.
7. **Operasyonel hedefler**
   - CI/CD pipeline.
   - Rolling/blue-green deployment stratejisi.
   - Feature flags.
   - Health/readiness/liveness endpoint standardı.
   - Load test ve performans kabul kriterleri.

## Doğrulama planı

### Doküman doğrulama

- README’de “Current” ve “Target” ayrımı açık olmalı.
- Event bus, database-per-tenant ve observability bölümleri mevcut kodla çelişmemeli.
- `docs/Architecture-Current-MVP.md` şu bileşenleri listelemeli:
  - Identity
  - Wallet
  - Telemetry
  - Gateway
  - In-memory Event Processor
  - PostgreSQL default DB
  - Redis
  - RabbitMQ container ama kullanılmıyor
  - Web Dashboard MVP
- Her Step dokümanı kendi kapsamını “MVP tamamlandı” olarak açıkça belirtmeli.

### Kod roadmap doğrulama

- Per-tenant DB task’ları için tenant ID ile connection string eşleşmesi test edilmeli.
- Ticketing Service task’ları için QR/refund/cancel domain testleri tanımlanmalı.
- Event bus task’ları için RabbitMQ integration testleri tanımlanmalı.
- Observability task’ları için trace/metrics endpointleri doğrulanmalı.
- Gateway task’ları için auth, tenant header, CORS ve routing testleri tanımlanmalı.

## Riskler

- Dokümanları düzeltmeden kod değişikliklerine başlamak, mimari beklentiyi yanlış yönlendirir.
- Per-tenant DB ve RabbitMQ aynı anda eklenirse kapsam büyür; önce MVP dokümanını düzeltmek daha güvenli.
- Web Dashboard gerçek auth’a geçerken token storage ve CORS birlikte ele alınmalı.
- Event bus gerçekleme outbox/inbox olmadan güvenilir olmaz.

## Öncelik sırası

1. Doküman drift düzeltmesi
2. Current MVP dokümanı
3. Per-tenant DB stratejisi
4. Ticketing Service
5. Event bus gerçekleme
6. Observability ve security hardening
7. CI/CD ve deployment hardening

## Not

Bu plan bir implementation-capable agent için hazırlanmıştır. Kaynak kod değişiklikleri plan dosyasında task olarak tanımlanır; bu oturumda uygulanmaz.

## Uygulama Durumu

- [x] Doküman drift düzeltmesi (bu oturumda tamamlandı)
- [x] .NET Aspire AppHost ekleme (bu oturumda tamamlandı)
- [ ] Per-tenant DB stratejisi
- [ ] Ticketing Service ekleme
- [ ] Event bus gerçekleme
- [ ] CQRS ayrımı
- [ ] Observability ve security hardening
- [ ] CI/CD ve deployment hardening

## Aspire Entegrasyon Notları

- `aspire/AppHost/AppHost.csproj` - Distribüt uygulama orchestratörü
- `aspire/ServiceDefaults/ServiceDefaults.csproj` - Paylaşımlı service konfigürasyonu (health checks, OpenTelemetry, service discovery, resilience)
- Servisler: Identity, Wallet, Telemetry, Gateway, Event Processor
- Altyapı: PostgreSQL, Redis, RabbitMQ
