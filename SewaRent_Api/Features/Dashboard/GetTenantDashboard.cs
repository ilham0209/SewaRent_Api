using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Dashboard;

public static class GetTenantDashboard
{
    public record Query() : IRequest<Response>;

    public record Response(
        CurrentInvoiceDto? CurrentInvoice,
        List<HistoryItem> History);

    public record CurrentInvoiceDto(
        string Status,
        decimal TotalAmount,
        DateTime DueDate,
        string InvoiceNumber);

    public record HistoryItem(
        Guid Id,
        string InvoiceNumber,
        string Status,
        decimal TotalAmount,
        bool HasReceipt);

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken ct)
        {
            var tenantId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var tenantRentalRequestIds = await db.RentalRequests
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .Select(r => r.Id)
                .ToListAsync(ct);

            var currentInvoice = await db.Invoices
                .Where(i => tenantRentalRequestIds.Contains(i.RentalRequestId)
                    && (i.Status == "Unpaid" || i.Status == "PaymentClaimed")
                    && !i.IsDeleted)
                .OrderByDescending(i => i.DueDate)
                .Select(i => new CurrentInvoiceDto(
                    i.Status,
                    i.TotalAmount,
                    i.DueDate,
                    i.InvoiceNumber))
                .FirstOrDefaultAsync(ct);

            var history = await db.Invoices
                .Where(i => tenantRentalRequestIds.Contains(i.RentalRequestId) && !i.IsDeleted)
                .OrderByDescending(i => i.DueDate)
                .Select(i => new HistoryItem(
                    i.Id,
                    i.InvoiceNumber,
                    i.Status,
                    i.TotalAmount,
                    db.Receipts.Any(r => r.InvoiceId == i.Id && !r.IsDeleted)))
                .ToListAsync(ct);

            return new Response(currentInvoice, history);
        }
    }
}
