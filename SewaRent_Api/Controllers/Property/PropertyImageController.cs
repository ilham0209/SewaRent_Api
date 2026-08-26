using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SewaRent_Api.Features.Property;

namespace SewaRent_Api.Controllers.Property;

[ApiController]
[Route("api/properties/{propertyId:guid}/images")]
[Authorize]
public class PropertyImageController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Upload(
        Guid propertyId,
        UploadPropertyImage.Command command,
        CancellationToken ct)
        => Ok(await sender.Send(command with { PropertyId = propertyId }, ct));

    [HttpDelete("{imageId:guid}")]
    public async Task<IActionResult> Delete(Guid propertyId, Guid imageId, CancellationToken ct)
    {
        await sender.Send(new DeletePropertyImage.Command(propertyId, imageId), ct);
        return NoContent();
    }
}
