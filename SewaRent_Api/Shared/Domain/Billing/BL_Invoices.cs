using SewaRent_Api.Shared.Domain;

namespace SewaRent_Api.Shared.Domain.Billing;

public class InvoiceEntity : BaseClass
{
    public Guid RentalRequestId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int BillingPeriodMonth { get; set; }
    public int BillingPeriodYear { get; set; }
    public decimal RentAmount { get; set; }
    public decimal? UtilityTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Unpaid";
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? RejectReason { get; set; }
    public string? BankNameSnapshot { get; set; }
    public string? BankAccountNumberSnapshot { get; set; }
    public DateTime? PdfGeneratedAt { get; set; }

    public ICollection<InvoiceItemEntity> InvoiceItems { get; set; } = new List<InvoiceItemEntity>();
    public ReceiptEntity? Receipt { get; set; }
}
