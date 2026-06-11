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