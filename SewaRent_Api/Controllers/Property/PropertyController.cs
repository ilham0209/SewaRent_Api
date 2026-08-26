using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SewaRent_Api.Features.Property;
using SewaRent_Api.Shared.Models;

namespace SewaRent_Api.Controllers.Property;

[ApiController]
[Route("api/properties")]
public class PropertyController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? city = null,
        [FromQuery] string? state = null,
        [FromQuery] Guid? propertyTypeId = null,
        [FromQuery] decimal? minRent = null,
        [FromQuery] decimal? maxRent = null,
        [FromQuery] int? bedrooms = null,
        [FromQuery] bool? isFurnished = null,
        CancellationToken ct = default)
    {
        var query = new GetAllProperty.Query(
            page, pageSize, search, city, state, propertyTypeId,
            minRent, maxRent, bedrooms, isFurnished);

        return Ok(await sender.Send(query, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetByIdProperty.Query(id), ct);
        return result is not null ? Ok(result) : NotFound();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProperty.Command command, CancellationToken ct)
        => CreatedAtAction(nameof(GetById), new { id = command.Title },
            await sender.Send(command, ct));

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProperty.Command command, CancellationToken ct)
        => Ok(await sender.Send(command with { Id = id }, ct));

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeactivateProperty.Command(id), ct);
        return NoContent();
    }
}
