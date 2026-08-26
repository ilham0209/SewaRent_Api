using SewaRent_Api.Shared.Domain;

namespace SewaRent_Api.Shared.Domain.Property;

public class PropertyImageEntity : BaseClass
{
    public Guid PropertyId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}
