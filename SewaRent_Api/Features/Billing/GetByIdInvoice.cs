using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Billing;

public static class GetByIdInvoice
{
    public record Query(Guid Id) : IRequest<Response?>;

    public record Response(
        Guid Id,
        string InvoiceNumber,
        int BillingPeriodMonth,
        int BillingPeriodYear,
        decimal RentAmount,
        decimal? UtilityTotal,
        decimal TotalAmount,
        string Status,
        DateTime DueDate,
        DateTime? PaidDate,
        string? RejectReason,
        string? BankName,
        string? BankAccountNumber,
        List<InvoiceItemDto> Items);

    public record InvoiceItemDto(Guid Id, string ItemType, string? Description, decimal Amount);

    public class Handler(SewaRentDbContext db) : IRequestHandler<Query, Response?>
    {
        public async Task<Response?> Handle(Query request, CancellationToken ct)
        {
            var invoice = await db.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == request.Id && !i.IsDeleted, ct);

            if (invoice is null)
                return null;

            var items = await db.InvoiceItems
                .Where(i => i.InvoiceId == invoice.Id && !i.IsDeleted)
                .Select(i => new InvoiceItemDto(i.Id, i.ItemType, i.Description, i.Amount))
                .ToListAsync(ct);

            return new Response(
                invoice.Id,
                invoice.InvoiceNumber,
                invoice.BillingPeriodMonth,
                invoice.BillingPeriodYear,
                invoice.RentAmount,
                invoice.UtilityTotal,
                invoice.TotalAmount,
                invoice.Status,
                invoice.DueDate,
                invoice.PaidDate,
                invoice.RejectReason,
                invoice.BankNameSnapshot,
                invoice.BankAccountNumberSnapshot,
                items);
        }
    }
}
