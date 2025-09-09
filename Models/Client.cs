namespace CuentasCorrientes.Models;

public class Client
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   
    public int Id { get; set; }
    [DisplayName("Apellido")]
    public string? Surname { get; set; }
    [DisplayName("Nombre")]
    public string? Name { get; set; }
    [UIHint("Cuit")]
    public string? Cuit { get; set; }
    [DisplayName("Direccion")]
    public string? Address { get; set; }
    [DisplayName("Identificador")]
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
    [DisplayName("Telefono")]
    public string? Telephone { get; set; }
    public int ClientTypeId { get; set; }
    [DisplayName("Tipo")]
    public ClientType? ClientType { get; set; }
    public ICollection<CurrentAccounts>? CurrentAccounts { get; set; }
    [DisplayName("Balance")]
    [NotMapped]
    public double Account { get; set; }
}
