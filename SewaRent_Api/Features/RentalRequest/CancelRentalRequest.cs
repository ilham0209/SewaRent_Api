using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.RentalRequest;

public static class CancelRentalRequest
{
    public record Command(Guid Id) : IRequest;

    public class Handler(RentalRequestDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken ct)
        {
            var tenantId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var rentalRequest = await db.RentalRequests
                .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, ct)
                ?? throw new InvalidOperationException("Rental request not found.");

            if (rentalRequest.TenantId != tenantId)
                throw new UnauthorizedAccessException("You can only cancel your own requests.");

            var pendingStatus = await db.RentalRequestStatuses
                .FirstOrDefaultAsync(s => s.Name == "Pending", ct)
                ?? throw new InvalidOperationException("Rental request status not configured.");

            if (rentalRequest.StatusId != pendingStatus.Id)
                throw new InvalidOperationException("Only pending requests can be cancelled.");

            var cancelledStatus = await db.RentalRequestStatuses
                .FirstOrDefaultAsync(s => s.Name == "Cancelled", ct)
                ?? throw new InvalidOperationException("Cancelled status not configured.");

            rentalRequest.StatusId = cancelledStatus.Id;
            rentalRequest.SysUserModified = httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            rentalRequest.SysDateModified = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
        }
    }
}
