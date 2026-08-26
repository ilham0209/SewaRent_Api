using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SewaRent_Api.Features.RentalRequest;

namespace SewaRent_Api.Controllers.RentalRequest;

[ApiController]
[Route("api/landlord/rental-requests")]
[Authorize]
public class LandlordRentalRequestController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? propertyId = null,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetLandlordRentalRequest.Query(page, pageSize, propertyId), ct));

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] DecideRequest? body,
        CancellationToken ct)
    {
        await sender.Send(new ApproveRentalRequest.Command(id, body?.DecisionNote), ct);
        return Ok(new { message = "Rental request approved." });
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] DecideRequest? body,
        CancellationToken ct)
    {
        await sender.Send(new RejectRentalRequest.Command(id, body?.DecisionNote), ct);
        return Ok(new { message = "Rental request rejected." });
    }

    public record DecideRequest(string? DecisionNote);
}
