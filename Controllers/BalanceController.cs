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

    // GET: Balance/Export
    public async Task<FileResult> Export(int? year, int? month)
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
            loggerService.Log($"Error in Balance Export action: {ex.Message}");
        }

        // Build an HTML table compatible with Excel (.xls)
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/></head><body>");
        sb.AppendLine($"<h2>Balance - {resolvedYear} / {resolvedMonth}</h2>");
        sb.AppendLine("<table border=\"1\"><thead><tr>");
        sb.AppendLine("<th>Fecha</th><th>Descripcion</th><th>Cliente</th><th>Debe</th><th>Haber</th>");
        sb.AppendLine("</tr></thead><tbody>");

        var culture = System.Globalization.CultureInfo.GetCultureInfo("es-AR");
        foreach (var item in balances)
        {
            var date = item.Date.ToString("dd/MM/yyyy", culture);
            var client = $"{item.Name} {item.Surname}";
            var debe = item.Amount > 0 ? item.Amount.ToString("C", culture) : string.Empty;
            var haber = item.Amount <= 0 ? item.Amount.ToString("C", culture) : string.Empty;

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(date)}</td>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(item.Description)}</td>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(client)}</td>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(debe)}</td>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(haber)}</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table>");
        sb.AppendLine("</body></html>");

        var fileName = $"Balance_{resolvedYear}_{resolvedMonth}.xlsx";
        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());

        // Return as Excel file
        return File(bytes, "application/vnd.ms-excel", fileName);
    }

}
