# Step 11: Docker Deployment ve Event Bus Best Practices

## Hedef

Bu adimda tum mikroservisler lokal Docker Desktop ortaminda tek komutla ayağa kalkacak sekilde paketlenir. Ayrica event bus yaklasimi icin production'a yakin best-practice konfigurasyon ciplagi eklenir.

Bu adim sonunda:

- Her servis/worker/simulator icin Dockerfile vardir.
- Kok dizinde docker-compose.yml ile tum sistem calisir.
- PostgreSQL, Redis ve RabbitMQ altyapisi compose icindedir.
- Worker tarafinda Brighter & Darker stack ve RabbitMQ transport konfigurasyonu ayri options sinifi ile okunur.

## Eklenen Dosyalar

1. docker-compose.yml
2. .dockerignore
3. gateway/Dockerfile
4. services/identity/Dockerfile
5. services/wallet/Dockerfile
6. services/telemetry/Dockerfile
7. workers/event-processor/Dockerfile
8. tools/simulator/Dockerfile
9. gateway/appsettings.Docker.json
10. services/identity/appsettings.Docker.json
11. services/wallet/appsettings.Docker.json
12. services/telemetry/appsettings.Docker.json
13. workers/event-processor/appsettings.Docker.json
14. workers/event-processor/Configuration/EventBusOptions.cs

## Guncellenen Kodlar

1. workers/event-processor/Program.cs
   - EventBus section icin options binding eklendi.
2. workers/event-processor/Worker.cs
   - Event bus stack/transport/exchange/queue/dlq bilgileri startup log'una eklendi.

## Compose Topolojisi

- sqlserver (mssql 2022)
- redis
- rabbitmq (management UI dahil)
- identity
- wallet
- telemetry
- event-processor
- gateway
- simulator (opsiyonel, profile: simulation)

## Event Bus Best Practices (Bu Adimda Uygulanan + Yol Haritasi)

### Uygulananlar

1. Stack/transport abstraction konfigurasyonu:
   - EventBus:Stack alani ile BrighterDarker secimi
   - EventBus:Transport alani ile InMemory veya RabbitMq secimi
2. DLQ isimlendirmesi ve ayri queue tanimi:
   - DeadLetterQueueName
3. Retry politikasinin configurable olmasi:
   - EventProcessor:MaxRetryCount, BaseRetryDelayMs
4. Prefetch ayari:
   - EventBus:PrefetchCount
5. Gozlemlenebilirlik:
   - Worker startup'ta stack/transport/exchange/queue/dlq bilgilerini loglar.

### Sonraki Teknik Adimlar (Production Harden)

1. Outbox pattern (publisher servislerde)
2. Inbox/idempotent consumer kaydinin kalici depoya tasinmasi
3. RabbitMQ publisher confirm + mandatory flag
4. Exponential backoff + jitter
5. DLQ retry/parking-lot stratejisi
6. Mesaj schema versioning ve contract testleri
7. OpenTelemetry trace propagation (correlation id + baggage)

## Calistirma

Tum sistemi baslat:

```powershell
docker compose up -d --build
```

Durum kontrol:

```powershell
docker compose ps
```

Gateway health:

```powershell
curl http://localhost:5197/health
```

Simulator profile ile yuk testi:

```powershell
docker compose --profile simulation up --build simulator
```

Kapatma:

```powershell
docker compose down
```

## Ogrenme Notu

Docker ortaminda appsettings.Docker.json aktif olur cunku compose icinde ASPNETCORE_ENVIRONMENT=Docker (worker icin DOTNET_ENVIRONMENT=Docker) set edilmistir. Bu sayede Brighter & Darker stack, RabbitMQ transport ile calisir.

## Tamamlanma Kontrol Listesi

- [x] Dockerfile'lar eklendi.
- [x] docker-compose.yml eklendi.
- [x] Altyapi container'lari eklendi (postgres, redis, rabbitmq).
- [x] Docker appsettings override'lari eklendi.
- [x] Event bus options sinifi ve binding eklendi.
- [x] Event bus best-practice notlari dokumante edildi.

Durum: Tamamlandi
