using System.Security.Claims;
using FluentValidation;
using MediatR;
using SewaRent_Api.Shared.Domain.Property;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Property;

public static class CreateProperty
{
    public record Command(
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
            var landlordId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var property = new Shared.Domain.Property.PropertyEntity
            {
                LandlordId = landlordId,
                Title = request.Title,
                Description = request.Description,
                MonthlyRent = request.MonthlyRent,
                PropertyTypeId = request.PropertyTypeId,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                City = request.City,
                State = request.State,
                Postcode = request.Postcode,
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                ParkingSpaces = request.ParkingSpaces,
                IsFurnished = request.IsFurnished,
                IsActive = true,
                AvailabilityStatus = "Available",
                SysUserCreated = httpContextAccessor.HttpContext!.User
                    .FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                SysDateCreated = DateTime.UtcNow
            };

            db.Properties.Add(property);
            await db.SaveChangesAsync(ct);

            return new Response(property.Id, property.Title, property.MonthlyRent);
        }
    }
}
