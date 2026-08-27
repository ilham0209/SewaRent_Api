using SewaRent_Api.Shared.Domain;

namespace SewaRent_Api.Shared.Domain.Billing;

public class InvoiceItemEntity : BaseClass
{
    public Guid InvoiceId { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
}
