using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Property;

public static class DeletePropertyImage
{
    public record Command(Guid PropertyId, Guid ImageId) : IRequest;

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken ct)
        {
            var userId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var property = await db.Properties.FindAsync([request.PropertyId], ct)
                ?? throw new InvalidOperationException("Property not found.");

            if (property.LandlordId != userId)
                throw new UnauthorizedAccessException("You can only delete images from your own properties.");

            var image = await db.PropertyImages
                .FirstOrDefaultAsync(i => i.Id == request.ImageId && i.PropertyId == request.PropertyId, ct)
                ?? throw new InvalidOperationException("Image not found.");

            image.IsDeleted = true;
            image.SysUserModified = httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            image.SysDateModified = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
        }
    }
}
