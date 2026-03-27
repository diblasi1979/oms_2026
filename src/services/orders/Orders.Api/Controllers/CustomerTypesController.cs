using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Models;
using Orders.Api.Services;

namespace Orders.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/customer-types")]
public sealed class CustomerTypesController : ControllerBase
{
    private readonly CustomerTypesService _customerTypesService;

    public CustomerTypesController(CustomerTypesService customerTypesService)
    {
        _customerTypesService = customerTypesService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerTypeResponse>>> GetCustomerTypes([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        return Ok(await _customerTypesService.GetAllAsync(includeInactive, cancellationToken));
    }

    [HttpGet("{customerTypeId:guid}")]
    public async Task<ActionResult<CustomerTypeResponse>> GetCustomerType(Guid customerTypeId, CancellationToken cancellationToken)
    {
        var customerType = await _customerTypesService.GetByIdAsync(customerTypeId, cancellationToken);
        return customerType is null ? NotFound() : Ok(customerType);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CustomerTypeResponse>> CreateCustomerType([FromBody] UpsertCustomerTypeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _customerTypesService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetCustomerType), new { customerTypeId = created.Id }, created);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{customerTypeId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CustomerTypeResponse>> UpdateCustomerType(Guid customerTypeId, [FromBody] UpsertCustomerTypeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _customerTypesService.UpdateAsync(customerTypeId, request, cancellationToken));
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