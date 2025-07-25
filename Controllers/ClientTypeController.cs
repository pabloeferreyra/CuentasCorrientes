using CuentasCorrientes.Models;
using CuentasCorrientes.Services;
using Microsoft.AspNetCore.Mvc;

namespace CuentasCorrientes.Controllers
{
    public class ClientTypeController(LoggerService loggerService,
        IGetClientTypeService getClientTypeService,
        ICreateClientTypeService createClientTypeService,
        IUpdateClientTypeService updateClientTypeService,
        IDeleteClientTypeService deleteClientTypeService) : Controller
    {
        public async Task<IActionResult> Index(string sortOrder)
        {
            loggerService.Log("ClientType Index action called.");
            var clientTypes = await getClientTypeService.GetAllClientTypes();

            clientTypes = sortOrder switch
            {
                "Name" => [.. clientTypes.OrderBy(c => c.Name)],
                "Name_desc" => [.. clientTypes.OrderByDescending(c => c.Name)],
                "Description" => [.. clientTypes.OrderBy(c => c.Description)],
                "Description_desc" => [.. clientTypes.OrderByDescending(c => c.Description)],
                _ => [.. clientTypes.OrderBy(c => c.Name)]
            };

            ViewBag.SortOrder = sortOrder;

                if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                return PartialView("_ClientTypeTable", clientTypes);

            return View(clientTypes);
        }

        public async Task<IActionResult> Details(int id)
        {
            loggerService.Log($"ClientType Details action called for ID: {id}");
            var clientType = await getClientTypeService.GetClientType(id);
            return View(clientType);
        }

        public IActionResult Create()
        {
            loggerService.Log("ClientType Create action called.");
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientType clientType)
        {
            loggerService.Log("ClientType Create POST action called.");
            if (ModelState.IsValid)
            {
                await createClientTypeService.CreateClientType(clientType);
                loggerService.Log($"ClientType created with ID: {clientType.Id}");
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            loggerService.Log($"ClientType Edit action called for ID: {id}");
            var clientType = await getClientTypeService.GetClientType(id);
            if (clientType == null)
            {
                loggerService.Log($"ClientType with ID: {id} not found.");
                return NotFound();
            }

            // Detecta AJAX correctamente
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                return PartialView("_Edit", clientType);

            return View(clientType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientType clientType)
        {
            loggerService.Log("ClientType Edit POST action called.");
            if (ModelState.IsValid)
            {
                await updateClientTypeService.UpdateClientType(clientType);
                loggerService.Log($"ClientType updated with ID: {clientType.Id}");
                return Json(new { success = true });
            }
            return PartialView("_Edit", clientType);
        }

        public async Task<IActionResult> Delete(int id)
        {
            loggerService.Log($"ClientType Delete action called for ID: {id}");
            var clientType = await getClientTypeService.GetClientType(id);
            if (clientType == null)
            {
                loggerService.Log($"ClientType with ID: {id} not found.");
                return NotFound();
            }
            return View(clientType);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            loggerService.Log($"ClientType DeleteConfirmed action called for ID: {id}");
            var clientType = await getClientTypeService.GetClientType(id);
            if (clientType == null)
            {
                loggerService.Log($"ClientType with ID: {id} not found.");
                return NotFound();
            }
            deleteClientTypeService.DeleteClientType(clientType);
            loggerService.Log($"ClientType deleted with ID: {id}");
            return RedirectToAction(nameof(Index));
        }
    }
}
