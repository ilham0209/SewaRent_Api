using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.RentalRequest;

public static class GetByIdRentalRequest
{
    public record Query(Guid Id) : IRequest<Response?>;

    public record Response(
        Guid Id,
        Guid PropertyId,
        string PropertyTitle,
        string PropertyCity,
        string PropertyAddress,
        Guid TenantId,
        string TenantName,
        string TenantEmail,
        string Status,
        string? Message,
        DateTime RequestedAt,
        DateTime? DecisionAt,
        string? DecisionNote);

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Query, Response?>
    {
        public async Task<Response?> Handle(Query request, CancellationToken ct)
        {
            var userId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var rentalRequest = await db.RentalRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(rr => rr.Id == request.Id && !rr.IsDeleted, ct);

            if (rentalRequest is null)
                return null;

            var property = await db.Properties
                .FirstOrDefaultAsync(p => p.Id == rentalRequest.PropertyId, ct);

            var tenant = await db.Users
                .FirstOrDefaultAsync(u => u.Id == rentalRequest.TenantId, ct);

            var status = await db.RentalRequestStatuses
                .FirstOrDefaultAsync(s => s.Id == rentalRequest.StatusId, ct);

            if (property is null || tenant is null || status is null)
                return null;

            var isOwner = property.LandlordId == userId;
            var isTenant = rentalRequest.TenantId == userId;

            if (!isOwner && !isTenant)
                return null;

            return new Response(
                rentalRequest.Id,
                rentalRequest.PropertyId,
                property.Title,
                property.City,
                property.AddressLine1,
                rentalRequest.TenantId,
                tenant.FullName,
                tenant.Email,
                status.Name,
                rentalRequest.Message,
                rentalRequest.RequestedAt,
                rentalRequest.DecisionAt,
                rentalRequest.DecisionNote);
        }
    }
}
