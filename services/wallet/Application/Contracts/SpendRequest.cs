namespace Ticketing.Wallet.Api.Application.Contracts;

public sealed record SpendRequest(decimal Amount, string Reference);