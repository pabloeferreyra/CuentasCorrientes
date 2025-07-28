namespace CuentasCorrientes.Models;

public class ClientType
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Display(Name = "Nombre")]
    public string? Name { get; set; }
    [Display(Name = "Descripcion")]
    public string? Description { get; set; }
    public ICollection<Client>? Clients { get; set; }
}
