namespace CuentasCorrientes.Controllers;

[Authorize(Roles = "Administrador, Usuario, SuperUser")]
public class CurrentAccountController(LoggerService loggerService,
    IGetCurrentAccountService get,
    ICreateCurrentAccountService create,
    IUpdateCurrentAccountService update,
    IDeleteCurrentAccountService delete,
    IGetClientService getClient) : Controller
{

    public async Task<ActionResult> Index(string sortOrder)
    {
        loggerService.Log("CurrentAccount Index action called.");
        List<Client> clients = await getClient.GetAllClients();
        ViewBag.Clients = clients.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        }).ToList();

        List<CurrentAccounts> currentAccounts = await get.GetAllCurrentAccounts();
        currentAccounts = sortOrder switch
        {
            "Client" => [.. currentAccounts.OrderBy(ca => ca.Client.Name)],
            "Client_desc" => [.. currentAccounts.OrderByDescending(ca => ca.Client.Name)],
            _ => [.. currentAccounts.OrderBy(ca => ca.Id)]
        };
        ViewBag.SortOrder = sortOrder;
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            return PartialView("_CurrentAccountTable", currentAccounts);
        return View(currentAccounts);
    }

    public async Task<ActionResult> Details(int id)
    {
        loggerService.Log($"CurrentAccount Details action called for ID: {id}");
        CurrentAccounts currentAccount = await get.GetCurrentAccountById(id);
        if (currentAccount == null)
        {
            loggerService.Log($"CurrentAccount with ID {id} not found.");
            return NotFound();
        }
        return View(currentAccount);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CurrentAccounts currentAccount)
    {
        loggerService.Log("CurrentAccount Create POST action called.");
            if (ModelState.IsValid)
            {
                try
                {
                    await create.CreateCurrentAccount(currentAccount);
                    loggerService.Log($"CurrentAccount created successfully for Client ID: {currentAccount.ClientId}");
                    return Ok();
                }
                catch (Exception ex)
                {
                    loggerService.Log($"Error creating CurrentAccount: {ex.Message}");
                    ModelState.AddModelError("", "Error creating CurrentAccount. Please try again.");
                }
            }
        return View(currentAccount);
    }

    [HttpGet]
    public async Task<ActionResult> Edit(int id)
    {
        loggerService.Log($"CurrentAccount Edit action called for ID: {id}");
        CurrentAccounts currentAccount = await get.GetCurrentAccountById(id);
        if (currentAccount == null)
        {
            loggerService.Log($"CurrentAccount with ID {id} not found.");
            return NotFound();
        }
        return View(currentAccount);
    }

    [HttpPost]
    public async Task<ActionResult> Edit(CurrentAccounts currentAccount)
    {
        loggerService.Log("CurrentAccount Edit POST action called.");
        if (ModelState.IsValid)
        {
            try
            {
                await update.UpdateCurrentAccount(currentAccount);
                loggerService.Log($"CurrentAccount updated successfully for Client ID: {currentAccount.ClientId}");
                return Ok();
            }
            catch (Exception ex)
            {
                loggerService.Log($"Error updating CurrentAccount: {ex.Message}");
                ModelState.AddModelError("", "Error updating CurrentAccount. Please try again.");
            }
        }
        return View(currentAccount);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        // Lógica para eliminar la cuenta corriente por id
        var currentAccount = await get.GetCurrentAccountById(id);
        if (currentAccount == null)
        {
            return NotFound();
        }

        delete.DeleteCurrentAccount(currentAccount);
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult> GetCurrentAccountByClientId(int clientId)
    {
        loggerService.Log($"GetCurrentAccountByClientId action called for Client ID: {clientId}");
        CurrentAccounts? currentAccount = await get.GetCurrentAccountByClientId(clientId);
        if (currentAccount == null)
        {
            loggerService.Log($"No CurrentAccount found for Client ID {clientId}.");
            return NotFound();
        }
        return PartialView("~/Views/Shared/CurrentAccount/_CurrentAccountTable.cshtml", currentAccount);
    }

    [HttpGet]
    public async Task<ActionResult> GetCurrentAccountsByClientId(int clientId)
    {
        loggerService.Log($"GetCurrentAccountsByClientId action called for Client ID: {clientId}");
        List<CurrentAccounts> currentAccounts = await get.GetCurrentAccountsByClientId(clientId);
        if (currentAccounts == null || !currentAccounts.Any())
        {
            loggerService.Log($"No CurrentAccounts found for Client ID {clientId}.");
            return NotFound();
        }
        return View(currentAccounts);
    }
}
