namespace CuentasCorrientes.Controllers;

[Authorize(Roles = "SuperUser,Admin,User")]
public class ClientController(LoggerService loggerService,
    IGetClientService get,
    ICreateClientService create,
    IUpdateClientService update,
    IDeleteClientService delete,
    IGetClientTypeService getClientType) : Controller
{
    public async Task<ActionResult> Index(string sortOrder)
    {
        List<ClientType> clientTypes = await getClientType.GetAllClientTypes();
        ViewBag.ClientTypes = clientTypes.Select(ct => new SelectListItem
        {
            Value = ct.Id.ToString(),
            Text = ct.Name
        }).ToList();

        loggerService.Log("Client Index action called.");
        List<Client> clients = await get.GetAllClients();
        clients = sortOrder switch
        {
            "Name" => [.. clients.OrderBy(c => c.Name)],
            "Name_desc" => [.. clients.OrderByDescending(c => c.Name)],
            "Surname" => [.. clients.OrderBy(c => c.Surname)],
            "Surname_desc" => [.. clients.OrderByDescending(c => c.Surname)],
            "Type" => [.. clients.OrderBy(c => c.ClientType?.Name)],
            "Type_desc" => [.. clients.OrderByDescending(c => c.ClientType?.Name)],
            _ => [.. clients.OrderBy(c => c.Name)]
        };
        ViewBag.SortOrder = sortOrder;
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            return PartialView("_ClientTable", clients);
        return View(clients);
    }

    public async Task<ActionResult> Details(int id)
    {
        loggerService.Log($"Client Details action called for ID: {id}");
        Client client = await get.GetClient(id);
        if (client == null)
        {
            loggerService.Log($"Client with ID {id} not found.");
            return NotFound();
        }
        return View(client);
    }

    
    [HttpPost]
    public async Task<ActionResult> Create(Client client)
    {
        loggerService.Log("Client Create POST action called.");
        if (ModelState.IsValid)
        {
            try
            {
                await create.CreateClient(client);
                loggerService.Log($"Client created with ID: {client.Id}");
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                loggerService.Log($"SQL Error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while creating the client. Please try again.");
            }
        }
        return View(client);
    }
    public async Task<ActionResult> Edit(int id)
    {
        loggerService.Log($"Client Edit action called for ID: {id}");
        Client client = await get.GetClient(id);

        List<ClientType> clientTypes = await getClientType.GetAllClientTypes();
        ViewBag.ClientTypes = clientTypes.Select(ct => new SelectListItem
        {
            Value = ct.Id.ToString(),
            Text = ct.Name
        }).ToList();

        if (clientTypes == null || !clientTypes.Any())
        {
            loggerService.Log("No client types found.");
            ModelState.AddModelError("", "No client types available. Please create a client type first.");
            return RedirectToAction(nameof(Index));
        }

        if (client == null)
        {
            loggerService.Log($"Client with ID {id} not found.");
            return NotFound();
        }
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            return PartialView("_Edit", client);
        return View(client);
    }
    [HttpPost]
    public async Task<ActionResult> Edit(Client client)
    {
        loggerService.Log("Client Edit POST action called.");
        if (ModelState.IsValid)
        {
            try
            {
                await update.UpdateClient(client);
                loggerService.Log($"Client updated with ID: {client.Id}");
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                loggerService.Log($"SQL Error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while updating the client. Please try again.");
            }
        }
        return View(client);
    }

    public async Task<ActionResult> Delete(int id)
    {
        loggerService.Log($"Client Delete action called for ID: {id}");
        Client client = await get.GetClient(id);
        if (client == null)
        {
            loggerService.Log($"Client with ID {id} not found.");
            return NotFound();
        }
        return View(client);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<ActionResult> DeleteConfirmed(int id)
    {
        loggerService.Log($"Client DeleteConfirmed action called for ID: {id}");
        var client = await get.GetClient(id);
        if (client == null)
        {
            loggerService.Log($"Client with ID {id} not found.");
            return NotFound();
        }
        try
        {
            delete.DeleteClient(client);
            loggerService.Log($"Client deleted with ID: {id}");
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            loggerService.Log($"SQL Error: {ex.Message}");
            ModelState.AddModelError("", "An error occurred while deleting the client. Please try again.");
            return RedirectToAction(nameof(Index));
        }
    }

}