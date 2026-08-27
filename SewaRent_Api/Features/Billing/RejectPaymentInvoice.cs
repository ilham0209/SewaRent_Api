using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Billing;

public static class RejectPaymentInvoice
{
    public record Command(Guid Id, string Reason) : IRequest;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        }
    }

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken ct)
        {
            var landlordId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var invoice = await db.Invoices
                .FirstOrDefaultAsync(i => i.Id == request.Id && !i.IsDeleted, ct)
                ?? throw new InvalidOperationException("Invoice not found.");

            var rentalRequest = await db.RentalRequests
                .FirstOrDefaultAsync(r => r.Id == invoice.RentalRequestId, ct)
                ?? throw new InvalidOperationException("Rental request not found.");

            var property = await db.Properties
                .FirstOrDefaultAsync(p => p.Id == rentalRequest.PropertyId, ct)
                ?? throw new InvalidOperationException("Property not found.");

            if (property.LandlordId != landlordId)
                throw new UnauthorizedAccessException("You can only reject payments for your own properties.");

            if (invoice.Status != "PaymentClaimed")
                throw new InvalidOperationException("Only claimed payments can be rejected.");

            invoice.Status = "Unpaid";
            invoice.RejectReason = request.Reason;
            invoice.SysUserModified = httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            invoice.SysDateModified = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
        }
    }
}
