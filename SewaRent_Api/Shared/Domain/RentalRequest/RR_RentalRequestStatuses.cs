using SewaRent_Api.Shared.Domain;

namespace SewaRent_Api.Shared.Domain.RentalRequest;

public class RentalRequestStatusEntity : BaseClass
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
