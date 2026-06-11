# Step 10: Simulation Clients - Gateway Uzerinden Yuk Testi

## Hedef
Bu adimda simulator araci Hello World seviyesinden cikartilip, gateway uzerinden tenant bazli trafik ureten bir yuk senaryosuna donusturulur.

Bu adim sonunda:
- Simulator birden fazla concurrent worker ile istek atar.
- X-Tenant-Id header ile multi-tenant akis test edilir.
- Basari/basarisizlik ve yaklasik RPS metriği ciktilanir.

## Onkosullar
- Step 00-09 tamamlanmis olmali.
- Gateway ve servis endpoint'leri calisir durumda olmali.

## Yapilan Kod Degisikligi
Dosya: tools/simulator/Program.cs

Uygulanan noktalar:
1. Ortam degiskenleri ile konfigurasyon:
   - SIMULATOR_BASE_URL (varsayilan: http://localhost:5197)
   - SIMULATOR_TENANT_ID (varsayilan: ankara)
   - SIMULATOR_DURATION_SECONDS (varsayilan: 30)
   - SIMULATOR_CONCURRENCY (varsayilan: 8)
   - SIMULATOR_DELAY_MS (varsayilan: 100)
2. Her worker dongusu icinde iki endpoint kontrolu:
   - /health
   - /api/telemetry/journeys/active/{busCode}
3. Telemetry endpoint'i icin X-Tenant-Id header gonderimi.
4. Sonuc ozet ciktilari:
   - Total Requests
   - Success
   - Failure
   - Approx RPS

## Neden Bu Tasarim?
1. Baslangic seviyesi icin kolay: tek dosya, minimum bagimlilik.
2. Gateway odakli: tum trafik tek giris noktasindan gectigi icin gercek hayata daha yakin.
3. Multi-tenant dogrulamasi: tenant header'inin her istekte tasinmasi test edilir.

## Calistirma
Lokal:
```powershell
dotnet run --project tools/simulator/MunicipalityTicketing.Simulator.csproj
```

Ortam degiskenleri ile:
```powershell
$env:SIMULATOR_BASE_URL="http://localhost:5197"
$env:SIMULATOR_TENANT_ID="istanbul"
$env:SIMULATOR_DURATION_SECONDS="60"
$env:SIMULATOR_CONCURRENCY="16"
$env:SIMULATOR_DELAY_MS="50"
dotnet run --project tools/simulator/MunicipalityTicketing.Simulator.csproj
```

## Tamamlanma Kontrol Listesi
- [x] Hello World yerine senaryo tabanli simulator yazildi.
- [x] Tenant header ile cagrilar eklendi.
- [x] Concurrency ve sure konfigurable hale getirildi.
- [x] Ozet metrik ciktilari eklendi.

## Step 11'e Gecis Notu
Bir sonraki adimda bu simulator konteyner olarak docker-compose icinde profile ile calistirilacaktir.

Durum: Tamamlandi
