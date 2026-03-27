using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Models;
using Orders.Api.Services;

namespace Orders.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly OrdersService _ordersService;

    public OrdersController(OrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderSummaryResponse>>> GetOrders([FromQuery] OrderStatus? status, [FromQuery] string? search, CancellationToken cancellationToken)
    {
        return Ok(await _ordersService.GetOrdersAsync(status, search, cancellationToken));
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailResponse>> GetOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _ordersService.GetByIdAsync(orderId, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDetailResponse>> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _ordersService.CreateOrderAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetOrder), new { orderId = order.Id }, order);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{orderId:guid}/status")]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDetailResponse>> UpdateStatus(Guid orderId, [FromQuery] OrderStatus status, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _ordersService.UpdateStatusAsync(orderId, status, cancellationToken));
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
