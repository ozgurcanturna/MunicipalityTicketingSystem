using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Repositories;
using Ticketing.Wallet.Api.Application.Repositories;
using Ticketing.Wallet.Api.Domain.Entities;
using Ticketing.Wallet.Api.Infrastructure.Persistence;

namespace Ticketing.Wallet.Api.Infrastructure.Repositories;

public sealed class WalletRepository : Repository<WalletAccount>, IWalletRepository
{
    public WalletRepository(WalletDbContext dbContext)
        : base(dbContext)
    {
    }

    private WalletDbContext WalletDbContext => (WalletDbContext)DbContext;

    public new Task<WalletAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return WalletDbContext.Wallets
            .Include(wallet => wallet.Transactions)
            .FirstOrDefaultAsync(wallet => wallet.Id == id, cancellationToken);
    }

    public Task<WalletAccount?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return WalletDbContext.Wallets
            .Include(wallet => wallet.Transactions)
            .FirstOrDefaultAsync(wallet => wallet.TenantId == tenantId, cancellationToken);
    }
}