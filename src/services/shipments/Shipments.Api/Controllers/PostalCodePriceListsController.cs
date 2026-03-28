using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shipments.Api.Models;
using Shipments.Api.Services;

namespace Shipments.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/postal-code-price-lists")]
public sealed class PostalCodePriceListsController : ControllerBase
{
    private readonly PostalCodePriceListsService _postalCodePriceListsService;

    public PostalCodePriceListsController(PostalCodePriceListsService postalCodePriceListsService)
    {
        _postalCodePriceListsService = postalCodePriceListsService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostalCodePriceListResponse>>> GetPriceLists(CancellationToken cancellationToken)
    {
        return Ok(await _postalCodePriceListsService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{postalCodePriceListId:guid}")]
    public async Task<ActionResult<PostalCodePriceListResponse>> GetPriceList(Guid postalCodePriceListId, CancellationToken cancellationToken)
    {
        var priceList = await _postalCodePriceListsService.GetByIdAsync(postalCodePriceListId, cancellationToken);
        return priceList is null ? NotFound() : Ok(priceList);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PostalCodePriceListResponse>> CreatePriceList([FromBody] UpsertPostalCodePriceListRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _postalCodePriceListsService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetPriceList), new { postalCodePriceListId = created.Id }, created);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{postalCodePriceListId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PostalCodePriceListResponse>> UpdatePriceList(Guid postalCodePriceListId, [FromBody] UpsertPostalCodePriceListRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _postalCodePriceListsService.UpdateAsync(postalCodePriceListId, request, cancellationToken));
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