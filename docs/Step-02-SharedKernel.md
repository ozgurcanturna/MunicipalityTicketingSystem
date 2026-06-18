# Step 02: Shared Kernel Implementation

## 🎯 Amaç

Bu adımda, tüm servislerin ortak kullanacağı Shared Kernel katmanı oluşturulur. Çıktı olarak Domain ve Infrastructure katmanlarında tekrar kullanılabilir temel sınıflar, event altyapısı ve generic repository yapısı hazırlanır.

## ✅ Beklenen Çıktılar

- SharedKernel.Domain projesi oluşturulmuş ve derlenebilir olmalı.
- SharedKernel.Infrastructure projesi oluşturulmuş, Domain'e referans vermeli ve derlenebilir olmalı.
- Entity, AggregateRoot, ValueObject, DomainEvent ve Repository altyapısı kodlanmış olmalı.
- Solution içerisine SharedKernel projeleri eklenmiş olmalı.

## Önkoşullar

- .NET 10 SDK kurulu olmalı.
- Step 01 tamamlanmış olmalı.

## 1) Proje Yapısı

### 1.1 Hedef dizin yapısı

```text
core/
└── SharedKernel/
    ├── Domain/
    │   ├── Common/
    │   │   └── ValueObject.cs
    │   ├── Entities/
    │   │   ├── Entity.cs
    │   │   └── AggregateRoot.cs
    │   ├── Events/
    │   │   └── DomainEvent.cs
    │   ├── Repositories/
    │   │   └── IRepository.cs
    │   └── SharedKernel.Domain.csproj
    └── Infrastructure/
        ├── Persistence/
        │   └── AppDbContext.cs
        ├── Repositories/
        │   └── Repository.cs
        └── SharedKernel.Infrastructure.csproj
```

### 1.2 PowerShell ile dizinleri oluşturma

```powershell
New-Item -ItemType Directory -Force -Path core/SharedKernel/Domain/Common
New-Item -ItemType Directory -Force -Path core/SharedKernel/Domain/Entities
New-Item -ItemType Directory -Force -Path core/SharedKernel/Domain/Events
New-Item -ItemType Directory -Force -Path core/SharedKernel/Domain/Repositories
New-Item -ItemType Directory -Force -Path core/SharedKernel/Infrastructure/Persistence
New-Item -ItemType Directory -Force -Path core/SharedKernel/Infrastructure/Repositories
```

## 2) SharedKernel.Domain

### 2.1 SharedKernel.Domain.csproj

Dosya: core/SharedKernel/Domain/SharedKernel.Domain.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>SharedKernel.Domain</RootNamespace>
    <EnableDefaultItems>false</EnableDefaultItems>
  </PropertyGroup>

  <ItemGroup>
    <Folder Include="Common\" />
    <Folder Include="Entities\" />
    <Folder Include="Events\" />
    <Folder Include="Repositories\" />
  </ItemGroup>

  <ItemGroup>
    <Compile Include="Common\ValueObject.cs" />
    <Compile Include="Entities\AggregateRoot.cs" />
    <Compile Include="Entities\Entity.cs" />
    <Compile Include="Events\DomainEvent.cs" />
    <Compile Include="Repositories\IRepository.cs" />
  </ItemGroup>

</Project>
```

### 2.2 Entity.cs

Dosya: core/SharedKernel/Domain/Entities/Entity.cs

```csharp
using SharedKernel.Domain.Events;

namespace SharedKernel.Domain.Entities;

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public IReadOnlyCollection<IDomainEvent> ClearDomainEvents()
    {
        var domainEvents = _domainEvents.ToArray();
        _domainEvents.Clear();
        return domainEvents;
    }

    protected void MarkAsModified()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return Id != Guid.Empty && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !Equals(left, right);
    }
}
```

### 2.3 AggregateRoot.cs

Dosya: core/SharedKernel/Domain/Entities/AggregateRoot.cs

```csharp
using SharedKernel.Domain.Events;

namespace SharedKernel.Domain.Entities;

public abstract class AggregateRoot : Entity
{
    public long Version { get; protected set; } = 1;

    protected void RaiseVersion()
    {
        Version++;
        MarkAsModified();
    }

    protected void RegisterCreated()
    {
        AddDomainEvent(new EntityCreatedEvent(Id));
    }

    protected void RegisterUpdated()
    {
        RaiseVersion();
        AddDomainEvent(new EntityUpdatedEvent(Id));
    }

    protected void RegisterDeleted()
    {
        AddDomainEvent(new EntityDeletedEvent(Id));
    }
}
```

### 2.4 ValueObject.cs

Dosya: core/SharedKernel/Domain/Common/ValueObject.cs

```csharp
namespace SharedKernel.Domain.Common;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject other || other.GetType() != GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(0, (current, component) => HashCode.Combine(current, component));
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
}
```

### 2.5 DomainEvent.cs

Dosya: core/SharedKernel/Domain/Events/DomainEvent.cs

```csharp
namespace SharedKernel.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
    string EventType => GetType().Name;
}

public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed class EntityCreatedEvent(Guid entityId) : DomainEvent
{
    public Guid EntityId { get; } = entityId;
}

public sealed class EntityUpdatedEvent(Guid entityId) : DomainEvent
{
    public Guid EntityId { get; } = entityId;
}

public sealed class EntityDeletedEvent(Guid entityId) : DomainEvent
{
    public Guid EntityId { get; } = entityId;
}
```

### 2.6 IRepository.cs

Dosya: core/SharedKernel/Domain/Repositories/IRepository.cs

```csharp
using System.Linq.Expressions;
using SharedKernel.Domain.Entities;

namespace SharedKernel.Domain.Repositories;

public interface IRepository<TEntity> : IRepository where TEntity : AggregateRoot
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}

public interface IRepository
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

## 3) SharedKernel.Infrastructure

### 3.1 SharedKernel.Infrastructure.csproj

Dosya: core/SharedKernel/Infrastructure/SharedKernel.Infrastructure.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>SharedKernel.Infrastructure</RootNamespace>
    <EnableDefaultItems>false</EnableDefaultItems>
  </PropertyGroup>

  <ItemGroup>
    <Folder Include="Persistence\" />
    <Folder Include="Repositories\" />
  </ItemGroup>

  <ItemGroup>
    <Compile Include="Persistence\AppDbContext.cs" />
    <Compile Include="Repositories\Repository.cs" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.9" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="10.0.9" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Domain\SharedKernel.Domain.csproj" />
  </ItemGroup>

</Project>
```

### 3.2 AppDbContext.cs

Dosya: core/SharedKernel/Infrastructure/Persistence/AppDbContext.cs

```csharp
using Microsoft.EntityFrameworkCore;
using SharedKernel.Domain.Entities;

namespace SharedKernel.Infrastructure.Persistence;

public abstract class AppDbContext : DbContext
{
    protected AppDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public override int SaveChanges()
    {
        UpdateAuditableEntities();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateAuditableEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureMultiTenancy(modelBuilder);
    }

    protected virtual void ConfigureMultiTenancy(ModelBuilder modelBuilder)
    {
    }

    private void UpdateAuditableEntities()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(entity => entity.CreatedAt).CurrentValue = utcNow;
                entry.Property(entity => entity.UpdatedAt).CurrentValue = null;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.UpdatedAt).CurrentValue = utcNow;
            }
        }
    }
}
```

### 3.3 Repository.cs

Dosya: core/SharedKernel/Infrastructure/Repositories/Repository.cs

```csharp
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Domain.Entities;
using SharedKernel.Domain.Repositories;
using SharedKernel.Infrastructure.Persistence;

namespace SharedKernel.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : AggregateRoot
{
    protected readonly AppDbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(AppDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return DbSet.AddAsync(entity, cancellationToken).AsTask();
    }

    public void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public void Delete(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return DbContext.SaveChangesAsync(cancellationToken);
    }
}
```

## 4) Solution'a projeleri ekleme

```powershell
dotnet sln MunicipalityTicketing.slnx add core/SharedKernel/Domain/SharedKernel.Domain.csproj
dotnet sln MunicipalityTicketing.slnx add core/SharedKernel/Infrastructure/SharedKernel.Infrastructure.csproj
```

Not: Projeler zaten ekliyse komut bilgi mesajı verebilir.

## 5) Doğrulama

```powershell
dotnet build core/SharedKernel/Domain/SharedKernel.Domain.csproj
dotnet build core/SharedKernel/Infrastructure/SharedKernel.Infrastructure.csproj
dotnet build MunicipalityTicketing.slnx
dotnet test MunicipalityTicketing.slnx
```

## 6) Step 02 Tamamlanma Kontrol Listesi

- ✅ Domain base sınıfları oluşturuldu.
- ✅ Domain event altyapısı oluşturuldu.
- ✅ Generic repository sözleşmesi oluşturuldu.
- ✅ Infrastructure AppDbContext ve generic repository implementasyonu tamamlandı.
- ✅ SharedKernel projeleri solution içinde.
- ✅ Build ve testler başarılı.

## 7) Sonraki Adımlar İçin İhtiyaç Analizi

Step 03 ve sonrasında zorlanmamak için bu adımın üzerine aşağıdaki ihtiyaçlar netleştirilmelidir.

### 7.1 Teknik ihtiyaçlar

- Her servis için kendi DbContext sınıfı (örnek: IdentityDbContext, WalletDbContext, TelemetryDbContext).
- Her servis için ayrı connection string ve tenant çözümleme stratejisi.
- EF Core migration stratejisi (servis bazlı migration klasörü).
- Domain event publish mekanizması (outbox veya transaction sonrası dispatcher).
- Ortak error-handling ve Result pattern standardı.

### 7.2 Step 03 için minimum backlog

1. SharedKernel.Infrastructure üzerine tenant-aware DbContext tabanı ekleme.
2. Servis projelerine SharedKernel referanslarını ekleme.
3. Identity servisinde ilk aggregate (Tenant/User) modelleme.
4. İlk repository implementasyonunun servis bazında türetilmesi.
5. İlk migration ve local çalışma doğrulaması.

### 7.3 Test ihtiyaçları

1. Entity ve ValueObject eşitlik davranışı için unit test.
2. Repository temel CRUD davranışı için integration test.
3. AppDbContext audit alanlarının otomatik set edilmesi için test.

## 8) Notlar

- Proje .NET 10 ve EF Core 10.0.9 ile çalışır.
- Nullable reference types ve implicit usings açıktır.
- Projede explicit compile yaklaşımı kullanıldığı için yeni dosya eklendiğinde csproj dosyasına Include satırı eklenmelidir.

---

**Durum**: ✅ Tamamlandı  
**Son Güncelleme**: 11.06.2026  
**Yazar**: Özgür Can TURNA
