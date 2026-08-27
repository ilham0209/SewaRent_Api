using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Billing;

public static class MarkPaidClaimInvoice
{
    public record Command(Guid Id) : IRequest;

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken ct)
        {
            var tenantId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var invoice = await db.Invoices
                .FirstOrDefaultAsync(i => i.Id == request.Id && !i.IsDeleted, ct)
                ?? throw new InvalidOperationException("Invoice not found.");

            var rentalRequest = await db.RentalRequests
                .FirstOrDefaultAsync(r => r.Id == invoice.RentalRequestId, ct)
                ?? throw new InvalidOperationException("Rental request not found.");

            if (rentalRequest.TenantId != tenantId)
                throw new UnauthorizedAccessException("You can only claim payment for your own invoices.");

            if (invoice.Status != "Unpaid")
                throw new InvalidOperationException("Only unpaid invoices can be marked as payment claimed.");

            invoice.Status = "PaymentClaimed";
            invoice.SysUserModified = httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            invoice.SysDateModified = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
        }
    }
}
