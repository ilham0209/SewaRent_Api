using SewaRent_Api.Shared.Domain;

namespace SewaRent_Api.Shared.Domain.Property;

public class PropertyEntity : BaseClass
{
    public Guid LandlordId { get; set; }
    public Guid PropertyTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyRent { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? Postcode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int? ParkingSpaces { get; set; }
    public bool IsFurnished { get; set; }
    public string AvailabilityStatus { get; set; } = "Available";
    public bool IsActive { get; set; } = true;

    public PropertyTypeEntity PropertyType { get; set; } = null!;
    public ICollection<PropertyImageEntity> PropertyImages { get; set; } = new List<PropertyImageEntity>();
}

public class PropertyTypeEntity : BaseClass
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PropertyImageEntity : BaseClass
{
    public Guid PropertyId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}
