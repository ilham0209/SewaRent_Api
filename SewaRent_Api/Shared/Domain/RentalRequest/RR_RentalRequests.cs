using SewaRent_Api.Shared.Domain;

namespace SewaRent_Api.Shared.Domain.RentalRequest;

public class RentalRequestEntity : BaseClass
{
    public Guid PropertyId { get; set; }
    public Guid TenantId { get; set; }
    public Guid StatusId { get; set; }
    public string? Message { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DecisionAt { get; set; }
    public string? DecisionNote { get; set; }
}
