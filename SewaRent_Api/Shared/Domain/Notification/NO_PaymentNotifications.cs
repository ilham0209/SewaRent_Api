using SewaRent_Api.Shared.Domain;

namespace SewaRent_Api.Shared.Domain.Notification;

public class PaymentNotificationEntity : BaseClass
{
    public Guid RentalRequestId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string RecipientRole { get; set; } = string.Empty;
    public Guid? InvoiceId { get; set; }
    public int? ScheduleDay { get; set; }
    public string? Message { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}
