namespace CuentasCorrientes.Controllers;

public class MovementsController(LoggerService loggerService,
    IGetMovementService get,
    ICreateMovementService create,
    IUpdateMovementService update,
    IDeleteMovementService delete) : Controller
{
    public async Task<ActionResult> Index(int id, string sortOrder)
    {
        loggerService.Log($"Movements Index action called for Current Account ID: {id}");
        List<Movements> movements = await get.GetMovementsByCurrentAccountId(id);
        movements = sortOrder switch
        {
            "Date" => [.. movements.OrderBy(m => m.Date)],
            "Date_desc" => [.. movements.OrderByDescending(m => m.Date)],
            _ => [.. movements.OrderBy(m => m.Id)]
        };
        ViewBag.SortOrder = sortOrder;
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            return PartialView("_MovementTable", movements);
        ViewBag.CurrentAccountId = id;
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
                loggerService.Log($"Movement created successfully for Current Account ID: {movement.CurrentAccountId}");
                return RedirectToAction(nameof(Index), new { currentAccountId = movement.CurrentAccountId });
            }
            catch (Exception ex)
            {
                loggerService.Log($"Error creating Movement: {ex.Message}");
                ModelState.AddModelError("", "Error creating Movement. Please try again.");
            }
        }
        return RedirectToAction(nameof(Index), new {currentAccountId = movement.CurrentAccountId});
    }

    [HttpPost]
    public async Task<ActionResult> Edit(Movements movement)
    {
        loggerService.Log("Movements Update POST action called.");
        if (ModelState.IsValid)
        {
            try
            {
                await update.UpdateMovement(movement);
                loggerService.Log($"Movement updated successfully for Current Account ID: {movement.CurrentAccountId}");
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
            delete.DeleteMovement(movement);
            loggerService.Log($"Movement deleted successfully for Current Account ID: {movement.CurrentAccountId}");
            return RedirectToAction(nameof(Index), new { currentAccountId = movement.CurrentAccountId });
        }
        catch (Exception ex)
        {
            loggerService.Log($"Error deleting Movement: {ex.Message}");
            ModelState.AddModelError("", "Error deleting Movement. Please try again.");
            return View(movement);
        }
    }
}
