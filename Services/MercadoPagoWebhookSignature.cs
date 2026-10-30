using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace CuentasCorrientes.Services;

internal static class MercadoPagoWebhookSignature
{
    public static bool IsValid(
        string? signatureHeader,
        string? requestId,
        string? dataId,
        string secret)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader) ||
            string.IsNullOrWhiteSpace(requestId) ||
            string.IsNullOrWhiteSpace(dataId) ||
            string.IsNullOrWhiteSpace(secret))
        {
            return false;
        }

        string? timestamp = null;
        string? signatureHex = null;
        foreach (var part in signatureHeader.Split(','))
        {
            var keyValue = part.Split('=', 2);
            if (keyValue.Length != 2)
            {
                continue;
            }

            var key = keyValue[0].Trim();
            var value = keyValue[1].Trim();
            switch (key)
            {
                case "ts" when timestamp is null:
                    timestamp = value;
                    break;
                case "v1" when signatureHex is null:
                    signatureHex = value;
                    break;
                case "ts" or "v1":
                    return false;
            }
        }

        if (timestamp is null ||
            !long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out _) ||
            signatureHex is null ||
            signatureHex.Length != 64)
        {
            return false;
        }

        byte[] providedSignature;
        try
        {
            providedSignature = Convert.FromHexString(signatureHex);
        }
        catch (FormatException)
        {
            return false;
        }

        var manifest = $"id:{dataId.ToLowerInvariant()};request-id:{requestId};ts:{timestamp};";
        var expectedSignature = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes(manifest));

        return CryptographicOperations.FixedTimeEquals(providedSignature, expectedSignature);
    }
}
