using Tenant.Identity.Api.Infrastructure.Authentication;

namespace MunicipalityTicketing.UnitTests.Identity;

public sealed class BCryptPasswordHasherTests
{
    [Fact]
    public void Hash_And_Verify_WithSamePassword_ShouldReturnTrue()
    {
        var hasher = new BCryptPasswordHasher();

        var hash = hasher.Hash("P@ssw0rd!");

        Assert.True(hasher.Verify("P@ssw0rd!", hash));
    }

    [Fact]
    public void Verify_WithDifferentPassword_ShouldReturnFalse()
    {
        var hasher = new BCryptPasswordHasher();
        var hash = hasher.Hash("P@ssw0rd!");

        Assert.False(hasher.Verify("wrong-password", hash));
    }
}