using SharedKernel.Domain.Repositories;
using Ticketing.Wallet.Api.Domain.Entities;

namespace Ticketing.Wallet.Api.Application.Repositories;

public interface IWalletRepository : IRepository<WalletAccount>
{
    Task<WalletAccount?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}