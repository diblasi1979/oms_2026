using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shipments.Api.Models;
using Shipments.Api.Services;

namespace Shipments.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/shipments")]
public sealed class ShipmentsController : ControllerBase
{
    private readonly ShipmentsService _shipmentsService;

    public ShipmentsController(ShipmentsService shipmentsService)
    {
        _shipmentsService = shipmentsService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShipmentResponse>>> GetShipments(CancellationToken cancellationToken)
    {
        return Ok(await _shipmentsService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{shipmentId:guid}")]
    public async Task<ActionResult<ShipmentResponse>> GetShipment(Guid shipmentId, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentsService.GetByIdAsync(shipmentId, cancellationToken);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [HttpPost]
    public async Task<ActionResult<ShipmentResponse>> CreateShipment([FromBody] CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var shipment = await _shipmentsService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetShipment), new { shipmentId = shipment.Id }, shipment);
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

    [HttpPost("{shipmentId:guid}/label")]
    public async Task<ActionResult<ShippingLabelResponse>> GenerateLabel(Guid shipmentId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _shipmentsService.GenerateLabelAsync(shipmentId, cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPost("by-order/{orderId:guid}/label")]
    public async Task<ActionResult<ShippingLabelResponse>> GenerateLabelByOrder(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _shipmentsService.GenerateLabelByOrderAsync(orderId, cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
