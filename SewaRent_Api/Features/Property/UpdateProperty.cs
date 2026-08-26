using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Property;

public static class UpdateProperty
{
    public record Command(
        Guid Id,
        string Title,
        string? Description,
        decimal MonthlyRent,
        Guid PropertyTypeId,
        string AddressLine1,
        string? AddressLine2,
        string City,
        string State,
        string? Postcode,
        int Bedrooms,
        int Bathrooms,
        int? ParkingSpaces,
        bool IsFurnished) : IRequest<Response>;

    public record Response(Guid Id, string Title, decimal MonthlyRent);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.MonthlyRent).GreaterThan(0);
            RuleFor(x => x.PropertyTypeId).NotEmpty();
            RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(255);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.State).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Bedrooms).GreaterThan(0);
            RuleFor(x => x.Bathrooms).GreaterThan(0);
        }
    }

    public class Handler(PropertyDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var userId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var property = await db.Properties.FindAsync([request.Id], ct)
                ?? throw new InvalidOperationException("Property not found.");

            if (property.LandlordId != userId)
                throw new UnauthorizedAccessException("You can only edit your own properties.");

            property.Title = request.Title;
            property.Description = request.Description;
            property.MonthlyRent = request.MonthlyRent;
            property.PropertyTypeId = request.PropertyTypeId;
            property.AddressLine1 = request.AddressLine1;
            property.AddressLine2 = request.AddressLine2;
            property.City = request.City;
            property.State = request.State;
            property.Postcode = request.Postcode;
            property.Bedrooms = request.Bedrooms;
            property.Bathrooms = request.Bathrooms;
            property.ParkingSpaces = request.ParkingSpaces;
            property.IsFurnished = request.IsFurnished;
            property.SysUserModified = httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            property.SysDateModified = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);

            return new Response(property.Id, property.Title, property.MonthlyRent);
        }
    }
}
