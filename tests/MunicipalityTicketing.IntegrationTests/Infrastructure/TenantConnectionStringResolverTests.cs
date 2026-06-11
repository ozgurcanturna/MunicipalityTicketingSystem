using Microsoft.Extensions.Configuration;
using SharedKernel.Infrastructure.Persistence;

namespace MunicipalityTicketing.IntegrationTests.Infrastructure;

public sealed class TenantConnectionStringResolverTests
{
    [Fact]
    public void Resolve_WhenTenantSpecificConnectionExists_ShouldReturnTenantConnection()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Default-Conn",
                ["ConnectionStrings:Tenants:ankara"] = "Ankara-Conn"
            })
            .Build();

        var resolver = new TenantConnectionStringResolver(configuration);

        var result = resolver.Resolve("ankara");

        Assert.Equal("Ankara-Conn", result);
    }

    [Fact]
    public void Resolve_WhenTenantConnectionMissing_ShouldFallbackToDefault()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Default-Conn"
            })
            .Build();

        var resolver = new TenantConnectionStringResolver(configuration);

        var result = resolver.Resolve("unknown");

        Assert.Equal("Default-Conn", result);
    }
}