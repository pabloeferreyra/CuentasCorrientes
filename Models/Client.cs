using System.ComponentModel.DataAnnotations.Schema;

namespace CuentasCorrientes.Models;

public class Client
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   
    public int Id { get; set; }
    public string? Surname { get; set; }
    public string? Name { get; set; }
    public string? Cuit { get; set; }
    public string? Address { get; set; }
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public int ClientTypeId { get; set; }
    public ClientType? ClientType { get; set; }
    public ICollection<CurrentAccounts>? CurrentAccounts { get; set; }
}
