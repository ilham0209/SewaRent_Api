using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SewaRent_Api.Features.Notification;

namespace SewaRent_Api.Controllers.Notification;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class PaymentNotificationController(ISender sender) : ControllerBase
{
    [HttpGet("my")]
    public async Task<IActionResult> GetMy(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetMyPaymentNotification.Query(page, pageSize), ct));
}
