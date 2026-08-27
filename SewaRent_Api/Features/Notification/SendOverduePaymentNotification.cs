using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Domain.Notification;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Notification;

public static class SendOverduePaymentNotification
{
    public record Command(Guid InvoiceId) : IRequest<Response>;

    public record Response(Guid Id, DateTime SentAt);

    public class Handler(SewaRentDbContext db) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var invoice = await db.Invoices
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && !i.IsDeleted, ct)
                ?? throw new InvalidOperationException("Invoice not found.");

            var rentalRequest = await db.RentalRequests
                .FirstOrDefaultAsync(r => r.Id == invoice.RentalRequestId, ct)
                ?? throw new InvalidOperationException("Rental request not found.");

            var property = await db.Properties
                .FirstOrDefaultAsync(p => p.Id == rentalRequest.PropertyId, ct)
                ?? throw new InvalidOperationException("Property not found.");

            var existingNotification = await db.PaymentNotifications
                .AnyAsync(n => n.InvoiceId == request.InvoiceId
                    && n.NotificationType == "Overdue"
                    && !n.IsDeleted, ct);

            if (existingNotification)
                throw new InvalidOperationException("An overdue notification for this invoice has already been sent.");

            var notification = new PaymentNotificationEntity
            {
                RentalRequestId = invoice.RentalRequestId,
                NotificationType = "Overdue",
                RecipientRole = "Landlord",
                InvoiceId = request.InvoiceId,
                Message = $"Invoice {invoice.InvoiceNumber} is overdue. Amount: {invoice.TotalAmount:C2}.",
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
