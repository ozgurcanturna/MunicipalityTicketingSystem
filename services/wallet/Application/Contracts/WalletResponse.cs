using Ticketing.Wallet.Api.Domain.Entities;

namespace Ticketing.Wallet.Api.Application.Contracts;

public sealed record WalletResponse(
    Guid Id,
    Guid TenantId,
    decimal Balance,
    bool IsActive,
    IReadOnlyCollection<WalletTransactionResponse> Transactions)
{
    public static WalletResponse FromDomain(WalletAccount wallet)
    {
        return new WalletResponse(
            wallet.Id,
            wallet.TenantId,
            wallet.Balance,
            wallet.IsActive,
            wallet.Transactions.Select(WalletTransactionResponse.FromDomain).ToArray());
    }
}