# Step 01: Initial Setup - Proje Kurulumu ve Temizlik

## 🎯 Amaç
Template projelerden gelen gereksiz kodları temizleyip, solution'ı build edilebilir hale getirmek.

---

## ⬜ Yapılacak İşlemler

### 1. Gereksiz Dosyaların Silinmesi
```bash
# Template Class1.cs dosyaları silindi
rm core/SharedKernel.Domain/Class1.cs
rm core/SharedKernel.Infrastructure/Class1.cs
```

### 2. Program.cs Dosyalarının Temizlenmesi
Tüm API projelerinde template WeatherForecast kodları kaldırıldı ve minimal API yapısına geçildi:

#### Tenant.Identity.Api
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Tenant Identity API is running");

app.Run();
```

#### Ticketing.Wallet.Api
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Ticketing Wallet API is running");

app.Run();
```

#### Journey.Telemetry.Api
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Journey Telemetry API is running");

app.Run();
```

#### ApiGateway.Yarp
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "API Gateway (YARP) is running");

app.Run();
```

#### Journey.EventProcessor.Worker
Worker template zaten temiz idi, değişiklik yapılmadı.

### 3. Dizin Yapısının Oluşturulması
```bash
# Simulation clients için dizin
mkdir -p tools/simulator
```

---

## 📋 Mevcut Proje Durumu

### Solution Dosyası
- **Dosya**: `MunicipalityTicketing.slnx`
- **Projeler**: 8 adet
  - SharedKernel.Domain (core/SharedKernel/Domain/)
  - SharedKernel.Infrastructure (core/SharedKernel/Infrastructure/)
  - Tenant.Identity.Api (services/identity/)
  - Ticketing.Wallet.Api (services/wallet/)
  - Journey.Telemetry.Api (services/telemetry/)
  - Journey.EventProcessor.Worker (workers/event-processor/)
  - ApiGateway.Yarp (gateway/ApiGateway.Yarp.csproj)
  - MunicipalityTicketing.Simulator (tools/simulator/)

### Target Framework
- Tüm projeler: **.NET 10.0**

### Proje Yapısı
```
MunicipalityTicketing/
├── core/
│   └── SharedKernel/
│       ├── Domain/
│       │   ├── Common/
│       │   │   └── ValueObject.cs
│       │   ├── Entities/
│       │   │   ├── Entity.cs
│       │   │   └── AggregateRoot.cs
│       │   ├── Events/
│       │   │   └── DomainEvent.cs
│       │   └── Repositories/
│       │       └── IRepository.cs
│       └── Infrastructure/
│           ├── Persistence/
│           │   └── AppDbContext.cs
│           └── Repositories/
│               └── Repository.cs
├── services/
│   ├── identity/              ⬜ Yapılacak
│   ├── wallet/                ⬜ Yapılacak
│   └── telemetry/             ⬜ Yapılacak
├── workers/
│   └── event-processor/       ⬜ Yapılacak
├── gateway/
│   ├── ApiGateway.Yarp.csproj  ⬜ Yapılacak
│   └── Program.cs              ⬜ Yapılacak
├── tools/
│   └── simulator/             📁 Yeni oluşturuldu
├── docs/
│   ├── skills.md             ⬜ Yapılacak
│   ├── Step-00-Planlama.md   ⬜ Yapılacak
│   ├── Step-01-InitialSetup.md ⬜ Bu dosya
│   └── Step-02-SharedKernel.md ⬜ Yapılacak
└── README.md                 🔄 Güncellenecek
```

---

## 🔧 Sonraki Adımlar

### Hemen Yapılması Gerekenler
1. **Solution Build Testi**: .NET SDK kurulu ise `dotnet build` komutu ile test
2. **Git Commit**: İlk temiz durumu commit et
3. **README.md Güncelleme**: Proje açıklamasını ekle

### Step 02'de Yapılacaklar
⬜ **SharedKernel.Domain**:
   - Entity abstract class (Id, CreatedAt, UpdatedAt, DomainEvents)
   - AggregateRoot base class (Version, RegisterCreated, RegisterDeleted)
   - ValueObject base class (equality components)
   - IRepository interface (CRUD operations)
   - IDomainEvent interface ve implementasyonları (EntityCreatedEvent, EntityDeletedEvent, EntityUpdatedEvent)

⬜ **SharedKernel.Infrastructure**:
   - AppDbContext base class (audit fields auto-update)
   - Repository<TEntity> base implementation

⬜ **Proje Referansları**:
   - SharedKernel.Infrastructure -> SharedKernel.Domain referansı eklendi
   - Solution dosyası güncellendi

---

## 📝 Notlar

- Template kodlar tamamen temizlendi
- Minimal API yapısı hazır
- Solution yapısı senaryoya uygun
- Simulation dizini oluşturuldu
- Dokümantasyon altyapısı hazır
- **Step 02 yapılacak**: Shared Kernel implementation

---

**Durum**: ⬜ Yapılacak  
**Son Güncelleme**: 11.06.2026  
**Yazar**: Özgür Can TURNA
