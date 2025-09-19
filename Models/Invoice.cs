namespace CuentasCorrientes.Models
{
    public class Invoice
    {
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Cuit { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public int Id { get; set; }
    }
}
