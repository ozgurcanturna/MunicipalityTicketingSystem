namespace SharedKernel.Infrastructure.MultiTenancy;

public interface ITenantProvider
{
    string? TenantId { get; }
}

public sealed class NullTenantProvider : ITenantProvider
{
    public string? TenantId => null;
}