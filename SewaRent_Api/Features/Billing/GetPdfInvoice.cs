using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Billing;

public static class GetPdfInvoice
{
    public record Query(Guid Id) : IRequest<Response?>;

    public record Response(
        string InvoiceNumber,
        int BillingPeriodMonth,
        int BillingPeriodYear,
        decimal RentAmount,
        decimal? UtilityTotal,
        decimal TotalAmount,
        string Status,
        DateTime DueDate,
        string? BankName,
        string? BankAccountNumber,
        string TenantName,
        string PropertyTitle,
        string PropertyAddress,
        List<InvoiceItemDto> Items);

    public record InvoiceItemDto(string ItemType, string? Description, decimal Amount);

    public class Handler(SewaRentDbContext db) : IRequestHandler<Query, Response?>
    {
        public async Task<Response?> Handle(Query request, CancellationToken ct)
        {
            var invoice = await db.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == request.Id && !i.IsDeleted, ct);

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

            if (tenant is null || property is null)
                return null;

            var items = await db.InvoiceItems
                .Where(i => i.InvoiceId == invoice.Id && !i.IsDeleted)
                .Select(i => new InvoiceItemDto(i.ItemType, i.Description, i.Amount))
                .ToListAsync(ct);

            invoice.PdfGeneratedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);

            return new Response(
                invoice.InvoiceNumber,
                invoice.BillingPeriodMonth,
                invoice.BillingPeriodYear,
                invoice.RentAmount,
                invoice.UtilityTotal,
                invoice.TotalAmount,
                invoice.Status,
                invoice.DueDate,
                invoice.BankNameSnapshot,
                invoice.BankAccountNumberSnapshot,
                tenant.FullName,
                property.Title,
                property.AddressLine1,
                items);
        }
    }
}
