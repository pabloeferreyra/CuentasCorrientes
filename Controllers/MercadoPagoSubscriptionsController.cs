using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CuentasCorrientes.Services;
using Microsoft.Extensions.Options;

namespace CuentasCorrientes.Controllers;

[Authorize]
public class MercadoPagoSubscriptionsController(
    ApplicationDbContext dbContext,
    UserManager<IdentityUser> userManager,
    MercadoPagoSubscriptionService subscriptionService,
    IOptions<MercadoPagoOptions> options,
    IWebHostEnvironment environment,
    ILogger<MercadoPagoSubscriptionsController> logger) : Controller
{
    private readonly MercadoPagoOptions _options = options.Value;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var subscriptionsQuery = dbContext.ApplicationSubscriptions.AsQueryable();
        if (!User.IsInRole("SuperUser"))
            subscriptionsQuery = subscriptionsQuery.Where(subscription => subscription.UserId == userId);

        var subscriptions = await (
            from subscription in subscriptionsQuery
            join identityUser in dbContext.Users
                on subscription.UserId equals identityUser.Id into identityUsers
            from identityUser in identityUsers.DefaultIfEmpty()
            orderby subscription.CreatedAt descending
            select new ApplicationSubscriptionListItem
            {
                Id = subscription.Id,
                UserName = identityUser == null
                    ? "Usuario eliminado"
                    : identityUser.UserName ?? identityUser.Email ?? "Usuario",
                MercadoPagoId = subscription.MercadoPagoId,
                Status = subscription.Status,
                MonthlyAmount = subscription.MonthlyAmount,
                CurrencyId = subscription.CurrencyId,
                CreatedAt = subscription.CreatedAt,
                UpdatedAt = subscription.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        ViewBag.MonthlyAmount = _options.MonthlyAmount;
        ViewBag.CurrencyId = _options.CurrencyId;
        return View(subscriptions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(string? payerEmail, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(payerEmail) || !new EmailAddressAttribute().IsValid(payerEmail))
        {
            TempData["SubscriptionError"] = "Ingresá un correo válido asociado a tu cuenta de Mercado Pago.";
            return RedirectToAction(nameof(Index));
        }

        var mercadoPagoPayerEmail = environment.IsDevelopment() &&
            !string.IsNullOrWhiteSpace(_options.TestPayerEmail)
                ? _options.TestPayerEmail
                : payerEmail;

        var hasOngoingSubscription = await dbContext.ApplicationSubscriptions.AnyAsync(
            subscription => subscription.UserId == userId &&
                (subscription.Status == "pending" || subscription.Status == "authorized"),
            cancellationToken);
        if (hasOngoingSubscription)
        {
            TempData["SubscriptionError"] = "Ya existe una suscripción pendiente o activa para esta cuenta.";
            return RedirectToAction(nameof(Index));
        }

        var publicBaseUrl = _options.PublicBaseUrl.TrimEnd('/');
        if (!Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out var publicUri) ||
            publicUri.Scheme != Uri.UriSchemeHttps)
        {
            TempData["SubscriptionError"] = "Configurá MercadoPago:PublicBaseUrl con la URL HTTPS pública del sistema.";
            return RedirectToAction(nameof(Index));
        }

        var externalReference = Guid.NewGuid().ToString("N");
        var subscription = new ApplicationSubscription
        {
            UserId = userId,
            ExternalReference = externalReference,
            MonthlyAmount = _options.MonthlyAmount,
            CurrencyId = _options.CurrencyId
        };
        dbContext.ApplicationSubscriptions.Add(subscription);
        await dbContext.SaveChangesAsync(cancellationToken);

        var notificationUrl = Url.Action(
            nameof(Webhook),
            "MercadoPagoSubscriptions",
            new { secret = _options.WebhookSecret },
            Uri.UriSchemeHttps,
            publicUri.Authority);
        var backUrl = Url.Action(
            nameof(Return),
            "MercadoPagoSubscriptions",
            new { externalReference },
            Uri.UriSchemeHttps,
            publicUri.Authority);

        if (notificationUrl is null || backUrl is null)
        {
            TempData["SubscriptionError"] = "No se pudieron generar las URL de retorno del pago.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var result = await subscriptionService.CreateAsync(
                mercadoPagoPayerEmail,
                externalReference,
                notificationUrl,
                backUrl,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(result.Id) ||
                string.IsNullOrWhiteSpace(result.InitPoint) ||
                result.ExternalReference != externalReference)
            {
                throw new InvalidDataException("La respuesta de Mercado Pago no contiene los datos esperados.");
            }

            if (!Uri.TryCreate(result.InitPoint, UriKind.Absolute, out var checkoutUri) ||
                checkoutUri.Scheme != Uri.UriSchemeHttps ||
                !(checkoutUri.Host.Equals("mercadopago.com.ar", StringComparison.OrdinalIgnoreCase) ||
                  checkoutUri.Host.EndsWith(".mercadopago.com.ar", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidDataException("Mercado Pago devolvió una URL de checkout no válida.");
            }

            await dbContext.Entry(subscription).ReloadAsync(cancellationToken);
            subscription.MercadoPagoId = result.Id;
            if (subscription.Status == "creating")
                subscription.Status = result.Status ?? "pending";
            subscription.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Redirect(checkoutUri.ToString());
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Mercado Pago rechazó crear la suscripción {ExternalReference}.", externalReference);
            subscription.Status = "creation_failed";
            subscription.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            TempData["SubscriptionError"] = "Mercado Pago no pudo iniciar la suscripción. Revisá la configuración e intentá nuevamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "La integración de Mercado Pago no está configurada correctamente.");
            subscription.Status = "creation_failed";
            subscription.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            TempData["SubscriptionError"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidDataException ex)
        {
            logger.LogError(ex, "Mercado Pago devolvió una respuesta inválida al crear {ExternalReference}.", externalReference);
            subscription.Status = "creation_failed";
            subscription.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            TempData["SubscriptionError"] = "Mercado Pago devolvió una respuesta inesperada.";
            return RedirectToAction(nameof(Index));
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "No se pudo interpretar la respuesta de Mercado Pago al crear {ExternalReference}.", externalReference);
            subscription.Status = "creation_failed";
            subscription.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            TempData["SubscriptionError"] = "Mercado Pago devolvió una respuesta inesperada.";
            return RedirectToAction(nameof(Index));
        }
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Webhook(
        [FromQuery] string? secret,
        [FromBody] JsonElement payload,
        CancellationToken cancellationToken)
    {
        if (!IsValidWebhookSecret(secret))
            return Unauthorized();

        var notificationType = GetJsonString(payload, "type") ?? Request.Query["type"].ToString();
        if (notificationType != "subscription_preapproval")
            return Ok();

        var subscriptionId = GetJsonString(payload, "data", "id") ?? Request.Query["data.id"].ToString();
        if (string.IsNullOrWhiteSpace(subscriptionId))
            return BadRequest();

        var remoteSubscription = await subscriptionService.GetAsync(subscriptionId, cancellationToken);
        if (string.IsNullOrWhiteSpace(remoteSubscription.Id) ||
            string.IsNullOrWhiteSpace(remoteSubscription.ExternalReference) ||
            string.IsNullOrWhiteSpace(remoteSubscription.Status))
        {
            return BadRequest();
        }

        var subscription = await dbContext.ApplicationSubscriptions.SingleOrDefaultAsync(
            item => item.ExternalReference == remoteSubscription.ExternalReference,
            cancellationToken);
        if (subscription is null)
            return Ok();

        if (subscription.MercadoPagoId is not null && subscription.MercadoPagoId != remoteSubscription.Id)
            return BadRequest();

        subscription.MercadoPagoId = remoteSubscription.Id;
        subscription.Status = remoteSubscription.Status;
        subscription.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Return(
        string? externalReference,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var subscription = await dbContext.ApplicationSubscriptions.SingleOrDefaultAsync(
            item => item.ExternalReference == externalReference &&
                (item.UserId == userId ||
                 (environment.IsDevelopment() && item.UserId == Guid.Empty.ToString())),
            cancellationToken);

        if (subscription?.MercadoPagoId is null)
        {
            TempData["SubscriptionMessage"] =
                "Regresaste de Mercado Pago. El estado se actualizará cuando recibamos su notificación.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var current = await subscriptionService.GetAsync(subscription.MercadoPagoId, cancellationToken);
            if (current.Id != subscription.MercadoPagoId ||
                current.ExternalReference != subscription.ExternalReference ||
                string.IsNullOrWhiteSpace(current.Status))
            {
                logger.LogWarning(
                    "Mercado Pago devolvió datos inconsistentes al actualizar la suscripción {ExternalReference}.",
                    subscription.ExternalReference);
                TempData["SubscriptionError"] = "No se pudo verificar el estado de la suscripción. Se actualizará al recibir la notificación.";
                return RedirectToAction(nameof(Index));
            }

            if (environment.IsDevelopment() && subscription.UserId == Guid.Empty.ToString())
                subscription.UserId = userId;
            subscription.Status = current.Status;
            subscription.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            TempData["SubscriptionMessage"] = current.Status == "authorized"
                ? "Mercado Pago confirmó la autorización de la suscripción."
                : $"Mercado Pago informa que la suscripción sigue en estado «{current.Status}».";
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(
                ex,
                "No se pudo consultar en Mercado Pago el estado de la suscripción {ExternalReference} al regresar del checkout.",
                subscription.ExternalReference);
            TempData["SubscriptionError"] = "No se pudo verificar el estado en este momento. Se actualizará al recibir la notificación.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refresh(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var subscription = await dbContext.ApplicationSubscriptions.SingleOrDefaultAsync(
            item => item.Id == id &&
                (item.UserId == userId ||
                 (environment.IsDevelopment() && item.UserId == Guid.Empty.ToString())),
            cancellationToken);
        if (subscription is null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(subscription.MercadoPagoId))
        {
            TempData["SubscriptionError"] = "Esta suscripción no tiene un identificador de Mercado Pago para consultar.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var current = await subscriptionService.GetAsync(subscription.MercadoPagoId, cancellationToken);
            if (current.Id != subscription.MercadoPagoId ||
                current.ExternalReference != subscription.ExternalReference ||
                string.IsNullOrWhiteSpace(current.Status))
            {
                logger.LogWarning(
                    "Mercado Pago devolvió datos inconsistentes al actualizar la suscripción local {SubscriptionId}.",
                    subscription.Id);
                TempData["SubscriptionError"] = "Mercado Pago devolvió datos que no coinciden con esta suscripción.";
                return RedirectToAction(nameof(Index));
            }

            if (environment.IsDevelopment() && subscription.UserId == Guid.Empty.ToString())
                subscription.UserId = userId;
            subscription.Status = current.Status;
            subscription.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Se actualizó la suscripción local {SubscriptionId} con estado remoto {Status}.",
                subscription.Id,
                current.Status);
            TempData["SubscriptionMessage"] = $"Estado consultado directamente en Mercado Pago: {current.Status}.";
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(
                ex,
                "No se pudo consultar en Mercado Pago la suscripción local {SubscriptionId}.",
                subscription.Id);
            TempData["SubscriptionError"] = "No se pudo consultar Mercado Pago. Revisá el log de la aplicación para ver el detalle.";
        }
        catch (JsonException ex)
        {
            logger.LogWarning(
                ex,
                "No se pudo interpretar la respuesta de Mercado Pago para la suscripción local {SubscriptionId}.",
                subscription.Id);
            TempData["SubscriptionError"] = "Mercado Pago devolvió una respuesta que no se pudo interpretar.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool IsValidWebhookSecret(string? secret)
    {
        if (string.IsNullOrEmpty(secret) ||
            string.IsNullOrEmpty(_options.WebhookSecret) ||
            Encoding.UTF8.GetByteCount(_options.WebhookSecret) < 32)
            return false;

        var provided = Encoding.UTF8.GetBytes(secret);
        var expected = Encoding.UTF8.GetBytes(_options.WebhookSecret);
        return provided.Length == expected.Length &&
            CryptographicOperations.FixedTimeEquals(provided, expected);
    }

    private static string? GetJsonString(JsonElement element, params string[] path)
    {
        foreach (var property in path)
        {
            if (element.ValueKind != JsonValueKind.Object ||
                !element.TryGetProperty(property, out element))
            {
                return null;
            }
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetRawText(),
            _ => null
        };
    }
}
