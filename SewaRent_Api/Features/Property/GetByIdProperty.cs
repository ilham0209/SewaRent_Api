using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Property;

public static class GetByIdProperty
{
    public record Query(Guid Id) : IRequest<Response?>;

    public record Response(
        Guid Id,
        string Title,
        string? Description,
        decimal MonthlyRent,
        string AddressLine1,
        string? AddressLine2,
        string City,
        string State,
        string? Postcode,
        int Bedrooms,
        int Bathrooms,
        int? ParkingSpaces,
        bool IsFurnished,
        string AvailabilityStatus,
        string PropertyTypeName,
        Guid LandlordId,
        List<ImageDto> Images);

    public record ImageDto(Guid Id, string ImageUrl, bool IsPrimary, int SortOrder);

    public class Handler(PropertyDbContext db) : IRequestHandler<Query, Response?>
    {
        public async Task<Response?> Handle(Query request, CancellationToken ct)
        {
            var property = await db.Properties
                .Include(p => p.PropertyType)
                .Include(p => p.PropertyImages.Where(i => !i.IsDeleted).OrderBy(i => i.SortOrder))
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (property is null || property.IsDeleted)
                return null;

            return new Response(
                property.Id,
                property.Title,
                property.Description,
                property.MonthlyRent,
                property.AddressLine1,
                property.AddressLine2,
                property.City,
                property.State,
                property.Postcode,
                property.Bedrooms,
                property.Bathrooms,
                property.ParkingSpaces,
                property.IsFurnished,
                property.AvailabilityStatus,
                property.PropertyType.Name,
                property.LandlordId,
                property.PropertyImages.Select(i => new ImageDto(i.Id, i.ImageUrl, i.IsPrimary, i.SortOrder)).ToList());
        }
    }
}
