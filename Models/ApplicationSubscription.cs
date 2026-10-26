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

public sealed class ApplicationSubscriptionListItem
{
    public int Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? MercadoPagoId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal MonthlyAmount { get; init; }
    public string CurrencyId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
