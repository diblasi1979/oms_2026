using System.Text.Json;
using Integrations.Api.Models;
using Integrations.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integrations.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/external-orders")]
public sealed class ExternalOrdersController : ControllerBase
{
    private readonly ExternalOrdersService _externalOrdersService;

    public ExternalOrdersController(ExternalOrdersService externalOrdersService)
    {
        _externalOrdersService = externalOrdersService;
    }

    [HttpPost("{provider}")]
    public async Task<ActionResult<CanonicalExternalOrderResponse>> ReceiveExternalOrder(string provider, [FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ExternalProvider>(provider, true, out var parsedProvider))
        {
            return BadRequest(new { message = "Proveedor no soportado. Usa Shopify, MercadoLibre, WooCommerce o Amazon." });
        }

        try
        {
            return Accepted(await _externalOrdersService.RegisterAsync(parsedProvider, payload, cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (Exception exception) when (exception is InvalidOperationException or KeyNotFoundException or JsonException)
        {
            return BadRequest(new { message = $"Payload inválido para {parsedProvider}: {exception.Message}" });
        }
    }
}
