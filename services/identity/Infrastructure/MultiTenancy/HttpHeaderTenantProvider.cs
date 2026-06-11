using SharedKernel.Infrastructure.MultiTenancy;

namespace Tenant.Identity.Api.Infrastructure.MultiTenancy;

public sealed class HttpHeaderTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpHeaderTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? TenantId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                return null;
            }

            if (!httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
            {
                return null;
            }

            var tenantId = tenantIdValues.FirstOrDefault();
            return string.IsNullOrWhiteSpace(tenantId) ? null : tenantId.Trim();
        }
    }
}