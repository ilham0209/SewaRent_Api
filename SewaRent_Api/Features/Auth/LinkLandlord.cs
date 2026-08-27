using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Auth;

public static class LinkLandlord
{
    public record Command(string LandlordCode) : IRequest<Response>;

    public record Response(Guid LandlordId);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.LandlordCode).NotEmpty().MaximumLength(20);
        }
    }

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var tenantId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var tenantRole = httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Role);

            if (tenantRole != "Tenant")
                throw new InvalidOperationException("Only tenants can link to a landlord.");

            var tenant = await db.Users.FindAsync([tenantId], ct)
                ?? throw new InvalidOperationException("User not found.");

            if (tenant.LandlordId.HasValue)
                throw new InvalidOperationException("You are already linked to a landlord.");

            var landlord = await db.Users
                .FirstOrDefaultAsync(u => u.LandlordCode == request.LandlordCode && !u.IsDeleted, ct)
                ?? throw new InvalidOperationException("Invalid landlord code.");

            tenant.LandlordId = landlord.Id;
            tenant.SysUserModified = tenant.Email;
            tenant.SysDateModified = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);

            return new Response(landlord.Id);
        }
    }
}
