# Municipality Ticketing System - Skills & Guidelines

## Proje Yazarı
**Özgür Can TURNA**  
*Proje Lideri ve Tek Geliştirici (Simülasyon)*

Bu doküman, 15 kişilik hayali bir yazılım geliştirme ekibinin proje lideri olarak davranan AI agent'ın, belediye otobüs biletleme sistemi projesini DDD (Domain-Driven Design) prensiplerine uygun şekilde geliştirmesi için gerekli beceri ve yönergeleri içerir.

---

## 🎯 Temel Prensipler

### 1. Domain-Driven Design (DDD)
- **Ubiquitous Language**: Her terim domain uzmanlarının diliyle tanımlanmalı
- **Bounded Context**: Her mikroservis kendi sınırlı bağlamında bağımsız
- **Aggregates**: Transaction sınırları aggregate root'lar tarafından belirlenmeli
- **Value Objects**: Immutable, equals/hashcode tabanlı karşılaştırma
- **Entities**: Identity-based nesneler, lifecycle yönetimi
- **Repositories**: Interface-segregation, persistence-agnostic
- **Domain Events**: Eventual consistency için event sourcing pattern

### 2. Mikroservis Mimarisi
- **Single Responsibility**: Her servis tek bir business capability
- **Decentralized Data**: Her servisin kendi database'i
- **API Gateway**: YARP ile routing, authentication, rate limiting
- **Event-Driven**: Brighter/Darker ile async communication
- **Circuit Breaker**: Polly ile resilience patterns

### 3. Multi-Tenancy
- **Database-per-Tenant**: Her belediye izole database
- **Tenant Identification**: JWT claims veya header-based routing
- **Schema Isolation**: Tenant-specific schema migration
- **Resource Quotas**: Per-tenant rate limiting ve quota management

### 4. Performance & Scalability
- **Caching**: Redis distributed cache
- **Connection Pooling**: EF Core connection pooling
- **Async/Await**: Non-blocking I/O operations
- **Load Balancing**: Kubernetes HPA veya Docker Swarm scaling
- **Database Indexing**: Query performance optimization

### 5. Security
- **Authentication**: JWT with refresh tokens
- **Authorization**: Policy-based RBAC
- **Encryption**: TLS 1.3, AES-256 data at rest
- **Audit Logging**: Tüm kritik işlemler loglanır
- **Input Validation**: FluentValidation ile strong typing

### 6. DevOps & CI/CD
- **Containerization**: Docker multi-stage builds
- **Orchestration**: Kubernetes manifests veya Docker Compose
- **Zero-Downtime Deployment**: Blue-green veya rolling updates
- **Health Checks**: Kubernetes liveness/readiness probes
- **Monitoring**: OpenTelemetry + Prometheus + Grafana

---

## 🛠️ Tool Set & Kütüphaneler

| Kategori | Araç/Kütüphane | Lisans | Kullanım Amacı |
|----------|----------------|--------|----------------|
| **Framework** | .NET 10 | MIT | Backend development |
| **Message Bus** | Brighter & Darker | Apache 2.0 | Event-driven architecture |
| **ORM** | Entity Framework Core 10 | MIT | Data persistence |
| **Cache** | StackExchange.Redis | MIT | Distributed caching |
| **Validation** | FluentValidation | Apache 2.0 | Request validation |
| **Mapping** | AutoMapper | MIT | DTO mapping |
| **Logging** | Serilog | Apache 2.0 | Structured logging |
| **Tracing** | OpenTelemetry | Apache 2.0 | Distributed tracing |
| **API Gateway** | YARP | MIT | Reverse proxy & routing |
| **Testing** | xUnit, Moq, Shouldly | Various | Unit & integration tests |
| **Simulation** | Custom .NET Clients | MIT | Load testing & demo |
| **Documentation** | Markdown, Mermaid | MIT | Architecture docs |
| **Container** | Docker | Apache 2.0 | Containerization |
| **Orchestration** | Docker Compose / K8s | Apache 2.0 | Service orchestration |

---

## 📝 Kodlama Standartları

### Naming Conventions
```csharp
// Entities
public class Ticket : AggregateRoot<Guid> { }

// Value Objects
public record Money(decimal Amount, string Currency);

// Repositories
public interface ITicketRepository : IRepository<Ticket, Guid> { }

// Services
public interface ITicketPricingService { }
public class TicketPricingService : ITicketPricingService { }

// DTOs
public record CreateTicketRequest(Guid RouteId, DateTime TravelDate);
public record TicketResponse(Guid Id, string Status);

// Events
public record TicketPurchasedEvent(Guid TicketId, Guid UserId, decimal Amount);
```

### Project Structure
```
src/
├── SharedKernel/
│   ├── Domain/          # Base classes, interfaces
│   ├── Infrastructure/  # EF Core, Redis, etc.
│   └── Application/     # CQRS handlers, validators
├── Tenants.Identity.Api/
├── Ticketing.Wallet.Api/
├── Journey.Telemetry.Api/
├── Journey.EventProcessor.Worker/
├── ApiGateway.Yarp/
└── Clients/
    └── Simulation/      # Load testing clients

tests/
├── MunicipalityTicketing.UnitTests/
└── MunicipalityTicketing.IntegrationTests/

docs/
├── skills.md
├── Step-00-Planlama.md
├── Step-01-InitialSetup.md
└── Step-XX-*.md
```

### Error Handling
```csharp
// Use Result pattern for business logic
public Result<Ticket> PurchaseTicket(CreateTicketRequest request)
{
    if (!route.Exists())
        return Result.Failure<Ticket>("Route not found");
    
    // Business logic
    return Result.Success(ticket);
}

// Global exception handler middleware
app.UseExceptionHandler("/error");
```

### Testing Strategy
```csharp
// Unit Tests: Fast, isolated, mock dependencies
[Fact]
public void PurchaseTicket_InsufficientBalance_ReturnsFailure()
{
    // Arrange
    var wallet = new Wallet(userId, initialBalance: 10);
    
    // Act
    var result = wallet.Deduct(50);
    
    // Assert
    result.IsFailure.ShouldBeTrue();
}

// Integration Tests: Real dependencies, test boundaries
[Fact]
public async Task PurchaseTicket_EndToEnd_CreatesTicketAndDeductsBalance()
{
    // Use TestServer or Docker containers
}
```

---

## 🚀 Simulation Client Guidelines

Simulation client'ları aşağıdaki senaryoları test etmelidir:

### 1. Load Testing
- 10M+ daily active ticket validations
- 100K+ ticket/credit purchases per day
- 10K+ bus journey tracking events per day

### 2. Failure Scenarios
- Database connection failures
- Message bus downtime
- High latency scenarios
- Concurrent user conflicts

### 3. Multi-Tenant Isolation
- Verify tenant A cannot access tenant B's data
- Test resource quotas per tenant
- Validate independent scaling

### 4. Zero-Downtime Updates
- Deploy new version while simulation running
- Verify no dropped requests
- Test rollback procedures

---

## 📚 Referanslar

- [Domain-Driven Design Distilled](https://www.amazon.com/Domain-Driven-Design-Distilled-Vaughn-Vernon/dp/0134434420)
- [Building Microservices](https://www.amazon.com/Building-Microservices-Designing-Fine-Grained-Systems/dp/1491950358)
- [Brighter Documentation](https://www.goparamore.io/)
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)

---

## 🔄 Sürekli İyileştirme

Bu doküman proje ilerledikçe güncellenecektir. Her step'te:
1. Yeni öğrenilen dersler dokümante edilmeli
2. Best practice'ler güncellenmeli
3. Tool set değişiklikleri yansıtılmalı
4. Simulation senaryoları genişletilmeli

**Son Güncelleme**: 2024  
**Yazar**: Özgür Can TURNA
