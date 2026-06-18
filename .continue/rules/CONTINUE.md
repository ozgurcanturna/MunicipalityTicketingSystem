# Municipality Ticketing System - CONTINUE.md Project Guide

## 1. Project Overview

**Municipality Ticketing System** is a highly scalable, multi-tenant bus ticketing system developed for municipalities. This project demonstrates DDD (Domain-Driven Design) principles and microservices architecture, simulating a 15-person software team.

### Key Technologies
- **.NET 10** - Primary framework
- **C#** with C# 13+ features (minimal APIs, pattern matching, records)
- **Entity Framework Core 10** - ORM with per-tenant database isolation
- **Brighter & Darker** - Event-driven architecture with RabbitMQ
- **StackExchange.Redis** - Distributed caching
- **YARP (Yet Another Reverse Proxy)** - API Gateway
- **OpenTelemetry** - Distributed tracing
- **Serilog** - Structured logging
- **Docker & Docker Compose** - Container orchestration
- **xUnit, Moq, Shouldly** - Testing framework

### High-Level Architecture
```
Client Layer (Mobile/Web/Simulator) → YARP Gateway → Microservices (Identity/Wallet/Telemetry)
                                           ↓
                                    Event Bus (RabbitMQ)
                                           ↓
                              Event Processor Worker (Background Service)
                                           ↓
                              Per-Tenant Databases (PostgreSQL)
```

---

## 2. Getting Started

### Prerequisites
- .NET 10 SDK
- Docker & Docker Compose
- Git
- Visual Studio 2022 or Rider (recommended)

### Installation

```bash
# Clone repository
git clone <repo-url>
cd MunicipalityTicketing

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run with Docker Compose
docker-compose up -d

# Run individual services locally
dotnet run --project services/identity/Tenant.Identity.Api --environment Development
```

### Configuration Files

| File | Purpose |
|------|---------|
| `appsettings.json` | Default configuration |
| `appsettings.Development.json` | Development-specific settings |
| `appsettings.Docker.json` | Docker environment settings |
| `docker-compose.yml` | Container orchestration |
| `launchSettings.json` | Debug configuration |

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/MunicipalityTicketing.UnitTests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## 3. Project Structure

```
MunicipalityTicketing/
├── core/                          # Shared Kernel (Domain & Infrastructure)
│   ├── SharedKernel/
│   │   ├── Domain/                # Domain models, entities, value objects
│   │   │   ├── Common/           # Base classes (Entity, AggregateRoot, ValueObject)
│   │   │   ├── Entities/         # Domain entities
│   │   │   ├── Events/           # Domain events
│   │   │   └── Repositories/     # Repository interfaces
│   │   └── Infrastructure/        # Implementation details
│   │       ├── Persistence/      # EF Core DbContext, migrations
│   │       ├── Repositories/     # Repository implementations
│   │       ├── MultiTenancy/     # Multi-tenant provider
│   │       └── DependencyInjection/
│
├── services/                      # Microservices (API Layer)
│   ├── identity/                  # Tenant.Identity.Api - User & tenant management
│   ├── wallet/                    # Ticketing.Wallet.Api - Payment & wallet
│   └── telemetry/                 # Journey.Telemetry.Api - Journey tracking
│
├── workers/                       # Background Workers
│   └── event-processor/           # Journey.EventProcessor.Worker
│       ├── Configuration/        # Configuration models
│       ├── Events/               # Integration event definitions
│       ├── Processing/           # Event handlers
│       └── Storage/              # Event persistence (InMemory implementation)
│
├── gateway/                       # API Gateway
│   └── ApiGateway.Yarp/           # YARP reverse proxy
│
├── tools/                         # Development & Simulation Tools
│   └── simulator/                 # Load testing simulation client
│
├── tests/                         # Test projects
│   ├── MunicipalityTicketing.UnitTests
│   └── MunicipalityTicketing.IntegrationTests
│
├── docs/                          # Documentation
│   ├── skills.md                  # Developer guidelines
│   └── Step-XX-*.md               # Step-by-step implementation guides
│
├── docker-compose.yml             # Container orchestration
├── MunicipalityTicketing.slnx     # Solution file
└── README.md
```

---

## 4. Development Workflow

### Coding Standards
- Use **minimal APIs** for endpoint definitions
- Follow **C# 13+ features** (records, pattern matching, collection expressions)
- **Immutable** by default - prefer `readonly`, `init` only properties
- **Nullable reference types** enabled
- **Implicit usings** enabled (`<ImplicitUsings>enable</ImplicitUsings>`)
- Follow **SOLID principles**
- **Dependency Injection** - constructor injection only
- **Async/await** - async all the way down

### Event-Driven Architecture

The project uses **Brighter & Darker** framework for event processing:

1. **Events** are defined as immutable records (`IntegrationEvent`)
2. **Handlers** implement `IIntegrationEventHandler`
3. **Resolver** (`IEventHandlerResolver`) maps event types to handlers
4. **Retry & Dead Letter** - Automatic retry with configurable attempts, failed events go to DLQ

### Multi-Tenancy Model
- **Database-per-tenant** - Each tenant has isolated database
- **Tenant context** - Propagated via HTTP headers and event payloads
- **Connection pooling** - Per-tenant connection strings resolved at runtime

### Build & Deployment
```bash
# Build Docker images
docker-compose build

# Run all services
docker-compose up -d

# View logs
docker-compose logs -f event-processor
docker-compose logs -f identity-api

# Stop services
docker-compose down
```

---

## 5. Key Concepts

### Domain-Driven Design (DDD)
- **Bounded Contexts**: Identity, Wallet, Telemetry, EventProcessor
- **Aggregate Roots**: Core entities with business rules
- **Domain Events**: Internal state changes (e.g., `UserCreated`)
- **Integration Events**: Cross-service events (e.g., `Identity.TenantCreated`)

### Event Processing Pattern
```
Producer Service → RabbitMQ → Event Processor Worker → Per-Tenant DB
                              ↓
                    [Retry Logic] → [Dead Letter Queue]
```

### Integration Event Structure
```csharp
public record IntegrationEvent(
    Guid EventId,           // Unique event identifier
    string TenantId,        // Multi-tenant isolation
    string EventType,       // Event type for handler routing
    string Payload,         // JSON payload
    DateTime OccurredAt,    // Event timestamp
    string CorrelationId    // Request tracing
);
```

### Handler Registration
Handlers are registered as **scoped services**:
```csharp
builder.Services.AddScoped<IIntegrationEventHandler, WalletDebitedEventHandler>();
```

---

## 6. Common Tasks

### Adding a New Event Handler

1. Define the event type in payload (or create new event record)
2. Create handler class:
```csharp
public sealed class NewEventHandler(ILogger<NewEventHandler> logger)
    : IIntegrationEventHandler
{
    public string EventType => "New.Event.Type";
    
    public Task HandleAsync(IntegrationEvent integrationEvent, 
                           CancellationToken cancellationToken)
    {
        logger.LogInformation("Handled: {@Event}", integrationEvent);
        return Task.CompletedTask;
    }
}
```
3. Register in `Program.cs`:
```csharp
builder.Services.AddScoped<IIntegrationEventHandler, NewEventHandler>();
```

### Adding New Tenant Database

1. Create new PostgreSQL database
2. Add connection string to `appsettings.json`
3. Use `PostgresDatabaseInitializer` to create schema
4. Ensure tenant provisioning middleware is configured

### Running Migration

```bash
cd core/SharedKernel/Infrastructure
dotnet ef migrations add NewMigrationName
dotnet ef database update
```

### Debugging Event Processing

1. Enable detailed logging in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Journey.EventProcessor.Worker": "Debug"
    }
  }
}
```

2. View event queue via InMemoryEventQueue (development only)
3. Check dead-letter store for failed events

---

## 7. Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| `Unable to connect to RabbitMQ` | Check docker-compose is running: `docker-compose ps` |
| `Tenant connection string not found` | Verify tenant provisioning and connection resolver |
| `Event handler not found` | Check `EventType` matches exactly in handler registration |
| `Database migration failed` | Run `dotnet ef database update` on the specific tenant DB |
| `Redis connection timeout` | Check Redis container is running and accessible |

### Debugging Tips

1. **Enable Request Correlation**: Check `CorrelationId` in event payloads
2. **View Event Flow**: Enable debug logging in event processor
3. **Check Dead Letter Queue**: Inspect `IDeadLetterStore` for failed events
4. **Multi-Tenant Isolation**: Verify tenant context propagates correctly

---

## 8. References

### Project Documentation
- [README.md](../README.md) - Project overview
- [docs/skills.md](../docs/skills.md) - Developer guidelines
- [docs/Step-00-Planlama.md](../docs/Step-00-Planlama.md) - Requirements & architecture
- [docs/Step-07-EventProcessor.md](../docs/Step-07-EventProcessor.md) - Event processing details

### External Resources
- [Brighter Framework](https://learnbattles.github.io/Brighter/) - Event-driven .NET
- [Darker Framework](https://github.com/ExpressiveCollections/Darker) - Query handlers
- [YARP](https://github.com/microsoft/reverse-proxy) - API Gateway
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) - ORM

### Architecture Decision Records (ADRs)
- Per-tenant database isolation
- Event-driven communication pattern
- In-memory event queue for development
- Retry with exponential backoff

---

## Additional Rules for Subdirectories

Create `rules.md` files in subdirectories for component-specific guidelines:

```
.continue/rules/
├── CONTINUE.md              # This file - Project-wide guide
├── services/identity/rules.md    # Identity service specific rules
├── core/SharedKernel/rules.md    # Shared kernel conventions
└── workers/event-processor/rules.md  # Event processing rules
```

---

*Note: This guide should be reviewed and updated regularly as the project evolves. The project is currently in MVP stage with ongoing hardening for production deployment.*