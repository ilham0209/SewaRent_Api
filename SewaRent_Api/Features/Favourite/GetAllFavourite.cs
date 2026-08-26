using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;
using SewaRent_Api.Shared.Models;

namespace SewaRent_Api.Features.Favourite;

public static class GetAllFavourite
{
    public record Query(int Page, int PageSize) : IRequest<DataGridResponse<FavouriteProperty>>;

    public record FavouriteProperty(
        Guid PropertyId,
        string Title,
        decimal MonthlyRent,
        string City,
        string State,
        string? ImageUrl,
        DateTime SavedAt);

    public class Handler(FavouriteDbContext favouriteDb, PropertyDbContext propertyDb,
        IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Query, DataGridResponse<FavouriteProperty>>
    {
        public async Task<DataGridResponse<FavouriteProperty>> Handle(Query request, CancellationToken ct)
        {
            var userId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var query = from f in favouriteDb.Favourites
                        join p in propertyDb.Properties on f.PropertyId equals p.Id
                        where f.UserId == userId && !p.IsDeleted
                        orderby f.CreatedAt descending
                        select new FavouriteProperty(
                            p.Id,
                            p.Title,
                            p.MonthlyRent,
                            p.City,
                            p.State,
                            propertyDb.PropertyImages
                                .Where(i => i.PropertyId == p.Id && i.IsPrimary && !i.IsDeleted)
                                .Select(i => i.ImageUrl)
                                .FirstOrDefault(),
                            f.CreatedAt);

            var totalCount = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            return new DataGridResponse<FavouriteProperty>(
                items, totalCount, totalPages, request.Page, request.PageSize);
        }
    }
}
