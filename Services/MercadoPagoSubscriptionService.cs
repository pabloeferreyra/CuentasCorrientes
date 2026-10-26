using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace CuentasCorrientes.Services;

public sealed class MercadoPagoOptions
{
    public string AccessToken { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string PublicBaseUrl { get; set; } = string.Empty;
    public string TestPayerEmail { get; set; } = string.Empty;
    public decimal MonthlyAmount { get; set; } = 17000m;
    public string CurrencyId { get; set; } = "ARS";
}

public sealed class MercadoPagoSubscriptionService(
    HttpClient httpClient,
    IOptions<MercadoPagoOptions> options)
{
    private readonly MercadoPagoOptions _options = options.Value;

    public async Task<MercadoPagoSubscriptionResponse> CreateAsync(
        string payerEmail,
        string externalReference,
        string notificationUrl,
        string backUrl,
        CancellationToken cancellationToken)
    {
        ValidateConfiguration();

        var request = new
        {
            reason = "Suscripción mensual a Cuentas Corrientes",
            external_reference = externalReference,
            payer_email = payerEmail,
            auto_recurring = new
            {
                frequency = 1,
                frequency_type = "months",
                transaction_amount = _options.MonthlyAmount,
                currency_id = _options.CurrencyId
            },
            notification_url = notificationUrl,
            back_url = backUrl,
            status = "pending"
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "preapproval")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        using var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorDetails = await ReadApiErrorDetailsAsync(response, cancellationToken);
            throw new HttpRequestException(
                $"Mercado Pago rechazó la creación de la suscripción ({(int)response.StatusCode}): {errorDetails}",
                null,
                response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MercadoPagoSubscriptionResponse>(cancellationToken)
            ?? throw new InvalidDataException("Mercado Pago devolvió una respuesta vacía al crear la suscripción.");
    }

    public async Task<MercadoPagoSubscriptionResponse> GetAsync(
        string subscriptionId,
        CancellationToken cancellationToken)
    {
        ValidateConfiguration();
        using var message = new HttpRequestMessage(
            HttpMethod.Get,
            $"preapproval/{Uri.EscapeDataString(subscriptionId)}");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        using var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorDetails = await ReadApiErrorDetailsAsync(response, cancellationToken);
            throw new HttpRequestException(
                $"Mercado Pago rechazó la consulta de la suscripción ({(int)response.StatusCode}): {errorDetails}",
                null,
                response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MercadoPagoSubscriptionResponse>(cancellationToken)
            ?? throw new InvalidDataException("Mercado Pago devolvió una respuesta vacía al consultar la suscripción.");
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.AccessToken))
            throw new InvalidOperationException("Falta configurar MercadoPago:AccessToken.");
        if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
            throw new InvalidOperationException("Falta configurar MercadoPago:WebhookSecret.");
        if (System.Text.Encoding.UTF8.GetByteCount(_options.WebhookSecret) < 32)
            throw new InvalidOperationException("MercadoPago:WebhookSecret debe tener al menos 32 bytes.");
        if (_options.MonthlyAmount <= 0)
            throw new InvalidOperationException("MercadoPago:MonthlyAmount debe ser mayor que cero.");
        if (string.IsNullOrWhiteSpace(_options.CurrencyId))
            throw new InvalidOperationException("Falta configurar MercadoPago:CurrencyId.");
    }

    private static async Task<string> ReadApiErrorDetailsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var details = new List<string>();
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (body.Length <= 8192)
        {
            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    AddStringProperty(root, "error", details);
                    AddStringProperty(root, "message", details);

                    if (root.TryGetProperty("cause", out var causes) && causes.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var cause in causes.EnumerateArray())
                        {
                            if (cause.ValueKind != JsonValueKind.Object)
                                continue;

                            if (cause.TryGetProperty("code", out var code))
                                details.Add($"cause code {code.GetRawText()}");
                            AddStringProperty(cause, "description", details);
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore unstructured response bodies; never log their raw contents.
            }
        }

        var requestId = response.Headers.TryGetValues("x-request-id", out var values)
            ? values.FirstOrDefault()
            : null;
        if (!string.IsNullOrWhiteSpace(requestId))
            details.Add($"request id {requestId}");

        if (details.Count == 0)
            return "la respuesta no incluyó un detalle estructurado";

        var message = string.Join("; ", details).Replace('\r', ' ').Replace('\n', ' ');
        return message.Length > 1000 ? message[..1000] : message;
    }

    private static void AddStringProperty(JsonElement element, string name, ICollection<string> details)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(value.GetString()))
        {
            details.Add($"{name}: {value.GetString()}");
        }
    }
}

public sealed class MercadoPagoSubscriptionResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("init_point")]
    public string? InitPoint { get; set; }

    [JsonPropertyName("external_reference")]
    public string? ExternalReference { get; set; }
}
