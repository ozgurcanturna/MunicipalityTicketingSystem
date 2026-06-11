# Step 03: Infrastructure - EF Core ve Redis Setup

## 🎯 Amaç
Bu adımda SharedKernel.Infrastructure katmanı, servislerin kullanabileceği şekilde EF Core ve Redis ile genişletilir.

Bu adım sonunda:
- Tenant bazlı connection string çözümleme altyapısı hazır olur.
- Ortak Infrastructure DI extension metodu hazır olur.
- Servis projeleri SharedKernel.Infrastructure referansı alır.
- appsettings dosyalarına connection string ve Redis ayarları eklenir.

---

## ✅ Önkoşullar
- Step 00 tamamlanmış olmalı.
- Step 01 tamamlanmış olmalı.
- Step 02 tamamlanmış olmalı.

---

## 1) SharedKernel.Infrastructure Projesini Güncelle

Dosya: core/SharedKernel/Infrastructure/SharedKernel.Infrastructure.csproj

### 1.1 Yeni klasör ve derleme dahil listesi
- DependencyInjection
- MultiTenancy
- Persistence (TenantConnectionStringResolver ile genişletildi)

### 1.2 Paketler
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Relational
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.Extensions.Caching.StackExchangeRedis
- Microsoft.Extensions.Configuration.Abstractions
- Microsoft.Extensions.DependencyInjection.Abstractions

---

## 2) Tenant ve Connection String Altyapısı

### 2.1 Tenant provider sözleşmesi
Dosya: core/SharedKernel/Infrastructure/MultiTenancy/ITenantProvider.cs

```csharp
namespace SharedKernel.Infrastructure.MultiTenancy;

public interface ITenantProvider
{
    string? TenantId { get; }
}

public sealed class NullTenantProvider : ITenantProvider
{
    public string? TenantId => null;
}
```

### 2.2 Tenant connection resolver
Dosya: core/SharedKernel/Infrastructure/Persistence/TenantConnectionStringResolver.cs

```csharp
using Microsoft.Extensions.Configuration;

namespace SharedKernel.Infrastructure.Persistence;

public sealed class TenantConnectionStringResolver
{
    private readonly IConfiguration _configuration;

    public TenantConnectionStringResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Resolve(string? tenantId)
    {
        var defaultConnection = _configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return defaultConnection ?? throw new InvalidOperationException(
                "ConnectionStrings:Default ayarı zorunludur.");
        }

        var tenantConnection = _configuration[$"ConnectionStrings:Tenants:{tenantId}"];

        if (!string.IsNullOrWhiteSpace(tenantConnection))
        {
            return tenantConnection;
        }

        return defaultConnection ?? throw new InvalidOperationException(
            $"Tenant '{tenantId}' için bağlantı bulunamadı ve Default bağlantısı tanımlı değil.");
    }
}
```

---

## 3) Infrastructure Dependency Injection Extension

Dosya: core/SharedKernel/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedKernel.Infrastructure.MultiTenancy;
using SharedKernel.Infrastructure.Persistence;

namespace SharedKernel.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSharedInfrastructure<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder, string>? dbOptions = null)
        where TDbContext : AppDbContext
    {
        services.TryAddScoped<ITenantProvider, NullTenantProvider>();
        services.TryAddSingleton<TenantConnectionStringResolver>();

        services.AddDbContext<TDbContext>((serviceProvider, options) =>
        {
            var tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            var resolver = serviceProvider.GetRequiredService<TenantConnectionStringResolver>();
            var connectionString = resolver.Resolve(tenantProvider.TenantId);

            if (dbOptions is not null)
            {
                dbOptions(options, connectionString);
                return;
            }

            options.UseSqlServer(connectionString);
        });

        ConfigureRedisCache(services, configuration);

        return services;
    }

    private static void ConfigureRedisCache(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "MunicipalityTicketing";
        });
    }
}
```

---

## 4) Servis Projelerine Infrastructure Referansı Ekle

Güncellenen dosyalar:
- services/identity/Tenant.Identity.Api.csproj
- services/wallet/Ticketing.Wallet.Api.csproj
- services/telemetry/Journey.Telemetry.Api.csproj
- workers/event-processor/Journey.EventProcessor.Worker.csproj

Her projede SharedKernel.Infrastructure project reference tanımlandı.

---

## 5) appsettings Yapılandırması

Güncellenen dosyalar:
- services/identity/appsettings.json
- services/wallet/appsettings.json
- services/telemetry/appsettings.json
- workers/event-processor/appsettings.json

Eklenen bölüm:
- ConnectionStrings:Default
- ConnectionStrings:Redis
- ConnectionStrings:Tenants:ankara
- ConnectionStrings:Tenants:istanbul

Bu yapı sayesinde tenant id yoksa Default bağlantı kullanılır, tenant id varsa tenant bazlı bağlantıya düşülür.

---

## 6) Doğrulama Komutları

```powershell
dotnet build core/SharedKernel/Domain/SharedKernel.Domain.csproj
dotnet build core/SharedKernel/Infrastructure/SharedKernel.Infrastructure.csproj
dotnet build MunicipalityTicketing.slnx
dotnet test MunicipalityTicketing.slnx
```

Beklenen sonuç: tüm build ve test adımları başarılı olmalı.

---

## 7) Tamamlanma Kontrol Listesi

- [x] SharedKernel.Infrastructure, EF provider ve Redis paketleri ile güncellendi.
- [x] Tenant provider sözleşmesi eklendi.
- [x] Tenant connection string resolver eklendi.
- [x] Infrastructure DI extension metodu eklendi.
- [x] Servis projelerine SharedKernel.Infrastructure referansı eklendi.
- [x] Servis appsettings dosyaları connection/redis ayarları ile güncellendi.
- [x] Çözüm build ve test doğrulandı.

---

## 8) Step 04 İçin İhtiyaç Analizi

Step 04 (Identity Service) için hazır olan altyapı ve kalan ihtiyaçlar:

### Hazır Olanlar
1. Tenant'a göre connection string çözümleme mekanizması
2. EF Core + Redis register eden ortak extension
3. Shared kernel domain ve repository altyapısı

### Kalan İhtiyaçlar
1. IdentityDbContext oluşturulması
2. Tenant/User aggregate modelleri
3. Identity'ye özel repository arayüzleri
4. API endpoint ve DTO sözleşmeleri
5. Tenant id'yi request'ten okuyan gerçek ITenantProvider implementasyonu

### Risk ve Aksiyon
1. Risk: Tenant id yanlış veya boş gelebilir
Aksiyon: Middleware ile tenant doğrulama eklenmeli.

2. Risk: Tenant connection map büyüdükçe yönetim zorlaşır
Aksiyon: Tenant connection'lar merkezi config provider veya secret store'a taşınmalı.

3. Risk: Redis tek nokta arızası
Aksiyon: Redis cluster/sentinel ve retry politikaları planlanmalı.

---

**Durum**: ✅ Tamamlandı (Doğrulandı)
**Son Güncelleme**: 11.06.2026
**Yazar**: Özgür Can TURNA