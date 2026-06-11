namespace Ticketing.Wallet.Api.Application.Contracts;

public sealed record TopUpWalletRequest(decimal Amount, string Reference);