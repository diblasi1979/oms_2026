using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Models;
using Orders.Api.Services;

namespace Orders.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly CustomersService _customersService;

    public CustomersController(CustomersService customersService)
    {
        _customersService = customersService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetCustomers([FromQuery] bool includeInactive = false, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        return Ok(await _customersService.GetAllAsync(includeInactive, search, cancellationToken));
    }

    [HttpGet("{customerId:guid}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await _customersService.GetByIdAsync(customerId, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer([FromBody] UpsertCustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _customersService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetCustomer), new { customerId = created.Id }, created);
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

    [HttpPut("{customerId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CustomerResponse>> UpdateCustomer(Guid customerId, [FromBody] UpsertCustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _customersService.UpdateAsync(customerId, request, cancellationToken));
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