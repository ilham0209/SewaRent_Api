using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Domain.RentalRequest;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.RentalRequest;

public static class CreateRentalRequest
{
    public record Command(Guid PropertyId, string? Message) : IRequest<Response>;

    public record Response(Guid Id, Guid PropertyId, Guid StatusId, DateTime RequestedAt);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.PropertyId).NotEmpty();
            RuleFor(x => x.Message).MaximumLength(1000);
        }
    }

    public class Handler(SewaRentDbContext db,
        IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var tenantId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var property = await db.Properties
                .FirstOrDefaultAsync(p => p.Id == request.PropertyId && !p.IsDeleted, ct)
                ?? throw new InvalidOperationException("Property not found.");

            if (property.LandlordId == tenantId)
                throw new InvalidOperationException("You cannot submit a rental request for your own property.");

            if (property.AvailabilityStatus != "Available")
                throw new InvalidOperationException("Property is not available for rent.");

            var pendingStatus = await db.RentalRequestStatuses
                .FirstOrDefaultAsync(s => s.Name == "Pending", ct)
                ?? throw new InvalidOperationException("Rental request status not configured.");

            var hasPendingRequest = await db.RentalRequests
                .AnyAsync(r => r.TenantId == tenantId
                    && r.PropertyId == request.PropertyId
                    && r.StatusId == pendingStatus.Id
                    && !r.IsDeleted, ct);

            if (hasPendingRequest)
                throw new InvalidOperationException("You already have a pending request for this property.");

            var rentalRequest = new Shared.Domain.RentalRequest.RentalRequestEntity
            {
                PropertyId = request.PropertyId,
                TenantId = tenantId,
                StatusId = pendingStatus.Id,
                Message = request.Message,
                RequestedAt = DateTime.UtcNow,
                SysUserCreated = httpContextAccessor.HttpContext!.User
                    .FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                SysDateCreated = DateTime.UtcNow
            };

            db.RentalRequests.Add(rentalRequest);
            await db.SaveChangesAsync(ct);

            return new Response(rentalRequest.Id, rentalRequest.PropertyId,
                rentalRequest.StatusId, rentalRequest.RequestedAt);
        }
    }
}
