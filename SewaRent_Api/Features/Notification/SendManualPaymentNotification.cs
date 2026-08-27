using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Domain.Notification;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Notification;

public static class SendManualPaymentNotification
{
    public record Command(Guid RentalRequestId, string? Message) : IRequest<Response>;

    public record Response(Guid Id, DateTime SentAt);

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var landlordId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var rentalRequest = await db.RentalRequests
                .FirstOrDefaultAsync(r => r.Id == request.RentalRequestId && !r.IsDeleted, ct)
                ?? throw new InvalidOperationException("Rental request not found.");

            var property = await db.Properties
                .FirstOrDefaultAsync(p => p.Id == rentalRequest.PropertyId, ct)
                ?? throw new InvalidOperationException("Property not found.");

            if (property.LandlordId != landlordId)
                throw new UnauthorizedAccessException("You can only send reminders for your own properties.");

            var notification = new PaymentNotificationEntity
            {
                RentalRequestId = request.RentalRequestId,
                NotificationType = "Manual",
                RecipientRole = "Tenant",
                Message = request.Message,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                SysUserCreated = httpContextAccessor.HttpContext!.User
                    .FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                SysDateCreated = DateTime.UtcNow
            };

            db.PaymentNotifications.Add(notification);
            await db.SaveChangesAsync(ct);

            return new Response(notification.Id, notification.SentAt);
        }
    }
}
