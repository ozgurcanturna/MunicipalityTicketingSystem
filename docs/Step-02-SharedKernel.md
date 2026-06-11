# Step 02: Shared Kernel Implementation

## Amaç
Bu adımda, Domain-Driven Design (DDD) prensiplerine uygun olarak tüm mikroservislerin ortak kullanacağı temel sınıfları ve interface'leri içeren **Shared Kernel** katmanını oluşturuyoruz.

## Önkoşullar
- .NET 10 SDK kurulu olmalı
- Step 01'deki temizlik işlemleri tamamlanmış olmalı

## Adım 1: Proje Yapısını Oluşturma

### 1.1. Dizin Yapısı
Aşağıdaki dizin yapısını `core/` altında oluşturun:

```
core/
├── SharedKernel.Domain/
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
└── SharedKernel.Infrastructure/
    ├── Persistence/
    │   └── AppDbContext.cs
    ├── Repositories/
    │   └── Repository.cs
    └── SharedKernel.Infrastructure.csproj
```

### 1.2. Komutlarla Oluşturma

```bash
# Shared Kernel dizinlerini oluştur
mkdir -p core/SharedKernel.Domain/{Entities,Events,Common,Repositories}
mkdir -p core/SharedKernel.Infrastructure/{Persistence,Repositories}
```

## Adım 2: SharedKernel.Domain Projesi

### 2.1. Proje Dosyası
`src/BuildingBlocks/SharedKernel/SharedKernel.Domain/SharedKernel.Domain.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>SharedKernel.Domain</RootNamespace>
  </PropertyGroup>
</Project>
```

### 2.2. Entity Temel Sınıfı
**Dosya:** `Entities/Entity.cs`

Tüm domain entity'leri için temel sınıf. Her entity'nin bir kimliği, audit alanları ve domain event desteği vardır.

**Özellikler:**
- `Id`: Guid tipinde benzersiz kimlik
- `CreatedAt`, `UpdatedAt`: Audit zaman damgaları
- `DomainEvents`: Domain event koleksiyonu
- Value-based equality (==, !=, Equals, GetHashCode)

```csharp
namespace SharedKernel.Domain.Entities;

public abstract class Entity
{
    private List<IDomainEvent>? _domainEvents;
    
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents?.AsReadOnly() ?? new List<IDomainEvent>();

    protected void AddDomainEvent(IDomainEvent domainEvent) { ... }
    public IEnumerable<IDomainEvent> ClearDomainEvents() { ... }
    protected void MarkAsModified() { UpdatedAt = DateTime.UtcNow; }
    
    // Equality operators
    public override bool Equals(object? obj) { ... }
    public override int GetHashCode() { ... }
    public static bool operator ==(Entity left, Entity right) { ... }
    public static bool operator !=(Entity left, Entity right) { ... }
}
```

### 2.3. Aggregate Root Temel Sınıfı
**Dosya:** `Entities/AggregateRoot.cs`

DDD'de aggregate root, bir tutarlılık sınırının giriş noktasıdır. Tüm iş kuralları burada uygulanır.

```csharp
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
        AddDomainEvent(new DomainEvents.EntityCreatedEvent(Id));
    }

    protected void RegisterDeleted()
    {
        AddDomainEvent(new DomainEvents.EntityDeletedEvent(Id));
    }
}
```

**Kullanım Örneği:**
```csharp
public class User : AggregateRoot
{
    public string Email { get; private set; }
    public string Name { get; private set; }

    private User() { } // EF Core için

    public static User Create(string email, string name)
    {
        var user = new User { Email = email, Name = name };
        user.RegisterCreated();
        return user;
    }

    public void UpdateName(string newName)
    {
        Name = newName;
        RaiseVersion();
        AddDomainEvent(new UserUpdatedEvent(Id));
    }
}
```

### 2.4. Value Object Temel Sınıfı
**Dosya:** `Common/ValueObject.cs`

Kimliği olmayan, sadece özellikleriyle tanımlanan nesneler için temel sınıf.

```csharp
namespace SharedKernel.Domain.Common;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj) { ... }
    public override int GetHashCode() { ... }
    public static bool operator ==(ValueObject left, ValueObject right) { ... }
    public static bool operator !=(ValueObject left, ValueObject right) { ... }
}
```

**Kullanım Örneği:**
```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

### 2.5. Domain Event Interface ve Sınıfları
**Dosya:** `Events/DomainEvent.cs`

Domain katmanında gerçekleşen önemli olayları temsil eder.

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

public sealed class EntityCreatedEvent : DomainEvent
{
    public Guid EntityId { get; }
    public EntityCreatedEvent(Guid entityId) => EntityId = entityId;
}

public sealed class EntityDeletedEvent : DomainEvent
{
    public Guid EntityId { get; }
    public EntityDeletedEvent(Guid entityId) => EntityId = entityId;
}

public sealed class EntityUpdatedEvent : DomainEvent
{
    public Guid EntityId { get; }
    public EntityUpdatedEvent(Guid entityId) => EntityId = entityId;
}
```

### 2.6. Repository Interface
**Dosya:** `Repositories/IRepository.cs`

Persistence katmanı için soyutlama sağlar.

```csharp
namespace SharedKernel.Domain.Repositories;

public interface IRepository<TEntity> where TEntity : AggregateRoot
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IRepository
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

## Adım 3: SharedKernel.Infrastructure Projesi

### 3.1. Proje Dosyası
**Dosya:** `SharedKernel.Infrastructure/SharedKernel.Infrastructure.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>SharedKernel.Infrastructure</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\SharedKernel.Domain\SharedKernel.Domain.csproj" />
  </ItemGroup>
</Project>
```

### 3.2. AppDbContext Temel Sınıfı
**Dosya:** `Persistence/AppDbContext.cs`

Tüm mikroservislerin DbContext sınıfları bu sınıftan türer.

```csharp
namespace SharedKernel.Infrastructure.Persistence;

public abstract class AppDbContext : DbContext
{
    protected AppDbContext(DbContextOptions options) : base(options) { }

    public override int SaveChanges()
    {
        UpdateAuditableEntities();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        var now = DateTime.UtcNow;
        var modifiedEntities = ChangeTracker.Entries<Domain.Entities.Entity>()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity);

        foreach (var entity in modifiedEntities)
        {
            entity.GetType().GetProperty("UpdatedAt")?.SetValue(entity, now);
        }
    }

    protected virtual void ConfigureMultiTenancy(ModelBuilder modelBuilder)
    {
        // Multi-tenancy desteği için alt sınıflar tarafından override edilir
    }
}
```

### 3.3. Generic Repository Implementasyonu
**Dosya:** `Repositories/Repository.cs`

```csharp
namespace SharedKernel.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : AggregateRoot
{
    protected readonly DbContext _dbContext;

    public Repository(DbContext dbContext) => _dbContext = dbContext;

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public virtual void Update(TEntity entity) => _dbContext.Set<TEntity>().Update(entity);
    public virtual void Delete(TEntity entity) => _dbContext.Set<TEntity>().Remove(entity);

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

## Adım 4: Çözüm Dosyasına Projeleri Ekleme

Mevcut `MunicipalityTicketing.slnx` çözüm dosyasına yeni projeleri ekleyin:

```bash
dotnet sln MunicipalityTicketing.slnx add core/SharedKernel.Domain/SharedKernel.Domain.csproj
dotnet sln MunicipalityTicketing.slnx add core/SharedKernel.Infrastructure/SharedKernel.Infrastructure.csproj
```

## Adım 5: Build Testi

Projelerin doğru şekilde oluşturulduğunu test edin:

```bash
dotnet build core/SharedKernel.Domain/SharedKernel.Domain.csproj
dotnet build core/SharedKernel.Infrastructure/SharedKernel.Infrastructure.csproj
```

## Özet

Bu adımda şunları oluşturduk:
- ✅ **Entity**: Tüm domain entity'leri için temel sınıf
- ✅ **AggregateRoot**: DDD aggregate root implementasyonu
- ✅ **ValueObject**: Değer nesneleri için temel sınıf
- ✅ **IDomainEvent & DomainEvent**: Domain event altyapısı
- ✅ **IRepository**: Generic repository interface
- ✅ **AppDbContext**: EF Core DbContext temel sınıfı
- ✅ **Repository<T>**: Generic repository implementasyonu

## Sonraki Adım

Bir sonraki adımda (**Step 03**), bu shared kernel'ı kullanarak ilk mikroservisimiz olan **Tenant.Identity.Api** projesini geliştireceğiz:
- Tenant (Belediye) entity'si
- User entity'si
- Identity servisleri
- API endpoints

## Notlar

- Tüm kodlar **nullable reference types** kullanır
- **Implicit usings** etkinleştirilmiştir
- EF Core 9.0 kullanılmıştır (.NET 10 ile uyumlu)
- Domain event pattern ile loose coupling sağlanmıştır
- Repository pattern ile persistence soyutlaması yapılmıştır
