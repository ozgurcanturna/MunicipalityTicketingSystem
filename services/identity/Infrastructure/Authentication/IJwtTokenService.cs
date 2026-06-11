using Tenant.Identity.Api.Application.Contracts;
using Tenant.Identity.Api.Domain.Entities;

namespace Tenant.Identity.Api.Infrastructure.Authentication;

public interface IJwtTokenService
{
    TokenResponse CreateToken(MunicipalityTenant tenant, User user);
}