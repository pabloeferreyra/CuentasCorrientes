namespace CuentasCorrientes.Controllers;

[Authorize(Roles = "SuperUser")]
public class AdminController(LoggerService logger, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> ManageUserClaims(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID cannot be null or empty.");
        }
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"User with ID {userId} not found.");
        }

        var existingClaims = await userManager.GetClaimsAsync(user);
        if (existingClaims == null)
        {
            return NotFound($"No claims found for user with ID {userId}.");
        }
        var model = new UserClaims
        {
            UserId = user.Id
        };

        var allClaims = new List<IEnumerable<Claim>> {
            ClaimsStore.RoleCLaims,
            ClaimsStore.UserClaims,
            ClaimsStore.MovementClaims,
            ClaimsStore.ClientClaims,
            ClaimsStore.AccountClaims
        };

        model.Claims = [.. allClaims
        .SelectMany(claimList => claimList)
        .Select(claim => new UserClaim
        {
            ClaimType = claim.Type,
            IsSelected = existingClaims.Any(c => c.Type == claim.Type)
        })];

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ManageUserClaims(UserClaims model)
    {
        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found";
            return View("NotFound");
        }

        var claims = await userManager.GetClaimsAsync(user);
        var result = await userManager.RemoveClaimsAsync(user, claims);

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Cannot remove user existing claims");
            return View(model);
        }

        result = await userManager.AddClaimsAsync(user, model.Claims.Where(c => c.IsSelected).Select(c => new Claim(c.ClaimType, c.ClaimType)));
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Cannot add selected claims to user");
            return View(model);
        }
        return RedirectToAction(nameof(EditUser), new { id = model.UserId });
    }

    [HttpGet]
    public IActionResult ListUsers()
    {
        var users = userManager.Users;
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
            return View("NotFound");
        }

        var userClaims = await userManager.GetClaimsAsync(user);
        var userRoles = await userManager.GetRolesAsync(user);

        var model = new EditUser
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            Claims = userClaims.Select(c => c.Value).ToList(),
            Roles = userRoles
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(EditUser model)
    {
        var user = await userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
            return View("NotFound");
        }
        else
        {
            try
            {
                user.Email = model.Email;
                user.UserName = model.UserName;

                var result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(ListUsers));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
            catch (DbUpdateException ex)
            {
                await logger.LogAsync($"Error updating user {ex}");
                return View("Error");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await userManager.FindByIdAsync(id);

        if (user == null)
        {
            ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";

            return View("NotFound");
        }
        else
        {
            try
            {
                var result = await userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(ListUsers));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("ListUsers");
            }
            catch (DbUpdateException ex)
            {
                logger.Log($"Error deleting user {ex}");
                return View("Error");
            }
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> DeleteRole(string id)
    {
        var role = await roleManager.FindByIdAsync(id);

        if (role == null)
        {
            ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";

            return View("NotFound");
        }
        else
        {
            try
            {
                var result = await roleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(ListRoles));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(nameof(ListRoles));
            }
            catch (DbUpdateException ex)
            {
                logger.Log($"Error deleting role {ex}");
                ViewBag.ErrorTitle = $"{role.Name} role is in use";
                ViewBag.ErrorMessage = $"{role.Name} role cannot be deleted as there are users " +
                    $"in this role. If you want to delete this role, please remove the users from" +
                    $"the role and then try to delete";
                return View("Error");
            }
        }
    }

    [HttpGet]
    public IActionResult ListRoles()
    {
        var userRoles = roleManager.Roles;
        return View(userRoles);
    }

    [HttpGet]
    public IActionResult CreateRole()
    {
        return View(new IdentityRole());
    }

    [HttpGet]
    public async Task<IActionResult> EditRole(string id)
    {
        var role = await roleManager.FindByIdAsync(id);

        if (role == null)
        {
            ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
            return View("NotFound");
        }

        var model = new EditRole
        {
            Id = role.Id,
            RoleName = role.Name
        };
        var users = userManager.Users.ToList();
        foreach (var user in users)
        {
            if (await userManager.IsInRoleAsync(user, role.Name))
            {
                model.Users.Add(user.UserName);
            }
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditRole(EditRole model)
    {
        var role = await roleManager.FindByIdAsync(model.Id);

        if (role == null)
        {
            ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
            return View("NotFound");
        }
        else
        {
            try
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(ListRoles));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);

            }
            catch (DbUpdateException ex)
            {
                logger.Log($"Error editing role {ex}");
                return View("Error");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(IdentityRole role)
    {
        await roleManager.CreateAsync(role);
        return RedirectToAction(nameof(ListRoles));
    }

    [HttpGet]
    public async Task<IActionResult> EditUsersInRole(string roleId)
    {
        ViewBag.roleId = roleId;

        var role = await roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
            return View("NotFound");
        }

        var model = new List<UserRole>();

        var users = userManager.Users.ToList();
        foreach (var user in users)
        {
            var userRoleViewModel = new UserRole
            {
                UserId = user.Id,
                UserName = user.UserName
            };
            if (await userManager.IsInRoleAsync(user, role.Name))
            {
                userRoleViewModel.IsSelected = true;
            }
            else
            {
                userRoleViewModel.IsSelected = false;
            }

            model.Add(userRoleViewModel);
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditUsersInRole(List<UserRole> model, string roleId)
    {
        var role = await roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
            return View("NotFound");
        }
        foreach (var userRole in model)
        {
            var user = await userManager.FindByIdAsync(userRole.UserId);

            if (userRole.IsSelected && !await userManager.IsInRoleAsync(user, role.Name))
            {
                await userManager.AddToRoleAsync(user, role.Name);
            }
            else if (!userRole.IsSelected && await userManager.IsInRoleAsync(user, role.Name))
            {
                await userManager.RemoveFromRoleAsync(user, role.Name);
            }
        }
        return RedirectToAction(nameof(EditRole), new { Id = roleId });
    }
}
