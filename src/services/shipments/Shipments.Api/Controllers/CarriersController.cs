using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shipments.Api.Models;
using Shipments.Api.Services;

namespace Shipments.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/carriers")]
public sealed class CarriersController : ControllerBase
{
    private readonly CarriersService _carriersService;

    public CarriersController(CarriersService carriersService)
    {
        _carriersService = carriersService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CarrierResponse>>> GetCarriers([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        return Ok(await _carriersService.GetAllAsync(includeInactive, cancellationToken));
    }

    [HttpGet("{carrierId:guid}")]
    public async Task<ActionResult<CarrierResponse>> GetCarrier(Guid carrierId, CancellationToken cancellationToken)
    {
        var carrier = await _carriersService.GetByIdAsync(carrierId, cancellationToken);
        return carrier is null ? NotFound() : Ok(carrier);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CarrierResponse>> CreateCarrier([FromBody] UpsertCarrierRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _carriersService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetCarrier), new { carrierId = created.Id }, created);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{carrierId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CarrierResponse>> UpdateCarrier(Guid carrierId, [FromBody] UpsertCarrierRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _carriersService.UpdateAsync(carrierId, request, cancellationToken));
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
