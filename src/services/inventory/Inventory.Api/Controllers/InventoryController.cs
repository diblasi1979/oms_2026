using Inventory.Api.Models;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly InventoryService _inventoryService;

    public InventoryController(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemResponse>>> Search([FromQuery] string? sku, [FromQuery] Guid? warehouseId, CancellationToken cancellationToken)
    {
        return Ok(await _inventoryService.SearchAsync(sku, warehouseId, cancellationToken));
    }

    [HttpPost("reserve")]
    public async Task<ActionResult<IEnumerable<InventoryItemResponse>>> Reserve([FromBody] ReserveStockRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _inventoryService.ReserveAsync(request, cancellationToken));
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

    [HttpPost("release")]
    public async Task<ActionResult<IEnumerable<InventoryItemResponse>>> Release([FromBody] ReleaseStockRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _inventoryService.ReleaseAsync(request, cancellationToken));
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
