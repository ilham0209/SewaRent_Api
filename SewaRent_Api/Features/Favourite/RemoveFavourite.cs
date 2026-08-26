using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Favourite;

public static class RemoveFavourite
{
    public record Command(Guid PropertyId) : IRequest;

    public class Handler(FavouriteDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken ct)
        {
            var userId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var favourite = await db.Favourites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == request.PropertyId, ct)
                ?? throw new InvalidOperationException("Favourite not found.");

            db.Favourites.Remove(favourite);
            await db.SaveChangesAsync(ct);
        }
    }
}
