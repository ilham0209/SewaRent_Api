using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Billing;

public static class GetPdfReceipt
{
    public record Query(Guid Id) : IRequest<Response?>;

    public record Response(
        string ReceiptNumber,
        DateTime IssuedDate,
        string InvoiceNumber,
        decimal TotalAmount,
        string TenantName,
        string PropertyTitle,
        string LandlordName);

    public class Handler(SewaRentDbContext db) : IRequestHandler<Query, Response?>
    {
        public async Task<Response?> Handle(Query request, CancellationToken ct)
        {
            var receipt = await db.Receipts
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, ct);

            if (receipt is null)
                return null;

            var invoice = await db.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == receipt.InvoiceId, ct);

            if (invoice is null)
                return null;

            var rentalRequest = await db.RentalRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == invoice.RentalRequestId, ct);

            if (rentalRequest is null)
                return null;

            var tenant = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == rentalRequest.TenantId, ct);

            var property = await db.Properties
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == rentalRequest.PropertyId, ct);

            var landlord = property is not null
                ? await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == property.LandlordId, ct)
                : null;

            if (tenant is null || property is null || landlord is null)
                return null;

            receipt.PdfGeneratedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);

            return new Response(
                receipt.ReceiptNumber,
                receipt.IssuedDate,
                invoice.InvoiceNumber,
                invoice.TotalAmount,
                tenant.FullName,
                property.Title,
                landlord.FullName);
        }
    }
}
