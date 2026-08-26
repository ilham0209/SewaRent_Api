using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Features.RentalRequest;
using SewaRent_Api.Shared.Domain.Property;
using SewaRent_Api.Shared.Domain.RentalRequest;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Tests.Features.RentalRequest;

public class CreateRentalRequestTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_NonExistentProperty_ThrowsInvalidOperation()
    {
        await using var rentalDb = TestDbFactory.CreateRentalRequestDb();
        await using var propertyDb = TestDbFactory.CreatePropertyDb();
        var httpAccessor = TestDbFactory.CreateHttpContextAccessor(TestUserId, "test@example.com");

        var handler = new CreateRentalRequest.Handler(rentalDb, propertyDb, httpAccessor);
        var command = new CreateRentalRequest.Command(Guid.NewGuid(), "Interested");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public void Validator_EmptyPropertyId_FailsValidation()
    {
        var validator = new CreateRentalRequest.Validator();
        var command = new CreateRentalRequest.Command(Guid.Empty, null);

        var result = validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Handle_UnavailableProperty_ThrowsInvalidOperation()
    {
        await using var rentalDb = TestDbFactory.CreateRentalRequestDb();
        await using var propertyDb = TestDbFactory.CreatePropertyDb();
        var httpAccessor = TestDbFactory.CreateHttpContextAccessor(TestUserId, "test@example.com");

        var property = new Shared.Domain.Property.PropertyEntity
        {
            Title = "Rented Condo",
            MonthlyRent = 1500m,
            PropertyTypeId = Guid.NewGuid(),
            LandlordId = Guid.NewGuid(),
            AddressLine1 = "123 Main St",
            City = "Kuala Lumpur",
            State = "WP",
            Bedrooms = 3,
            Bathrooms = 2,
            IsFurnished = true,
            IsActive = true,
            AvailabilityStatus = "Rented",
            SysUserCreated = "System",
            SysDateCreated = DateTime.UtcNow
        };
        propertyDb.Properties.Add(property);
        await propertyDb.SaveChangesAsync();

        var handler = new CreateRentalRequest.Handler(rentalDb, propertyDb, httpAccessor);
        var command = new CreateRentalRequest.Command(property.Id, "Interested");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
