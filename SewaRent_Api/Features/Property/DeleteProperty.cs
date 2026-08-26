using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Property;

public static class DeleteProperty
{
    public record Command(Guid Id) : IRequest;

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken ct)
        {
            var userId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var property = await db.Properties.FindAsync([request.Id], ct)
                ?? throw new InvalidOperationException("Property not found.");

            if (property.LandlordId != userId)
                throw new UnauthorizedAccessException("You can only delete your own properties.");

            property.IsDeleted = true;
            property.SysUserModified = httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            property.SysDateModified = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
        }
    }
}
