namespace CuentasCorrientes.Helpers
{
    public static class StringExtensions
    {
        public static string ToCuitFormat(this string cuit)
        {
            if (string.IsNullOrEmpty(cuit))
                return cuit;

            var digits = new string([.. cuit.Where(char.IsDigit)]);
            return digits.Length == 11
                ? $"{digits[..2]}-{digits.Substring(2, 8)}-{digits.Substring(10, 1)}"
                : cuit;
        }
    }
}
