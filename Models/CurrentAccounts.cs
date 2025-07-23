using System.ComponentModel.DataAnnotations.Schema;

namespace CuentasCorrientes.Models;

public class CurrentAccounts
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public double Debt { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string? Description { get; set; }
    public int ClientId { get; set; }  
    public Client? Client { get; set; }
    public ICollection<Movements>? Movements { get; set; }
}
