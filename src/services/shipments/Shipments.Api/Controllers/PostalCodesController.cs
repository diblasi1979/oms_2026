using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shipments.Api.Models;
using Shipments.Api.Services;

namespace Shipments.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/postal-codes")]
public sealed class PostalCodesController : ControllerBase
{
    private readonly PostalCodesService _postalCodesService;

    public PostalCodesController(PostalCodesService postalCodesService)
    {
        _postalCodesService = postalCodesService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostalCodeResponse>>> GetPostalCodes([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        return Ok(await _postalCodesService.GetAllAsync(includeInactive, cancellationToken));
    }

    [HttpGet("{postalCodeId:guid}")]
    public async Task<ActionResult<PostalCodeResponse>> GetPostalCode(Guid postalCodeId, CancellationToken cancellationToken)
    {
        var postalCode = await _postalCodesService.GetByIdAsync(postalCodeId, cancellationToken);
        return postalCode is null ? NotFound() : Ok(postalCode);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PostalCodeResponse>> CreatePostalCode([FromBody] UpsertPostalCodeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _postalCodesService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetPostalCode), new { postalCodeId = created.Id }, created);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{postalCodeId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PostalCodeResponse>> UpdatePostalCode(Guid postalCodeId, [FromBody] UpsertPostalCodeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _postalCodesService.UpdateAsync(postalCodeId, request, cancellationToken));
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