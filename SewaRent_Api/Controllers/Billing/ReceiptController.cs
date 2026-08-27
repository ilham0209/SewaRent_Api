using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SewaRent_Api.Features.Billing;

namespace SewaRent_Api.Controllers.Billing;

[ApiController]
[Route("api/receipts")]
[Authorize]
public class ReceiptController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetPdfReceipt.Query(id), ct);
        return result is not null ? Ok(result) : NotFound();
    }
}
