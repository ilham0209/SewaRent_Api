using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SewaRent_Api.Features.Billing;

namespace SewaRent_Api.Controllers.Billing;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoiceController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetByIdInvoice.Query(id), ct);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetMyInvoice.Query(page, pageSize, status), ct));

    [HttpPost("{id:guid}/mark-paid-claim")]
    public async Task<IActionResult> MarkPaidClaim(Guid id, CancellationToken ct)
    {
        await sender.Send(new MarkPaidClaimInvoice.Command(id), ct);
        return Ok(new { message = "Payment claimed successfully." });
    }

    [HttpPost("{id:guid}/accept-payment")]
    public async Task<IActionResult> AcceptPayment(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new AcceptPaymentInvoice.Command(id), ct));

    [HttpPost("{id:guid}/reject-payment")]
    public async Task<IActionResult> RejectPayment(
        Guid id,
        [FromBody] RejectPaymentRequest body,
        CancellationToken ct)
    {
        await sender.Send(new RejectPaymentInvoice.Command(id, body.Reason), ct);
        return Ok(new { message = "Payment rejected." });
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetPdfInvoice.Query(id), ct);
        return result is not null ? Ok(result) : NotFound();
    }

    public record RejectPaymentRequest(string Reason);
}
