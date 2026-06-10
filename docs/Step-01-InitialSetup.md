# Step 01: Initial Setup - Proje Kurulumu ve Temizlik

## 🎯 Amaç
Template projelerden gelen gereksiz kodları temizleyip, solution'ı build edilebilir hale getirmek.

---

## ✅ Tamamlanan İşlemler

### 1. Gereksiz Dosyaların Silinmesi
```bash
# Template Class1.cs dosyaları silindi
rm src/SharedKernel.Domain/Class1.cs
rm src/SharedKernel.Infrastructure/Class1.cs
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
mkdir -p src/Clients/Simulation

# Test projeleri için dizinler
mkdir -p tests/MunicipalityTicketing.UnitTests
mkdir -p tests/MunicipalityTicketing.IntegrationTests
```

---

## 📋 Mevcut Proje Durumu

### Solution Dosyası
- **Dosya**: `MunicipalityTicketing.slnx`
- **Projeler**: 7 adet
  - SharedKernel.Domain (src/BuildingBlocks/SharedKernel/)
  - SharedKernel.Infrastructure (src/BuildingBlocks/SharedKernel/)
  - Tenant.Identity.Api
  - Ticketing.Wallet.Api
  - Journey.Telemetry.Api
  - Journey.EventProcessor.Worker
  - ApiGateway.Yarp

### Target Framework
- Tüm projeler: **.NET 10.0**

### Proje Yapısı
```
MunicipalityTicketing/
├── src/
│   ├── BuildingBlocks/
│   │   └── SharedKernel/
│   │       ├── SharedKernel.Domain/
│   │       │   ├── Entities/
│   │       │   │   ├── Entity.cs
│   │       │   │   └── AggregateRoot.cs
│   │       │   ├── Common/
│   │       │   │   └── ValueObject.cs
│   │       │   ├── Events/
│   │       │   │   └── DomainEvent.cs
│   │       │   ├── Repositories/
│   │       │   │   └── IRepository.cs
│   │       │   └── SharedKernel.Domain.csproj
│   │       └── SharedKernel.Infrastructure/
│   │           ├── Persistence/
│   │           │   └── AppDbContext.cs
│   │           ├── Repositories/
│   │           │   └── Repository.cs
│   │           └── SharedKernel.Infrastructure.csproj
│   ├── Tenants.Identity.Api/ ✅ Temiz
│   ├── Ticketing.Wallet.Api/ ✅ Temiz
│   ├── Journey.Telemetry.Api/✅ Temiz
│   ├── Journey.EventProcessor.Worker/ ✅ Temiz
│   ├── ApiGateway.Yarp/      ✅ Temiz
│   └── Clients/
│       └── Simulation/       📁 Yeni oluşturuldu
├── tests/
│   ├── MunicipalityTicketing.UnitTests/ 📁 Yeni oluşturuldu
│   └── MunicipalityTicketing.IntegrationTests/ 📁 Yeni oluşturuldu
├── docs/
│   ├── skills.md             ✅ Oluşturuldu
│   ├── Step-00-Planlama.md   ✅ Oluşturuldu
│   ├── Step-01-InitialSetup.md ✅ Bu dosya
│   └── Step-02-SharedKernel.md ✅ Oluşturuldu
└── README.md                 🔄 Güncellenecek
```

---

## 🔧 Sonraki Adımlar

### Hemen Yapılması Gerekenler
1. **Solution Build Testi**: .NET SDK kurulu ise `dotnet build` komutu ile test
2. **Git Commit**: İlk temiz durumu commit et
3. **README.md Güncelleme**: Proje açıklamasını ekle

### Step 02'de Yapılacaklar (TAMAMLANDI)
✅ **SharedKernel.Domain**:
   - Entity abstract class (Id, CreatedAt, UpdatedAt, DomainEvents)
   - AggregateRoot base class (Version, RegisterCreated, RegisterDeleted)
   - ValueObject base class (equality components)
   - IRepository interface (CRUD operations)
   - IDomainEvent interface ve implementasyonları (EntityCreatedEvent, EntityDeletedEvent, EntityUpdatedEvent)

✅ **SharedKernel.Infrastructure**:
   - AppDbContext base class (audit fields auto-update)
   - Repository<TEntity> base implementation

✅ **Proje Referansları**:
   - SharedKernel.Infrastructure -> SharedKernel.Domain referansı eklendi
   - Solution dosyası güncellendi

---

## 📝 Notlar

- Template kodlar tamamen temizlendi
- Minimal API yapısı hazır
- Solution yapısı senaryoya uygun
- Test ve simulation dizinleri oluşturuldu
- Dokümantasyon altyapısı hazır
- **Step 02 tamamlandı**: Shared Kernel implementation hazır

---

**Durum**: ✅ Tamamlandı  
**Son Güncelleme**: 2024  
**Yazar**: Özgür Can TURNA
