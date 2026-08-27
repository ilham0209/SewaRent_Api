using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Extensions;
using SewaRent_Api.Shared.Infrastructure.Persistence;
using SewaRent_Api.Shared.Models;

namespace SewaRent_Api.Features.Notification;

public static class GetMyPaymentNotification
{
    public record Query(int Page, int PageSize) : IRequest<DataGridResponse<PaymentNotificationSummary>>;

    public record PaymentNotificationSummary(
        Guid Id,
        string NotificationType,
        string? Message,
        DateTime SentAt,
        bool IsRead,
        Guid? InvoiceId);

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Query, DataGridResponse<PaymentNotificationSummary>>
    {
        public async Task<DataGridResponse<PaymentNotificationSummary>> Handle(Query request, CancellationToken ct)
        {
            var userId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var role = httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Role);

            var query = db.PaymentNotifications
                .Where(n => !n.IsDeleted && n.RecipientRole == role);

            if (role == "Tenant")
            {
                var tenantRentalRequestIds = await db.RentalRequests
                    .Where(r => r.TenantId == userId && !r.IsDeleted)
                    .Select(r => r.Id)
                    .ToListAsync(ct);

                query = query.Where(n => tenantRentalRequestIds.Contains(n.RentalRequestId)
                    && (n.NotificationType == "Scheduled" || n.NotificationType == "Manual"));
            }
            else if (role == "Landlord")
            {
                var landlordPropertyIds = await db.Properties
                    .Where(p => p.LandlordId == userId && !p.IsDeleted)
                    .Select(p => p.Id)
                    .ToListAsync(ct);

                var landlordRentalRequestIds = await db.RentalRequests
                    .Where(r => landlordPropertyIds.Contains(r.PropertyId) && !r.IsDeleted)
                    .Select(r => r.Id)
                    .ToListAsync(ct);

                query = query.Where(n => landlordRentalRequestIds.Contains(n.RentalRequestId)
                    && n.NotificationType == "Overdue");
            }

            var projectedQuery = query
                .OrderByDescending(n => n.SentAt)
                .Select(n => new PaymentNotificationSummary(
                    n.Id,
                    n.NotificationType,
                    n.Message,
                    n.SentAt,
                    n.IsRead,
                    n.InvoiceId));

            return await projectedQuery.ToDataGridResponseAsync(request.Page, request.PageSize, ct);
        }
    }
}
