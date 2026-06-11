using Ticketing.Wallet.Api.Domain.Entities;

namespace Ticketing.Wallet.Api.Application.Contracts;

public sealed record WalletTransactionResponse(
    Guid Id,
    Guid WalletId,
    decimal Amount,
    string Type,
    string Reference,
    DateTime OccurredAt)
{
    public static WalletTransactionResponse FromDomain(WalletTransaction transaction)
    {
        return new WalletTransactionResponse(
            transaction.Id,
            transaction.WalletId,
            transaction.Amount,
            transaction.Type,
            transaction.Reference,
            transaction.OccurredAt);
    }
}