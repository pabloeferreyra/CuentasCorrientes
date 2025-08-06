using System.Security.Claims;

namespace CuentasCorrientes.Models;

public static class ClaimsStore
{
    public static readonly List<Claim> RoleCLaims =
    [
        new Claim("Create Role", "Create Role"),
        new Claim("Edit Role", "Edit Role"),
        new Claim("Delete Role", "Delete Role"),
        new Claim("View Role", "View Role"),
    ];

    public static readonly List<Claim> UserClaims = 
    [
        new Claim("Create User", "Create User"),
        new Claim("Edit User", "Edit User"),
        new Claim("Delete User", "Delete User"),
        new Claim("View User", "View User")
    ];
    public static readonly List<Claim> MovementClaims = 
    [
        new Claim("Create Movement", "Create Movement"),
        new Claim("Edit Movement", "Edit Movement"),
        new Claim("Delete Movement", "Delete Movement"),
        new Claim("View Movement", "View Movement")
    ];
    public static readonly List<Claim> ClientClaims = 
    [
        new Claim("Create Client", "Create Client"),
        new Claim("Edit Client", "Edit Client"),
        new Claim("Delete Client", "Delete Client"),
        new Claim("View Client", "View Client")
    ];
    public static readonly List<Claim> AccountClaims = 
    [
        new Claim("Create Account", "Create Account"),
        new Claim("Edit Account", "Edit Account"),
        new Claim("Delete Account", "Delete Account"),
        new Claim("View Account", "View Account")
    ];
}