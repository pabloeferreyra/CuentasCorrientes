namespace CuentasCorrientes.Models;

public class UserClaims
{
    public UserClaims()
    {
        Claims = [];
    }
    public string UserId { get; set; }
    public List<UserClaim> Claims { get; set; }
}
public class UserClaim
{
    public string? ClaimType { get; set; }
    public bool IsSelected { get; set; }
}