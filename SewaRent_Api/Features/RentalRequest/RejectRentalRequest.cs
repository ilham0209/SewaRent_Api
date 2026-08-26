using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.RentalRequest;

public static class RejectRentalRequest
{
    public record Command(Guid Id, string? DecisionNote) : IRequest;

    public class Handler(RentalRequestDbContext rentalDb, PropertyDbContext propertyDb,
        IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken ct)
        {
            var landlordId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var rentalRequest = await rentalDb.RentalRequests
                .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, ct)
                ?? throw new InvalidOperationException("Rental request not found.");

            var property = await propertyDb.Properties
                .FirstOrDefaultAsync(p => p.Id == rentalRequest.PropertyId, ct)
                ?? throw new InvalidOperationException("Property not found.");

            if (property.LandlordId != landlordId)
                throw new UnauthorizedAccessException("You can only reject requests for your own properties.");

            var pendingStatus = await rentalDb.RentalRequestStatuses
                .FirstOrDefaultAsync(s => s.Name == "Pending", ct)
                ?? throw new InvalidOperationException("Rental request status not configured.");

            if (rentalRequest.StatusId != pendingStatus.Id)
                throw new InvalidOperationException("Only pending requests can be rejected.");

            var rejectedStatus = await rentalDb.RentalRequestStatuses
                .FirstOrDefaultAsync(s => s.Name == "Rejected", ct)
                ?? throw new InvalidOperationException("Rejected status not configured.");

            rentalRequest.StatusId = rejectedStatus.Id;
            rentalRequest.DecisionAt = DateTime.UtcNow;
            rentalRequest.DecisionNote = request.DecisionNote;
            rentalRequest.SysUserModified = httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            rentalRequest.SysDateModified = DateTime.UtcNow;

            await rentalDb.SaveChangesAsync(ct);
        }
    }
}
