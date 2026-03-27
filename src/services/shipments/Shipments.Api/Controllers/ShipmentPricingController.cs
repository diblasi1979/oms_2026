using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shipments.Api.Models;
using Shipments.Api.Services;

namespace Shipments.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/shipment-pricing")]
public sealed class ShipmentPricingController : ControllerBase
{
    private readonly ShipmentPricingService _shipmentPricingService;

    public ShipmentPricingController(ShipmentPricingService shipmentPricingService)
    {
        _shipmentPricingService = shipmentPricingService;
    }

    [HttpGet("settings")]
    public async Task<ActionResult<ShipmentPricingSettingsResponse>> GetSettings(CancellationToken cancellationToken)
    {
        return Ok(await _shipmentPricingService.GetSettingsAsync(cancellationToken));
    }

    [HttpPut("settings")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ShipmentPricingSettingsResponse>> UpdateSettings([FromBody] UpdateShipmentPricingSettingsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _shipmentPricingService.UpdateSettingsAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("quote")]
    [AllowAnonymous]
    public async Task<ActionResult<ShipmentPricingQuoteResponse>> GetQuote([FromQuery] ShipmentPricingQuoteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _shipmentPricingService.QuoteAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("quote")]
    [AllowAnonymous]
    public async Task<ActionResult<ShipmentPricingQuoteResponse>> Quote([FromBody] ShipmentPricingQuoteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _shipmentPricingService.QuoteAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}