"# Redis & OpenTelemetry Infrastructure Setup

This document describes the Redis and OpenTelemetry infrastructure added to the Municipality Ticketing System.

## Overview

### Redis Distributed Caching
- **Purpose**: High-performance distributed caching layer for session storage, JWT token caching, and hot data
- **Implementation**: StackExchange.Redis with per-service cache interfaces
- **Services Using Redis**: Identity, Wallet, Telemetry

### OpenTelemetry Distributed Tracing
- **Purpose**: End-to-end request tracing across microservices
- **Implementation**: OTLP exporters to OpenTelemetry Collector
- **Features**: Request correlation IDs, span context propagation

## Files Created/Modified

### Core Infrastructure
- `core/SharedKernel/Infrastructure/Caching/IIdentityCacheService.cs` - Interface for identity service caching
- `core/SharedKernel/Infrastructure/Caching/IWalletCacheService.cs` - Interface for wallet service caching
- `core/SharedKernel/Infrastructure/Caching/ITelemetryCacheService.cs` - Interface for telemetry service caching
- `Directory.Build.props` - NuGet package references for OpenTelemetry and Redis

### Service Updates
- `services/identity/Program.cs` - Added OpenTelemetry + Redis
- `services/wallet/Program.cs` - Added OpenTelemetry + Redis
- `services/telemetry/Program.cs` - Added OpenTelemetry + Redis
- `gateway/Program.cs` - Added OpenTelemetry
- `workers/event-processor/Program.cs` - Added OpenTelemetry
- `docker-compose.yml` - Added OpenTelemetry collector service + environment variables
- `gateway/appsettings.json` - YARP reverse proxy configuration
- `otel-collector-config.yaml` - OpenTelemetry Collector configuration

## Configuration

### Redis Configuration
```json
{
  "Redis": {
    "Enabled": true,
    "ConnectionString": "localhost:6379",
    "Prefix": "muni:"
  }
}
```

### OpenTelemetry Configuration
```json
{
  "OpenTelemetry": {
    "Enabled": true,
    "ServiceName": "{service-name}",
    "Version": "1.0.0",
    "Traces": {
      "Enabled": true,
      "Exporter": "otlp",
      "Endpoint": "http://otel-collector:4317"
    }
  }
}
```

## Usage

### Running with Redis & OpenTelemetry

```bash
# Start all services with Docker Compose
docker compose up -d --build

# Validate compose syntax
docker compose config --quiet

# View services
docker compose ps

# View logs
docker compose logs -f identity
docker compose logs -f otel-collector
```

### Redis Cache Usage

**Identity Service**:
```csharp
var cache = serviceProvider.GetRequiredService<IIdentityCacheService>();
await cache.SetJwtTokenAsync(userId, tenantId, token, TimeSpan.FromMinutes(30));
var cachedToken = await cache.GetJwtTokenAsync(userId, tenantId);
```

**Wallet Service**:
```csharp
var cache = serviceProvider.GetRequiredService<IWalletCacheService>();
await cache.SetWalletBalanceAsync(walletId, balanceJson, TimeSpan.FromMinutes(5));
```

**Telemetry Service**:
```csharp
var cache = serviceProvider.GetRequiredService<ITelemetryCacheService>();
await cache.SetActiveJourneyAsync(vehicleId, journeyJson, TimeSpan.FromMinutes(30));
```

### OpenTelemetry Tracing

Traces are automatically collected for:
- ASP.NET Core HTTP requests
- Entity Framework Core database queries
- HTTP client calls
- Custom application logs

**Correlation IDs**: Every request includes an `X-Correlation-Id` header that propagates across all services for distributed tracing.

## OpenTelemetry Collector

The collector receives traces and metrics from all services, then exports to:
- Console/logging exporter in development
- OTLP endpoint configured in `otel-collector-config.yaml`

**Access Points**:
- gRPC: `localhost:4317`
- HTTP: `localhost:4318`
- Prometheus metrics: `localhost:8889`
- Health check: `localhost:13133`

## YARP Gateway Configuration

Routes are configured in `gateway/appsettings.json`:
- `/api/identity/*` → Identity Service
- `/api/wallet/*` → Wallet Service
- `/api/telemetry/*` → Telemetry Service

`gateway/YarpRoutes.json` is a legacy/reference route file and is not loaded by the current gateway startup.

## Next Steps

1. Add Jaeger/Tempo backend and wire `otel-collector-config.yaml` to it
2. Configure Redis connection pooling
3. Add metrics dashboards for Grafana/Prometheus
4. Implement Redis-based distributed session management
5. Add health checks for Redis and OTLP endpoints

## Troubleshooting

### Redis Connection Issues
```bash
# Check Redis container is running
docker compose ps | Select-String redis

# Test connection
docker exec -it municipality-redis redis-cli ping
```

### OpenTelemetry Issues
```bash
# Check collector logs
docker compose logs -f otel-collector

# Verify endpoints are accessible
curl http://localhost:4318
```

---

*Last updated: Infrastructure setup complete. Services now support Redis caching and distributed tracing.*