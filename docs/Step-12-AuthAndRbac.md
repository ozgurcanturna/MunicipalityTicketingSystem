# Step 12: JWT Authentication ve RBAC

## 🎯 Amaç

Bu adımda sistem, tenant-aware JWT authentication ve role-based access control (RBAC) ile korunur.

Bu adım sonunda:

- Identity servisi kullanıcı girişi için JWT token üretebilir.
- User modeli rol bilgisi taşır.
- Wallet ve Telemetry servisleri bearer token doğrular.
- Token içindeki tenant claim ile X-Tenant-Id header eşleştirilir.
- Endpointler temel rollerle korunur.

---

## ✅ Önkoşullar

- Step 00-11 tamamlanmış olmalı.

---

## 1) Domain ve Veri Modeli Güncellemeleri

### 1.1 User entity genişletmesi

Dosya: services/identity/Domain/Entities/User.cs

Eklenen alanlar:

- PasswordHash
- Role

Kurallar:

- PasswordHash bos olamaz.
- Role bos olamaz.
- Role buyuk harfe normalize edilir.

### 1.2 Tenant aggregate user ekleme akisi

Dosya: services/identity/Domain/Entities/Tenant.cs

AddUser metodu su alanlari alir:

- Email
- FullName
- PasswordHash
- Role

---

## 2) Identity Service JWT Uretimi

### 2.1 JWT konfigurasyonu

Dosyalar:

- services/identity/Infrastructure/Authentication/JwtOptions.cs
- services/identity/appsettings.json
- services/identity/appsettings.Docker.json

Alanlar:

- Issuer
- Audience
- SecretKey
- ExpirationMinutes

### 2.2 Password hasher

Dosyalar:

- services/identity/Infrastructure/Authentication/IPasswordHasher.cs
- services/identity/Infrastructure/Authentication/Sha256PasswordHasher.cs

Not:

- Bu adimda minimum calisirlik icin SHA256 tabanli hash kullanilir.
- Production hardening asamasinda salt + work factor iceren daha guclu bir algoritmaya gecilmelidir.

### 2.3 JWT token service

Dosyalar:

- services/identity/Infrastructure/Authentication/IJwtTokenService.cs
- services/identity/Infrastructure/Authentication/JwtTokenService.cs

Token claim'leri:

- sub / nameidentifier: UserId
- email
- role
- tenant_id
- tenant_name

---

## 3) Identity API Endpointleri

Dosya: services/identity/Program.cs

Eklenenler:

- AddAuthentication(JwtBearer)
- AddAuthorization
- /auth/bootstrap endpointi
- /auth/login endpointi
- /auth/me endpointi

Rol politikaları:

- TenantAdmin: ADMIN
- AuthenticatedUser: authenticated principal

Endpoint davranışı:

1. POST /auth/bootstrap

- Sistem hic tenant icermiyorsa ilk tenant + ilk ADMIN user olusturur.
- Response icinde bootstrap token dondurur.

1. POST /auth/login

- TenantId + Email + Password ile login olur.
- Basarili login durumunda JWT token doner.

1. GET /auth/me

- Token icindeki principal bilgisini dondurur.

1. POST /tenants

- ADMIN rolu gerekir.

1. POST /tenants/{id}/users

- ADMIN rolu gerekir.

1. GET /tenants/{id}

- Authenticated user gerekir.

---

## 4) Wallet ve Telemetry JWT Dogrulamasi

Dosyalar:

- services/wallet/Program.cs
- services/telemetry/Program.cs
- services/wallet/appsettings.json
- services/telemetry/appsettings.json

Davranis:

- Bearer token zorunludur.
- X-Tenant-Id header zorunludur.
- Header ile token icindeki tenant_id claim eslesmezse 403 doner.

Identity servisi icin de ayni claim-header eslestirmesi, authenticated request'lerde uygulanir.

Wallet rol politikalari:

- WalletAdmin: ADMIN
- WalletUser: ADMIN, USER

Telemetry rol politikalari:

- TelemetryOperator: ADMIN, OPERATOR
- TelemetryReader: ADMIN, OPERATOR, USER

---

## 5) Contract Degisiklikleri

Yeni contract dosyalari:

- services/identity/Application/Contracts/LoginRequest.cs
- services/identity/Application/Contracts/TokenResponse.cs
- services/identity/Application/Contracts/BootstrapTenantRequest.cs

Guncellenen contract:

- services/identity/Application/Contracts/AddUserRequest.cs

AddUserRequest yeni alanlari:

- Password
- Role

---

## 6) Doğrulama

```powershell
dotnet build MunicipalityTicketing.slnx
dotnet test MunicipalityTicketing.slnx
```

Ornek akış:

```http
POST /auth/bootstrap
Content-Type: application/json

{
  "tenantName": "ankara",
  "adminEmail": "admin@ankara.local",
  "adminFullName": "Ankara Admin",
  "adminPassword": "P@ssw0rd!"
}
```

```http
POST /tenants/{tenantId}/users
Authorization: Bearer <admin-token>
X-Tenant-Id: <tenant-guid>
Content-Type: application/json

{
  "email": "operator@ankara.local",
  "fullName": "Ankara Operator",
  "password": "P@ssw0rd!",
  "role": "OPERATOR"
}
```

```http
POST /auth/login
X-Tenant-Id: <tenant-guid>
Content-Type: application/json

{
  "tenantId": "<tenant-guid>",
  "email": "operator@ankara.local",
  "password": "P@ssw0rd!"
}
```

---

## 7) Tamamlanma Kontrol Listesi

- [x] Identity servisine JWT token uretimi eklendi.
- [x] User rol bilgisi domain modeline eklendi.
- [x] Wallet servisinde bearer auth + RBAC eklendi.
- [x] Telemetry servisinde bearer auth + RBAC eklendi.
- [x] Ilk tenant ve admin olusturmak icin bootstrap akisi eklendi.
- [x] Tenant claim ile tenant header eslestirme eklendi.
- [x] Testler role/password degisikliklerine gore guncellendi.

---

## 8) Sinirlar ve Sonraki Adimlar

Bu adim minimum calisir auth katmani sunar. Henuz tamamlanmayan production konulari:

- Refresh token akisi
- Password reset ve password policy
- BCrypt/Argon2 gibi guclu password hashing
- Key rotation ve secret management
- Gateway seviyesinde merkezi auth enforcement
- Identity endpointleri icin bootstrap admin stratejisi

---

**Durum**: ✅ MVP Tamamlandi
**Son Guncelleme**: 12.06.2026
**Yazar**: Özgür Can TURNA
