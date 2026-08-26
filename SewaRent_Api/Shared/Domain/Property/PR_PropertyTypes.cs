using SewaRent_Api.Shared.Domain;

namespace SewaRent_Api.Shared.Domain.Property;

public class PropertyTypeEntity : BaseClass
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
