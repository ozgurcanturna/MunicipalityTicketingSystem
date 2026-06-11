using Tenant.Identity.Api.Domain.Entities;

namespace MunicipalityTicketing.UnitTests.Identity;

public sealed class UserTests
{
    [Fact]
    public void Create_WhenRoleMissing_ShouldThrow()
    {
        var action = () => User.Create(Guid.NewGuid(), "user@sample.com", "Test User", "hash", " ");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_WhenRoleProvided_ShouldNormalizeRole()
    {
        var user = User.Create(Guid.NewGuid(), "user@sample.com", "Test User", "hash", "admin");

        Assert.Equal("ADMIN", user.Role);
    }
}