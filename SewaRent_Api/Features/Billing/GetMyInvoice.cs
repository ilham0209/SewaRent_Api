using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Extensions;
using SewaRent_Api.Shared.Infrastructure.Persistence;
using SewaRent_Api.Shared.Models;

namespace SewaRent_Api.Features.Billing;

public static class GetMyInvoice
{
    public record Query(int Page, int PageSize, string? Status) : IRequest<DataGridResponse<InvoiceSummary>>;

    public record InvoiceSummary(
        Guid Id,
        string InvoiceNumber,
        decimal TotalAmount,
        string Status,
        DateTime DueDate);

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Query, DataGridResponse<InvoiceSummary>>
    {
        public async Task<DataGridResponse<InvoiceSummary>> Handle(Query request, CancellationToken ct)
        {
            var tenantId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var tenantRentalRequestIds = await db.RentalRequests
                .Where(r => r.TenantId == tenantId && !r.IsDeleted)
                .Select(r => r.Id)
                .ToListAsync(ct);

            var query = db.Invoices
                .Where(i => tenantRentalRequestIds.Contains(i.RentalRequestId) && !i.IsDeleted)
                .OrderByDescending(i => i.DueDate)
                .Select(i => new InvoiceSummary(
                    i.Id,
                    i.InvoiceNumber,
                    i.TotalAmount,
                    i.Status,
                    i.DueDate));

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(i => i.Status == request.Status);

            return await query.ToDataGridResponseAsync(request.Page, request.PageSize, ct);
        }
    }
}
