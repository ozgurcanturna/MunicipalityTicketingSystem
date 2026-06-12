namespace Tenant.Identity.Api.Domain.Constants;

public static class IdentityRoles
{
    public const string Admin = "ADMIN";
    public const string Operator = "OPERATOR";
    public const string User = "USER";

    public static bool IsSupported(string role)
    {
        return role is Admin or Operator or User;
    }
}