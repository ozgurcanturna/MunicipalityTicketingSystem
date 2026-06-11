using Ticketing.Wallet.Api.Domain.Entities;

namespace MunicipalityTicketing.UnitTests.Wallet;

public sealed class WalletAccountTests
{
    [Fact]
    public void TopUp_ShouldIncreaseBalance_AndAddTransaction()
    {
        var wallet = WalletAccount.Create(Guid.NewGuid());

        wallet.TopUp(100m, "TOPUP-1");

        Assert.Equal(100m, wallet.Balance);
        Assert.Single(wallet.Transactions);
        Assert.Equal(WalletTransactionType.TopUp, wallet.Transactions.Single().Type);
    }

    [Fact]
    public void Spend_WhenInsufficientBalance_ShouldThrow()
    {
        var wallet = WalletAccount.Create(Guid.NewGuid());

        var action = () => wallet.Spend(10m, "SPEND-1");

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void Spend_WhenBalanceSufficient_ShouldDecreaseBalance()
    {
        var wallet = WalletAccount.Create(Guid.NewGuid());
        wallet.TopUp(50m, "TOPUP-1");

        wallet.Spend(20m, "SPEND-1");

        Assert.Equal(30m, wallet.Balance);
        Assert.Equal(2, wallet.Transactions.Count);
        Assert.Equal(WalletTransactionType.Spend, wallet.Transactions.Last().Type);
    }
}