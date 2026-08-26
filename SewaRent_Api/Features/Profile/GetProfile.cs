using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Profile;

public static class GetProfile
{
    public record Query(Guid UserId) : IRequest<Response?>;

    public record Response(Guid Id, string FullName, string Email, string? PhoneNumber, string? ProfileImageUrl);

    public class Handler(SewaRentDbContext db) : IRequestHandler<Query, Response?>
    {
        public async Task<Response?> Handle(Query request, CancellationToken ct)
        {
            var user = await db.Users.FindAsync([request.UserId], ct);
            if (user is null || user.IsDeleted)
                return null;

            return new Response(user.Id, user.FullName, user.Email, user.PhoneNumber, user.ProfileImageUrl);
        }
    }
}
