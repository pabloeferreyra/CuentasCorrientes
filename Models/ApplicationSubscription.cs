namespace CuentasCorrientes.Models;

public class ApplicationSubscription
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ExternalReference { get; set; } = string.Empty;
    public string? MercadoPagoId { get; set; }
    public string Status { get; set; } = "creating";
    public decimal MonthlyAmount { get; set; }
    public string CurrencyId { get; set; } = "ARS";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
