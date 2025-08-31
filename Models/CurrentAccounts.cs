namespace CuentasCorrientes.Models;

public class CurrentAccounts
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [DisplayName("Saldo")]
    public double Debt { get; set; } = 0;
    [DisplayName("Fecha Cuenta")]
    public DateOnly Date { get; set; }
    [DisplayName("Descripcion")]
    public string? Description { get; set; }
    public int ClientId { get; set; }  
    public Client? Client { get; set; }
    public ICollection<Movements>? Movements { get; set; }
}
