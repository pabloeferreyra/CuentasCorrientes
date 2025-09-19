using CuentasCorrientes.Models;

namespace CuentasCorrientes.Controllers;

[Authorize(Roles = "Usuario, Administrador")]
public class MovementsController(LoggerService loggerService,
    IGetMovementService get,
    ICreateMovementService create,
    IUpdateMovementService update,
    IDeleteMovementService delete, 
    IGetClientService getClient) : Controller
{
    public async Task<ActionResult> Index(int id, string sortOrder)
        {
        loggerService.Log($"Movements Index action called for Account ID: {id}");
        Client client = await getClient.GetClientByCurrentAccountId(id);
        List<Movements> movements = await get.GetMovementsByCurrentAccountId(id);
        movements = sortOrder switch
        {
            "Date" => [.. movements.OrderBy(m => m.Date)],
            "Date_desc" => [.. movements.OrderByDescending(m => m.Date)],
            _ => [.. movements.OrderBy(m => m.Date)]
        };
        ViewBag.SortOrder = sortOrder;
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            return PartialView("_MovementTable", movements);
        ViewBag.CurrentAccountId = id;
        ViewBag.ClientName = client.ToString();
        return View(movements);
    }
    
    public async Task<ActionResult> Details(int id)
    {
        loggerService.Log($"Movements Details action called for ID: {id}");
        Movements? movement = await get.GetMovementById(id);
        if (movement == null)
        {
            loggerService.Log($"Movement with ID {id} not found.");
            return NotFound();
        }
        return View(movement);
    }

    [HttpPost]
    public async Task<ActionResult> Create(Movements movement)
    {
        loggerService.Log("Movements Create POST action called.");
        if (ModelState.IsValid)
        {
            try
            {
                await create.CreateMovement(movement);
                loggerService.Log($"Movement created successfully for Account ID: {movement.CurrentAccountId}");
                var client = await getClient.GetClientByCurrentAccountId(movement.CurrentAccountId);
                
                // Si es una petición AJAX, devolver la URL de redirección
                Invoice invoice = new()
                {
                    Name = client.Name,
                    LastName = client.Surname,
                    Cuit = client.Cuit,
                    Date = movement.Date,
                    Description = movement.Description,
                    Amount = (decimal)movement.Amount,
                    Id = movement.CurrentAccountId
                };
                if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                {
                    string? redirectUrl = movement.Debt
                        ? Url.Action(nameof(Invoice), invoice)
                        : Url.Action(nameof(Index), new { id = movement.CurrentAccountId });

                    return Json(new
                    {
                        success = true,
                        redirectUrl
                    });
                }
                else
                {
                    return movement.Debt
                        ? RedirectToAction(nameof(Invoice), invoice)
                        : RedirectToAction(nameof(Index), new { id = movement.CurrentAccountId });
                }
                
            }
            catch (Exception ex)
            {
                loggerService.Log($"Error creating Movement: {ex.Message}");
                ModelState.AddModelError("", "Error creating Movement. Please try again.");
            }
        }
        return RedirectToAction(nameof(Index), new {id = movement.CurrentAccountId});
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<ActionResult> Edit(Movements movement)
    {
        loggerService.Log("Movements Update POST action called.");
        if (ModelState.IsValid)
        {
            try
            {
                await update.UpdateMovement(movement);
                loggerService.Log($"Movement updated successfully for Account ID: {movement.CurrentAccountId}");
                return RedirectToAction(nameof(Index), new { currentAccountId = movement.CurrentAccountId });
            }
            catch (Exception ex)
            {
                loggerService.Log($"Error updating Movement: {ex.Message}");
                ModelState.AddModelError("", "Error updating Movement. Please try again.");
            }
        }
        return View(movement);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<ActionResult> Delete(int id)
    {
        loggerService.Log($"Movements Delete action called for ID: {id}");
        Movements? movement = await get.GetMovementById(id);
        if (movement == null)
        {
            loggerService.Log($"Movement with ID {id} not found.");
            return NotFound();
        }
        try
        {
            await delete.DeleteMovement(movement);
            loggerService.Log($"Movement deleted successfully for Account ID: {movement.CurrentAccountId}");
            return Ok();
        }
        catch (Exception ex)
        {
            loggerService.Log($"Error deleting Movement: {ex.Message}");
            ModelState.AddModelError("", "Error deleting Movement. Please try again.");
            return View(movement);
        }
    }

    public async Task<ActionResult> ManualPrint(int id)
    {
        Movements movement = await get.GetMovementById(id);
        Client client = await getClient.GetClientByCurrentAccountId(movement.CurrentAccountId);
        Invoice invoice = new()
        {
            Name = client.Name,
            LastName = client.Surname,
            Cuit = client.Cuit,
            Date = movement.Date,
            Description = movement.Description,
            Amount = (decimal)movement.Amount,
            Id = movement.CurrentAccountId
        };
        return RedirectToAction(nameof(Invoice), invoice);
    }

    [HttpGet, HttpPost]
    public ActionResult Invoice(Invoice invoice)
    {
        return View("Invoice", invoice);
    }
}
