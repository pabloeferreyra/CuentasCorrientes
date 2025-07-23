using System.ComponentModel.DataAnnotations.Schema;

namespace CuentasCorrientes.Models;

public class ClientType
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ICollection<Client>? Clients { get; set; }
}
