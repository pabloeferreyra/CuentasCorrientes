﻿namespace CuentasCorrientes.Models;

public class EditUser
{
    public EditUser()
    {
        Claims = []; Roles = [];
    }

    public string Id { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public List<string> Claims { get; set; }
    public IList<string> Roles { get; set; }
}
