# Municipality Ticketing System

## 🚌 Proje Açıklaması

Belediyeler için geliştirilen yüksek ölçeklenebilir, multi-tenant otobüs biletleme sistemi. Bu proje, 15 kişilik hayali bir yazılım ekibinin proje lideri tarafından, DDD (Domain-Driven Design) prensipleri ve mikroservis mimarisi kullanılarak geliştirilmektedir.

### Senaryo

- **İlk Müşteriler**: Bursa, Eskişehir, Van ve Mersin
- **Performans Hedefi** (her belediye için):
  - 10 milyon+ günlük aktif bilet kullanımı
  - 100.000+ günlük bilet/kredi satın alma
  - 10.000+ günlük otobüs yolculuğu takibi
- **Kritik Gereksinimler**: Multi-tenancy, fault isolation, zero-downtime deployment

---

## 🏗️ Current MVP Architecture

```mermaid
graph TB
    subgraph "Client Layer"
        Web[Web Dashboard<br/>MVP Placeholder]
    end

    subgraph "API Gateway"
        YARP[YARP API Gateway<br/>JWT auth, rate limit]
    end

    subgraph "Microservices"
        Identity[Tenant.Identity.Api<br/>User Management]
        Wallet[Ticketing.Wallet.Api<br/>Balance & Payments]
        Telemetry[Journey.Telemetry.Api<br/>Location Tracking]
    end

    subgraph "Infrastructure"
        Redis[Redis Cache]
        OTel[OpenTelemetry Collector]
    end

    subgraph "Data Store"
        DB1[(MunicipalityTicketing.Default<br/>Single DB)]
    end

    subgraph "Messaging (Not Used)"
        RabbitMQ[RabbitMQ Container<br/>Idle]
    end

    Web --> YARP
    YARP --> Identity
    YARP --> Wallet
    YARP --> Telemetry
    Identity --> Redis
    Wallet --> Redis
    Telemetry --> Redis
    Identity --> DB1
    Wallet --> DB1
    Telemetry --> DB1
    RabbitMQ -.-> EventProc[Event Processor<br/>In-Memory Queue]
```

> **Note**: The Event Processor worker runs but uses `InMemoryEventQueue` only. RabbitMQ is defined in compose but not connected to services.

---

## 🏗️ Target Architecture

```mermaid
graph TB
    subgraph "Client Layer"
        Mobile[Mobile App]
        Web[Web Dashboard<br/>React 19 + RBAC]
        Simulator[Simulation Client]
    end

    subgraph "API Gateway"
        YARP[YARP API Gateway<br/>CORS, rate limit, routing]
    end

    subgraph "Microservices"
        Identity[Tenant.Identity.Api<br/>User Management]
        Wallet[Ticketing.Wallet.Api<br/>Balance & Payments]
        Telemetry[Journey.Telemetry.Api<br/>Location Tracking]
        Ticketing[Ticketing.Service<br/>Ticket Lifecycle]
    end

    subgraph "Infrastructure"
        Redis[Redis Cache]
        MessageBus[Brighter & Darker<br/>RabbitMQ transport]
        OTel[OpenTelemetry<br/>+ Jaeger/Prometheus/Grafana]
    end

    subgraph "Data Stores (Per Tenant)"
        DB1[(Tenant A DB)]
        DB2[(Tenant B DB)]
        DB3[(Tenant C DB)]
        DB4[(Tenant D DB)]
    end

    Mobile --> YARP
    Web --> YARP
    Simulator --> YARP
    YARP --> Identity
    YARP --> Wallet
    YARP --> Telemetry
    YARP --> Ticketing
    Identity --> Redis
    Wallet --> Redis
    Telemetry --> Redis
    Identity --> MessageBus
    Wallet --> MessageBus
    Telemetry --> MessageBus
    Ticketing --> MessageBus
    MessageBus --> EventProc[Journey.EventProcessor.Worker]
    Identity --> DB1 & DB2 & DB3 & DB4
    Wallet --> DB1 & DB2 & DB3 & DB4
    Telemetry --> DB1 & DB2 & DB3 & DB4
    Ticketing --> DB1 & DB2 & DB3 & DB4
```

> **Target**: Database-per-Tenant, Ticketing Service, real RabbitMQ transport, full observability stack.

---

## 🛠️ Teknolojiler

| Kategori | Araç | Lisans |
| ---------- | ------ | -------- |
| Framework | .NET 10 | MIT |
| Message Bus | Brighter & Darker | Apache 2.0 |
| ORM | Entity Framework Core 10 | MIT |
| Cache | StackExchange.Redis | MIT |
| Validation | FluentValidation | Apache 2.0 |
| Logging | Serilog | Apache 2.0 |
| Tracing | OpenTelemetry | Apache 2.0 |
| API Gateway | YARP | MIT |
| Testing | xUnit, Moq, Shouldly | Various |
| Container | Docker | Apache 2.0 |

---

## 📌 Kapsam Gerceklik Durumu (MVP vs Hedef)

Bu repo Step 01-11 icin teknik bir MVP akisi sunar. Ancak Step dokumanlarinda tanimlanan tum business/hedef gereksinimleri henuz tam olarak production seviyesinde tamamlanmamistir.

### Şu an kodda bulunanlar

- Shared Kernel + EF Core + Redis tabanli altyapi
- Identity/Wallet/Telemetry minimal endpointleri ve temel domain kurallari
- Identity bootstrap + JWT login + BCrypt password hashing + rol bazli endpoint korumalari + Bursa/Eskişehir/Van/Mersin demo tenant/user seed
- Gateway katmaninda merkezi JWT dogrulamasi ve tenant-claim kontrolu
- Event processor **MVP sürümünde In-Memory EventQueue kullanıyor**; RabbitMQ transport hedef ama compose'ta hazir (henuz consumer/producer entegrasyonu yok)
- YARP gateway routing + tenant header zorunlulugu + basic rate limit
- Unit ve integration testlerin temel kapsami
- Docker compose ile lokal ortam kurulumu

### Henuz eksik veya kismi kalan kritik hedefler

- JWT authentication ve RBAC (FR-001) icin Bursa, Eskişehir, Van ve Mersin demo seed, temel roller ve ornek kullanicilar eklendi; admin/provisioning arayuzleri ve production hardening devam ediyor
- Bilet yonetimi (FR-003: QR, iade/iptal, ticket lifecycle) ayri bir servis olarak uygulanmadi
- Brighter & Darker domain event katmani kullaniliyor; RabbitMQ transport compose ve appsettings tarafinda tanimli ama **producer/consumer entegrasyonu MVP sürümünde yok**
- Serilog/OpenTelemetry/Prometheus/Grafana gozleme zincirinden sadece OpenTelemetry temel trace aktif; Serilog logger, Prometheus/Grafana/Jaeger hedef ama kod/pipeline seviyesinde tam uygulanmadi
- CI/CD, zero-downtime deployment ve feature flag gibi operasyonel hedefler dokumante ama kod/pipeline seviyesinde tam uygulanmadi
- Database-per-tenant hedefi dokumande var ama Docker ortaminda henuz uygulanmadi (tek `MunicipalityTicketing.Default` DB kullaniliyor)

Not: Step durumlarinin "tamamlandi" olmasi, adim bazli MVP tesliminin tamamlandigini ifade eder; tum BR/FR/NFR hedeflerinin production-hardening seviyesinde bittiği anlamina gelmez.

---

## 📁 Proje Yapısı

```text
MunicipalityTicketing/
├── aspire/                          # .NET Aspire orchestration
│   ├── AppHost/                     # Distributed application orchestrator
│   └── ServiceDefaults/             # Shared service configuration
├── core/                          # Shared Kernel
│   └── SharedKernel/
│       ├── Domain/                # Base classes, interfaces
│       │   ├── Common/
│       │   ├── Entities/
│       │   ├── Events/
│       │   └── Repositories/
│       └── Infrastructure/        # EF Core, Redis implementations
│           ├── Persistence/
│           └── Repositories/
├── services/                      # Microservices
│   ├── identity/                  # Tenant.Identity.Api - Kullanıcı yönetimi
│   ├── wallet/                    # Ticketing.Wallet.Api - Cüzdan ve ödeme işlemleri
│   └── telemetry/                 # Journey.Telemetry.Api - Yolculuk takibi
├── workers/                       # Background Workers
│   └── event-processor/           # Journey.EventProcessor.Worker - Asenkron event processing
├── gateway/                       # API Gateway
│   └── ApiGateway.Yarp/           # YARP reverse proxy
├── tools/                         # Development Tools
│   └── simulator/                 # Load testing clients
├── tests/
│   ├── MunicipalityTicketing.UnitTests
│   └── MunicipalityTicketing.IntegrationTests
├── docs/
│   ├── skills.md                  # Developer guidelines
│   ├── Step-00-Planlama.md        # Requirements & architecture
│   ├── Step-01-InitialSetup.md    # Initial setup steps
│   └── Step-XX-*.md               # Diğer adımlar
├── docker-compose.yml
├── MunicipalityTicketing.slnx
└── README.md
```

---

## 📋 Dokümantasyon

| Dosya | Açıklama |
| ------- | ---------- |
| [docs/skills.md](docs/skills.md) | Developer becerileri ve kodlama standartları |
| [docs/Step-00-Planlama.md](docs/Step-00-Planlama.md) | İş gereksinimleri, mimari, tool set |
| [docs/Step-01-InitialSetup.md](docs/Step-01-InitialSetup.md) | Proje kurulumu ve temizlik adımları |
| [docs/Step-02-SharedKernel.md](docs/Step-02-SharedKernel.md) | Shared Kernel domain ve infrastructure adımları |
| [docs/Step-03-Infrastructure.md](docs/Step-03-Infrastructure.md) | EF Core ve Redis altyapı hazırlığı |
| [docs/Step-04-IdentityService.md](docs/Step-04-IdentityService.md) | Identity domain, persistence ve API adımları |
| [docs/Step-05-WalletService.md](docs/Step-05-WalletService.md) | Wallet domain, persistence ve API adımları |
| [docs/Step-06-TelemetryService.md](docs/Step-06-TelemetryService.md) | Telemetry domain, persistence ve API adımları |
| [docs/Step-07-EventProcessor.md](docs/Step-07-EventProcessor.md) | Event consumer, retry ve dead-letter adımları |
| [docs/Step-08-ApiGateway.md](docs/Step-08-ApiGateway.md) | Gateway routing, header ve rate-limit adımları |
| [docs/Step-09-Testing.md](docs/Step-09-Testing.md) | Unit ve integration test adımları |
| [docs/Step-10-SimulationClients.md](docs/Step-10-SimulationClients.md) | Simulator ile gateway uzerinden yuk testi adımları |
| [docs/Step-11-DockerDeployment.md](docs/Step-11-DockerDeployment.md) | Docker compose kurulumu ve event bus deployment adımları |
| [docs/Step-12-AuthAndRbac.md](docs/Step-12-AuthAndRbac.md) | JWT authentication, rol tabanli yetkilendirme ve tenant-claim eslestirme |
| [docs/Step-13-WebDashboard.md](docs/Step-13-WebDashboard.md) | React 19 web yönetim paneli, mevcut MVP ve roadmap |
| [clients/webDashboard/README.md](clients/webDashboard/README.md) | Web Dashboard kurulum ve geliştirme komutları |

---

## 🚀 Başlangıç

### Gereksinimmler

- .NET 10 SDK
- Docker & Docker Compose
- Git

### Aspire Orchestration (Önerilen)

```bash
# Clone repository
git clone <repo-url>
cd MunicipalityTicketing

# Run with .NET Aspire (automatic container orchestration)
dotnet run --project aspire/AppHost/AppHost.csproj
```

### Docker Compose (Alternatif)

```bash
# Build solution
dotnet build MunicipalityTicketing.slnx

# Run with Docker Compose
docker compose up -d --build
```

Aspire, servis bağımlılıklarını, ortam değişkenlerini ve container'ları otomatik olarak yönetir. Dashboard üzerinden tracing, metrics ve logs izlenebilir.

---

## 🧪 Test Senaryoları

Proje tamamlandığında simulation client'ları ile aşağıdaki senaryolar test edilecek:

1. **Load Testing**: 10M+ daily transactions
2. **Failure Scenarios**: Database failures, message bus downtime
3. **Multi-Tenant Isolation**: Cross-tenant data access prevention
4. **Zero-Downtime Updates**: Rolling deployment validation

---

## 📝 Geliştirme Adımları

| Step | Konu | Durum |
| ------ | ------ | ------- |
| 00 | Planlama ve Gereksinimler | ✅ Tamamlandı |
| 01 | Initial Setup - Template Temizliği | ✅ MVP Tamamlandı |
| 02 | Shared Kernel - Domain Base Classes | ✅ MVP Tamamlandı |
| 03 | Infrastructure - EF Core & Redis | ✅ MVP Tamamlandı |
| 04 | Identity Service | ✅ MVP Tamamlandı |
| 05 | Wallet Service | ✅ MVP Tamamlandı |
| 06 | Telemetry Service | ✅ MVP Tamamlandı |
| 07 | Event Processor | ✅ MVP Tamamlandı |
| 08 | API Gateway | ✅ MVP Tamamlandı |
| 09 | Testing | ✅ MVP Tamamlandı |
| 10 | Simulation Clients | ✅ MVP Tamamlandı |
| 11 | Docker & Deployment | ✅ MVP Tamamlandı |
| 12 | JWT Authentication & RBAC | ✅ MVP Tamamlandı |
| 13 | Web Dashboard | 🟡 MVP Tamamlandı |

---

## 👨‍💻 Proje Lideri

**Özgür Can TURNA**  
*Proje Lideri ve Tek Geliştirici (Simülasyon)*

Bu proje, gerçek bir 15 kişilik ekip çalışmasını simüle etmek amacıyla DDD ve mikroservis best practice'lerini uygulamak için oluşturulmuştur.

---

## 📄 Lisans

MIT License - Detaylar için LICENSE dosyasına bakınız.
