using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Extensions;
using SewaRent_Api.Shared.Infrastructure.Persistence;
using SewaRent_Api.Shared.Models;

namespace SewaRent_Api.Features.Billing;

public static class GetLandlordInvoice
{
    public record Query(int Page, int PageSize, string? Status) : IRequest<DataGridResponse<LandlordInvoiceSummary>>;

    public record LandlordInvoiceSummary(
        Guid Id,
        string InvoiceNumber,
        string TenantName,
        string PropertyTitle,
        decimal TotalAmount,
        string Status,
        DateTime DueDate);

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Query, DataGridResponse<LandlordInvoiceSummary>>
    {
        public async Task<DataGridResponse<LandlordInvoiceSummary>> Handle(Query request, CancellationToken ct)
        {
            var landlordId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var landlordPropertyIds = await db.Properties
                .Where(p => p.LandlordId == landlordId && !p.IsDeleted)
                .Select(p => p.Id)
                .ToListAsync(ct);

            var landlordRentalRequestIds = await db.RentalRequests
                .Where(r => landlordPropertyIds.Contains(r.PropertyId) && !r.IsDeleted)
                .Select(r => r.Id)
                .ToListAsync(ct);

            var query = from i in db.Invoices
                        join rr in db.RentalRequests on i.RentalRequestId equals rr.Id
                        join p in db.Properties on rr.PropertyId equals p.Id
                        join u in db.Users on rr.TenantId equals u.Id
                        where landlordRentalRequestIds.Contains(i.RentalRequestId) && !i.IsDeleted
                        orderby i.DueDate descending
                        select new LandlordInvoiceSummary(
                            i.Id,
                            i.InvoiceNumber,
                            u.FullName,
                            p.Title,
                            i.TotalAmount,
                            i.Status,
                            i.DueDate);

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(i => i.Status == request.Status);

            return await query.ToDataGridResponseAsync(request.Page, request.PageSize, ct);
        }
    }
}
