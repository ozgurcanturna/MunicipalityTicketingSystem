using Tenant.Identity.Api.Domain.Entities;

namespace MunicipalityTicketing.UnitTests.Identity;

public sealed class MunicipalityTenantTests
{
    [Fact]
    public void Create_WhenNameIsEmpty_ShouldThrow()
    {
        var action = () => MunicipalityTenant.Create("  ");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void AddUser_WhenSameEmailAddedTwice_ShouldThrow()
    {
        var tenant = MunicipalityTenant.Create("bursa");
        tenant.AddUser("user@sample.com", "Test User", "hash-1", "ADMIN");

        var action = () => tenant.AddUser("USER@sample.com", "Another User", "hash-2", "USER");

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddUser_WhenTenantIsInactive_ShouldThrow()
    {
        var tenant = MunicipalityTenant.Create("bursa");
        tenant.Deactivate();

        var action = () => tenant.AddUser("user@sample.com", "Test User", "hash-1", "USER");

        Assert.Throws<InvalidOperationException>(action);
    }
}