namespace CuentasCorrientes.Helpers;

public static class CurrencyFormatting
{
    private static readonly CultureInfo ArgentineCulture = CreateArgentineCulture();

    public static string Format(double amount) => amount.ToString("C", ArgentineCulture);
    public static string Format(decimal amount) => amount.ToString("C", ArgentineCulture);

    private static CultureInfo CreateArgentineCulture()
    {
        var culture = (CultureInfo)CultureInfo.GetCultureInfo("es-AR").Clone();
        culture.NumberFormat.CurrencySymbol = "$";
        return culture;
    }
}
