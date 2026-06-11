using SharedKernel.Domain.Entities;

namespace Ticketing.Wallet.Api.Domain.Entities;

public sealed class WalletTransaction : Entity
{
    public Guid WalletId { get; private set; }
    public decimal Amount { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Reference { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }

    private WalletTransaction()
    {
    }

    private WalletTransaction(Guid walletId, decimal amount, string type, string reference)
    {
        WalletId = walletId;
        Amount = amount;
        Type = type;
        Reference = reference;
        OccurredAt = DateTime.UtcNow;
    }

    internal static WalletTransaction Create(Guid walletId, decimal amount, string type, string reference)
    {
        return new WalletTransaction(walletId, amount, type, reference);
    }
}