namespace Tenant.Identity.Api.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "MunicipalityTicketing.Identity";
    public string Audience { get; set; } = "MunicipalityTicketing.Clients";
    public string SecretKey { get; set; } = "super-secret-development-key-change-me-12345";
    public int ExpirationMinutes { get; set; } = 60;
}