using SewaRent_Api.Shared.Domain;

namespace SewaRent_Api.Shared.Domain.Billing;

public class ReceiptEntity : BaseClass
{
    public Guid InvoiceId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public DateTime? PdfGeneratedAt { get; set; }
}
