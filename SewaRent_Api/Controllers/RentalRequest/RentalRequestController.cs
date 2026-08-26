using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SewaRent_Api.Features.RentalRequest;

namespace SewaRent_Api.Controllers.RentalRequest;

[ApiController]
[Route("api/rental-requests")]
[Authorize]
public class RentalRequestController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateRentalRequest.Command command, CancellationToken ct)
        => Ok(await sender.Send(command, ct));

    [HttpGet("my")]
    public async Task<IActionResult> GetMy(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetMyRentalRequest.Query(page, pageSize), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetByIdRentalRequest.Query(id), ct);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await sender.Send(new CancelRentalRequest.Command(id), ct);
        return Ok(new { message = "Rental request cancelled." });
    }
}
