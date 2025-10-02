namespace CuentasCorrientes.Controllers;

[Authorize(Roles = "Administrador")]
public class BalanceController(LoggerService loggerService,
    IBalanceService balanceService) : Controller
{
    // GET: BalanceController
    public async Task<ActionResult> Index(int? year, int? month)
    {
        int resolvedYear = year ?? DateTime.Now.Year;
        int resolvedMonth = month ?? DateTime.Now.AddMonths(-1).Month;

        var balances = new List<Balance>();
        try
        {
            balances = await balanceService.GetBalance(resolvedYear, resolvedMonth);
        }
        catch (Exception ex)
        {
            loggerService.Log($"Error in Balance Index action: {ex.Message}");
        }

        // Provide the resolved values back to the view so inputs keep their selection
        ViewData["Year"] = resolvedYear;
        ViewData["Month"] = resolvedMonth;

        return View(balances);
    }

}
