namespace CuentasCorrientes.Models
{
    public class EditRole
    {
        public EditRole()
        {
            Users = [];
        }
        public string Id { get; set; }

        [Required(ErrorMessage = "Role Name is required")]
        public string RoleName { get; set; }

        public List<string> Users { get; set; }
    }
}
