namespace Tenant.Identity.Api.Application.Contracts;

public sealed record AddUserRequest(string Email, string FullName, string Password, string Role);