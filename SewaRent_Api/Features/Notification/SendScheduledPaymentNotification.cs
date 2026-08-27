using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Domain.Notification;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Notification;

public static class SendScheduledPaymentNotification
{
    public record Command(Guid RentalRequestId, int ScheduleDay, Guid? InvoiceId, string? Message) : IRequest<Response>;

    public record Response(Guid Id, DateTime SentAt);

    public class Handler(SewaRentDbContext db) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var notification = new PaymentNotificationEntity
            {
                RentalRequestId = request.RentalRequestId,
                NotificationType = "Scheduled",
                RecipientRole = "Tenant",
                InvoiceId = request.InvoiceId,
                ScheduleDay = request.ScheduleDay,
                Message = request.Message,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                SysUserCreated = "System",
                SysDateCreated = DateTime.UtcNow
            };

            db.PaymentNotifications.Add(notification);
            await db.SaveChangesAsync(ct);

            return new Response(notification.Id, notification.SentAt);
        }
    }
}
