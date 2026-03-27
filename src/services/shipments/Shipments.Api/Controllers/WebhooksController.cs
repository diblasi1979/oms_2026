using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shipments.Api.Models;
using Shipments.Api.Services;

namespace Shipments.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/webhooks/carriers")]
public sealed class WebhooksController : ControllerBase
{
    private readonly ShipmentsService _shipmentsService;

    public WebhooksController(ShipmentsService shipmentsService)
    {
        _shipmentsService = shipmentsService;
    }

    [HttpPost("status-update")]
    public async Task<ActionResult<ShipmentResponse>> ReceiveStatusUpdate([FromBody] CarrierWebhookRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _shipmentsService.UpdateStatusAsync(request, cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
