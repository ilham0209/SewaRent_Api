using FluentValidation;
using SewaRent_Api.Features.Property;
using SewaRent_Api.Shared.Domain.Property;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Tests.Features.Property;

public class CreatePropertyTests
{
    [Fact]
    public void Validator_EmptyTitle_FailsValidation()
    {
        var validator = new CreateProperty.Validator();
        var command = new CreateProperty.Command(
            "", null, 1500m, Guid.NewGuid(),
            "123 Main St", null, "Kuala Lumpur", "WP", null,
            3, 2, null, true);

        var result = validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_ZeroRent_FailsValidation()
    {
        var validator = new CreateProperty.Validator();
        var command = new CreateProperty.Command(
            "Nice Condo", null, 0m, Guid.NewGuid(),
            "123 Main St", null, "Kuala Lumpur", "WP", null,
            3, 2, null, true);

        var result = validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_ValidCommand_PassesValidation()
    {
        var validator = new CreateProperty.Validator();
        var command = new CreateProperty.Command(
            "Nice Condo", null, 1500m, Guid.NewGuid(),
            "123 Main St", null, "Kuala Lumpur", "WP", null,
            3, 2, null, true);

        var result = validator.Validate(command);
        Assert.True(result.IsValid);
    }
}
