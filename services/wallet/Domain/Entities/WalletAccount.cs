using SharedKernel.Domain.Entities;

namespace Ticketing.Wallet.Api.Domain.Entities;

public sealed class WalletAccount : AggregateRoot
{
    private readonly List<WalletTransaction> _transactions = [];

    public Guid TenantId { get; private set; }
    public decimal Balance { get; private set; }
    public bool IsActive { get; private set; } = true;
    public IReadOnlyCollection<WalletTransaction> Transactions => _transactions.AsReadOnly();

    private WalletAccount()
    {
    }

    private WalletAccount(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId zorunludur.", nameof(tenantId));
        }

        TenantId = tenantId;
        Balance = 0m;
        RegisterCreated();
    }

    public static WalletAccount Create(Guid tenantId)
    {
        return new WalletAccount(tenantId);
    }

    public void TopUp(decimal amount, string reference)
    {
        ValidateAmount(amount);
        ValidateReference(reference);

        Balance += amount;
        _transactions.Add(WalletTransaction.Create(Id, amount, WalletTransactionType.TopUp, reference));
        RegisterUpdated();
    }

    public void Spend(decimal amount, string reference)
    {
        ValidateAmount(amount);
        ValidateReference(reference);

        if (Balance < amount)
        {
            throw new InvalidOperationException("Yetersiz bakiye.");
        }

        Balance -= amount;
        _transactions.Add(WalletTransaction.Create(Id, amount, WalletTransactionType.Spend, reference));
        RegisterUpdated();
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Tutar sıfırdan büyük olmalıdır.", nameof(amount));
        }
    }

    private static void ValidateReference(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException("Reference zorunludur.", nameof(reference));
        }
    }
}