using StackExchange.Redis;

namespace SharedKernel.Infrastructure.Caching;

public interface IWalletCacheService
{
    Task<bool> SetWalletCacheAsync(Guid walletId, string tenantId, CancellationToken cancellationToken);

    Task<WalletCache?> GetWalletCacheAsync(Guid walletId, string tenantId, CancellationToken cancellationToken);

    Task InvalidateWalletCacheAsync(Guid walletId, string tenantId, CancellationToken cancellationToken);

    Task<bool> SetBalanceCacheAsync(string tenantId, decimal balance, CancellationToken cancellationToken);

    Task<decimal?> GetBalanceCacheAsync(string tenantId, CancellationToken cancellationToken);

    Task InvalidateBalanceCacheAsync(string tenantId, CancellationToken cancellationToken);
}
