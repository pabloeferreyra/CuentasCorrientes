namespace CuentasCorrientes.Models;

public class Movements
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public double Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string? Description { get; set; }
    public int CurrentAccountId { get; set; }
    public CurrentAccounts? CurrentAccount { get; set; }
    public required string CreatedByUserId { get; set; }
    public required string ModifiedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    [NotMapped]
    public bool Debt { get; set; }
}
