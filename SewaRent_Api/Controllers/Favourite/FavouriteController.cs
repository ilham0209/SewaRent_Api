using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SewaRent_Api.Features.Favourite;

namespace SewaRent_Api.Controllers.Favourite;

[ApiController]
[Route("api/favourites")]
[Authorize]
public class FavouriteController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetAllFavourite.Query(page, pageSize), ct));

    [HttpPost]
    public async Task<IActionResult> Add(AddFavourite.Command command, CancellationToken ct)
    {
        await sender.Send(command, ct);
        return Ok(new { message = "Property added to favourites." });
    }

    [HttpDelete("{propertyId:guid}")]
    public async Task<IActionResult> Remove(Guid propertyId, CancellationToken ct)
    {
        await sender.Send(new RemoveFavourite.Command(propertyId), ct);
        return NoContent();
    }
}
