# AI Agent Skills & Project Rules

> **Project**: Municipality Bus Ticketing System  
> **Author**: Özgür Can TURNA  
> **Role**: Project Lead (simulating a 15-person dev team)  
> **Runtime**: .NET 10 | C# 14  

---

## 1. Architecture Constraints

| Rule | Detail |
|------|--------|
| Pattern | DDD (Domain-Driven Design) + CQRS |
| Style | Microservices with event-driven communication |
| Multi-Tenancy | Database-per-Tenant (full isolation) |
| Messaging | Brighter (commands) & Darker (queries) |
| API Gateway | YARP reverse proxy |
| Caching | Redis (StackExchange.Redis) |
| ORM | Entity Framework Core 10 |
| Deployment | Docker Compose → Kubernetes |

---

## 2. Project Structure

```
MunicipalityTicketing/
├── core/SharedKernel/
│   ├── Domain/           # Base classes: Entity, AggregateRoot, ValueObject
│   │   ├── Common/       # ValueObject, Result pattern
│   │   ├── Entities/     # Entity, AggregateRoot
│   │   ├── Events/       # IDomainEvent, DomainEvent
│   │   └── Repositories/ # IRepository<T>
│   └── Infrastructure/   # EF Core, Redis implementations
│       ├── Persistence/  # AppDbContext
│       └── Repositories/ # Repository<T>
├── services/
│   ├── identity/         # Tenant.Identity.Api
│   ├── wallet/           # Ticketing.Wallet.Api
│   └── telemetry/        # Journey.Telemetry.Api
├── workers/
│   └── event-processor/  # Journey.EventProcessor.Worker
├── gateway/              # ApiGateway.Yarp
├── tools/simulator/      # Load testing & simulation clients
├── tests/
│   ├── MunicipalityTicketing.UnitTests/
│   └── MunicipalityTicketing.IntegrationTests/
├── docs/                 # Step-XX docs, this file
└── MunicipalityTicketing.slnx
```

---

## 3. Naming Conventions

```csharp
// === Entities (inherit from AggregateRoot) ===
public class Ticket : AggregateRoot { }

// === Value Objects (inherit from ValueObject) ===
public class Money : ValueObject { }

// === Repository Interfaces ===
public interface ITicketRepository : IRepository<Ticket> { }

// === Domain Services ===
public interface ITicketPricingService { }
public class TicketPricingService : ITicketPricingService { }

// === DTOs (use records) ===
public record CreateTicketRequest(Guid RouteId, DateTime TravelDate);
public record TicketResponse(Guid Id, string Status);

// === Domain Events (use records) ===
public record TicketPurchasedEvent(Guid TicketId, Guid UserId, decimal Amount);
```

### File & Namespace Rules
- **Namespace** = folder path (e.g., `SharedKernel.Domain.Entities`)
- **One class per file** — filename matches class name
- **Nullable reference types**: always enabled
- **Implicit usings**: always enabled

---

## 4. Coding Standards

### Error Handling
- Use **Result pattern** for business logic — never throw exceptions for expected failures
- Use **global exception middleware** for unhandled errors
- Return **Problem Details (RFC 7807)** from API endpoints

```csharp
// Result pattern
public Result<Ticket> PurchaseTicket(CreateTicketRequest request)
{
    if (!route.Exists())
        return Result.Failure<Ticket>("Route not found");
    return Result.Success(ticket);
}
```

### Async
- All I/O operations **must** be async (`Task<T>`, `CancellationToken`)
- Use `ConfigureAwait(false)` in library code
- Never use `.Result` or `.Wait()` — no sync-over-async

### Validation
- Use **FluentValidation** for request validation
- Validate at API boundary, not in domain layer
- Domain enforces invariants via constructor/factory methods

### Dependency Injection
- Register services in `Program.cs` using extension methods
- Use `IServiceCollection` extensions per feature area
- Scoped lifetime for DbContext and repositories
- Singleton for configuration and caching services

---

## 5. DDD Rules

| Concept | Rule |
|---------|------|
| **Entity** | Has identity (`Guid Id`), mutable, tracked by EF Core |
| **AggregateRoot** | Transaction boundary, owns child entities, has `Version` |
| **ValueObject** | Immutable, equality by components, no identity |
| **DomainEvent** | Raised inside aggregates, dispatched after `SaveChanges` |
| **Repository** | One per aggregate root, interface in Domain, impl in Infrastructure |

### Aggregate Design Rules
1. Aggregates reference other aggregates **by ID only**
2. One transaction = one aggregate modification
3. Use domain events for cross-aggregate side effects
4. Factory methods (`Create()`) for entity construction — no public constructors

---

## 6. Multi-Tenancy Rules

- Every request **must** carry a `TenantId` (JWT claim or header)
- Tenant resolution happens at middleware level before any business logic
- Database connection string is resolved per tenant
- **Never** share data across tenants — enforce at query level
- Per-tenant rate limiting at API Gateway

---

## 7. Technology Stack

| Category | Tool | Purpose |
|----------|------|---------|
| Framework | .NET 10 | Backend runtime |
| Messaging | Brighter & Darker | CQRS, event-driven |
| ORM | EF Core 10 | Data persistence |
| Cache | StackExchange.Redis | Distributed caching |
| Validation | FluentValidation | Request validation |
| Logging | Serilog | Structured logging |
| Tracing | OpenTelemetry | Distributed tracing |
| Gateway | YARP | Reverse proxy & routing |
| Testing | xUnit, Moq, Shouldly | Unit & integration tests |
| Container | Docker & Docker Compose | Containerization |

---

## 8. Testing Strategy

| Layer | Framework | Scope |
|-------|-----------|-------|
| Unit | xUnit + Moq + Shouldly | Domain logic, services |
| Integration | xUnit + TestServer | API endpoints, DB queries |
| Load | Custom simulator clients | 10M+ daily transactions |

```csharp
// Unit test pattern: MethodName_Condition_ExpectedResult
[Fact]
public void Deduct_InsufficientBalance_ReturnsFailure()
{
    var wallet = new Wallet(userId, initialBalance: 10);
    var result = wallet.Deduct(50);
    result.IsFailure.ShouldBeTrue();
}
```

### Test coverage target: >80%

---

## 9. Performance Targets

| Metric | Target |
|--------|--------|
| API response (p95) | < 200ms |
| DB query (p95) | < 50ms |
| Throughput per tenant | 10K req/sec |
| Daily ticket validations | 10M+ |
| Daily purchases | 100K+ |
| Daily journey events | 10K+ |

---

## 10. Step-by-Step Docs

Development follows numbered step documents in `docs/`:

| Step | Topic |
|------|-------|
| 00 | Planning & Requirements |
| 01 | Initial Setup & Template Cleanup |
| 02 | Shared Kernel — Domain Base Classes |
| 03 | Infrastructure — EF Core & Redis |
| 04 | Identity Service |
| 05 | Wallet Service |
| 06 | Telemetry Service |
| 07 | Event Processor Worker |
| 08 | API Gateway (YARP) |
| 09 | Testing |
| 10 | Simulation Clients |
| 11 | Docker & Deployment |

> **Rule**: Execute steps sequentially. Do NOT skip ahead. Each step builds on the previous one.

---

## 11. References

- [Brighter Docs](https://www.goparamore.io/)
- [YARP Docs](https://microsoft.github.io/reverse-proxy/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [EF Core Docs](https://learn.microsoft.com/en-us/ef/core/)

---

**Last Updated**: 11.06.2026  
**Author**: Özgür Can TURNA
