using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Property;

public static class DeactivateProperty
{
    public record Command(Guid Id) : IRequest;

    public class Handler(PropertyDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken ct)
        {
            var userId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var property = await db.Properties.FindAsync([request.Id], ct)
                ?? throw new InvalidOperationException("Property not found.");

            if (property.LandlordId != userId)
                throw new UnauthorizedAccessException("You can only deactivate your own properties.");

            property.IsActive = false;
            property.SysUserModified = httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            property.SysDateModified = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
        }
    }
}
