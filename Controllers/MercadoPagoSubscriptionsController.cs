using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CuentasCorrientes.Services;
using Microsoft.Extensions.Options;

namespace CuentasCorrientes.Controllers;

[AllowAnonymous]
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
            null,
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

    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [RequestSizeLimit(16_384)]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        var dataId = Request.Query["data.id"].FirstOrDefault();
        var notificationType = Request.Query["type"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(notificationType))
        {
            notificationType = Request.Query["topic"].FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(notificationType))
        {
            try
            {
                using var payload = await JsonDocument.ParseAsync(Request.Body, cancellationToken: cancellationToken);
                if (payload.RootElement.TryGetProperty("type", out var typeValue)
                    && typeValue.ValueKind == JsonValueKind.String)
                {
                    notificationType = typeValue.GetString();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (JsonException)
            {
                return BadRequest();
            }
        }

        if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            logger.LogError("Mercado Pago webhook secret is not configured.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        if (string.IsNullOrWhiteSpace(dataId))
        {
            return BadRequest();
        }

        if (!MercadoPagoWebhookSignature.IsValid(
                Request.Headers["x-signature"].FirstOrDefault(),
                Request.Headers["x-request-id"].FirstOrDefault(),
                dataId,
                _options.WebhookSecret))
        {
            return Unauthorized();
        }

        if (!string.Equals(notificationType, "subscription_preapproval", StringComparison.Ordinal))
        {
            return Ok();
        }

        var preapproval = await subscriptionService.GetAsync(dataId, cancellationToken);
        if (!string.Equals(preapproval.Id, dataId, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(preapproval.ExternalReference))
        {
            logger.LogWarning("Mercado Pago webhook returned a mismatched subscription identifier.");
            return Ok();
        }

        var subscription = await dbContext.ApplicationSubscriptions.SingleOrDefaultAsync(
            record => record.ExternalReference == preapproval.ExternalReference,
            cancellationToken);
        if (subscription is null)
        {
            logger.LogWarning("Received a Mercado Pago webhook without a matching local subscription.");
            return Ok();
        }

        if (subscription.MercadoPagoId is not null
            && !string.Equals(subscription.MercadoPagoId, preapproval.Id, StringComparison.Ordinal))
        {
            logger.LogError("Mercado Pago webhook identifier did not match the stored subscription.");
            return Ok();
        }

        subscription.MercadoPagoId = preapproval.Id;
        subscription.Status = preapproval.Status;
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

        var isSuperUser = User.IsInRole("SuperUser");
        var subscription = await dbContext.ApplicationSubscriptions.SingleOrDefaultAsync(
            item => item.Id == id &&
                (isSuperUser ||
                 item.UserId == userId ||
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

}
