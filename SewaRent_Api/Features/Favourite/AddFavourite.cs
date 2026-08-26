using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Favourite;

public static class AddFavourite
{
    public record Command(Guid PropertyId) : IRequest;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.PropertyId).NotEmpty();
        }
    }

    public class Handler(FavouriteDbContext favouriteDb, PropertyDbContext propertyDb,
        IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken ct)
        {
            var userId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var propertyExists = await propertyDb.Properties
                .AnyAsync(p => p.Id == request.PropertyId, ct);

            if (!propertyExists)
                throw new InvalidOperationException("Property not found.");

            var alreadyFavourited = await favouriteDb.Favourites
                .AnyAsync(f => f.UserId == userId && f.PropertyId == request.PropertyId, ct);

            if (alreadyFavourited)
                throw new InvalidOperationException("Property is already in favourites.");

            favouriteDb.Favourites.Add(new Shared.Domain.Favourite.FavouriteEntity
            {
                UserId = userId,
                PropertyId = request.PropertyId,
                CreatedAt = DateTime.UtcNow
            });

            await favouriteDb.SaveChangesAsync(ct);
        }
    }
}
