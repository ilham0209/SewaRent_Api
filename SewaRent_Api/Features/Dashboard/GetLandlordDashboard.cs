using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Dashboard;

public static class GetLandlordDashboard
{
    public record Query() : IRequest<Response>;

    public record Response(
        decimal TotalCollectedThisMonth,
        int OverdueCount,
        int TotalTenants,
        List<TenantStatus> Tenants);

    public record TenantStatus(
        string TenantName,
        string PropertyTitle,
        string InvoiceStatus,
        DateTime? DueDate);

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken ct)
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

            var now = DateTime.UtcNow;
            var totalCollectedThisMonth = await db.Invoices
                .Where(i => landlordRentalRequestIds.Contains(i.RentalRequestId)
                    && i.Status == "Paid"
                    && i.PaidDate.HasValue
                    && i.PaidDate.Value.Month == now.Month
                    && i.PaidDate.Value.Year == now.Year
                    && !i.IsDeleted)
                .SumAsync(i => i.TotalAmount, ct);

            var overdueCount = await db.Invoices
                .Where(i => landlordRentalRequestIds.Contains(i.RentalRequestId)
                    && (i.Status == "Unpaid" || i.Status == "PaymentClaimed")
                    && i.DueDate < now
                    && !i.IsDeleted)
                .CountAsync(ct);

            var tenantIds = await db.RentalRequests
                .Where(r => landlordRentalRequestIds.Contains(r.Id) && !r.IsDeleted)
                .Select(r => r.TenantId)
                .Distinct()
                .ToListAsync(ct);

            var tenantStatuses = await (
                from rr in db.RentalRequests
                join p in db.Properties on rr.PropertyId equals p.Id
                join u in db.Users on rr.TenantId equals u.Id
                join i in db.Invoices on rr.Id equals i.RentalRequestId into invoices
                from inv in invoices.Where(i => !i.IsDeleted).OrderByDescending(i => i.DueDate).Take(1).DefaultIfEmpty()
                where landlordPropertyIds.Contains(rr.PropertyId) && !rr.IsDeleted
                select new TenantStatus(
                    u.FullName,
                    p.Title,
                    inv != null ? inv.Status : "No Invoice",
                    inv != null ? inv.DueDate : null))
                .ToListAsync(ct);

            return new Response(totalCollectedThisMonth, overdueCount, tenantIds.Count, tenantStatuses);
        }
    }
}
