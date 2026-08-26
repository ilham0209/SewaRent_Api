using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Features.Property;
using SewaRent_Api.Shared.Domain.Property;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Tests.Features.Property;

public class GetByIdPropertyTests
{
    [Fact]
    public async Task Handle_ExistingProperty_ReturnsResponse()
    {
        await using var db = TestDbFactory.CreatePropertyDb();
        var propertyType = new PropertyTypeEntity
        {
            Name = "Apartment",
            IsActive = true,
            SysUserCreated = "System",
            SysDateCreated = DateTime.UtcNow
        };
        db.PropertyTypes.Add(propertyType);

        var property = new Shared.Domain.Property.PropertyEntity
        {
            Title = "Nice Condo",
            MonthlyRent = 1500m,
            PropertyTypeId = propertyType.Id,
            LandlordId = Guid.NewGuid(),
            AddressLine1 = "123 Main St",
            City = "Kuala Lumpur",
            State = "WP",
            Bedrooms = 3,
            Bathrooms = 2,
            IsFurnished = true,
            IsActive = true,
            SysUserCreated = "System",
            SysDateCreated = DateTime.UtcNow
        };
        db.Properties.Add(property);
        await db.SaveChangesAsync();

        var handler = new GetByIdProperty.Handler(db);
        var result = await handler.Handle(new GetByIdProperty.Query(property.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Nice Condo", result!.Title);
        Assert.Equal(1500m, result.MonthlyRent);
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsNull()
    {
        await using var db = TestDbFactory.CreatePropertyDb();
        var handler = new GetByIdProperty.Handler(db);
        var result = await handler.Handle(new GetByIdProperty.Query(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
