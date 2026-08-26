using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Features.Favourite;
using SewaRent_Api.Shared.Domain.Property;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Tests.Features.Favourite;

public class AddFavouriteTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_NonExistentProperty_ThrowsInvalidOperation()
    {
        await using var propertyDb = TestDbFactory.CreatePropertyDb();
        await using var favouriteDb = TestDbFactory.CreateFavouriteDb();
        var httpAccessor = TestDbFactory.CreateHttpContextAccessor(TestUserId, "test@example.com");

        var handler = new AddFavourite.Handler(favouriteDb, propertyDb, httpAccessor);
        var command = new AddFavourite.Command(Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public void AddFavourite_Validator_EmptyPropertyId_FailsValidation()
    {
        var validator = new AddFavourite.Validator();
        var command = new AddFavourite.Command(Guid.Empty);

        var result = validator.Validate(command);
        Assert.False(result.IsValid);
    }
}
