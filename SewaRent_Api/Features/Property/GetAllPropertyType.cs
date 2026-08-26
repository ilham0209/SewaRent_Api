using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Property;

public static class GetAllPropertyType
{
    public record Query() : IRequest<List<Response>>;

    public record Response(Guid Id, string Name, string? Description, bool IsActive);

    public class Handler(SewaRentDbContext db) : IRequestHandler<Query, List<Response>>
    {
        public async Task<List<Response>> Handle(Query request, CancellationToken ct)
        {
            return await db.PropertyTypes
                .Where(x => x.IsActive)
                .Select(x => new Response(x.Id, x.Name, x.Description, x.IsActive))
                .ToListAsync(ct);
        }
    }
}
