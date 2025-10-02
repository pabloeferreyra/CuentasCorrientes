namespace CuentasCorrientes.Models;

public class Balance
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }

    // Datos del cliente
    public string Surname { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Cuit { get; set; } = string.Empty;
}
