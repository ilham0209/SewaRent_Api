using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Domain.Billing;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Billing;

public static class AcceptPaymentInvoice
{
    public record Command(Guid Id) : IRequest<Response>;

    public record Response(Guid InvoiceId, Guid ReceiptId, string ReceiptNumber);

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
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
                throw new UnauthorizedAccessException("You can only accept payments for your own properties.");

            if (invoice.Status != "PaymentClaimed")
                throw new InvalidOperationException("Only claimed payments can be accepted.");

            var existingReceipt = await db.Receipts
                .FirstOrDefaultAsync(r => r.InvoiceId == invoice.Id && !r.IsDeleted, ct);

            if (existingReceipt is not null)
                throw new InvalidOperationException("A receipt has already been generated for this invoice.");

            invoice.Status = "Paid";
            invoice.PaidDate = DateTime.UtcNow;
            invoice.SysUserModified = httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            invoice.SysDateModified = DateTime.UtcNow;

            var receiptNumber = await GenerateReceiptNumber(db, ct);
            var receipt = new ReceiptEntity
            {
                InvoiceId = invoice.Id,
                ReceiptNumber = receiptNumber,
                IssuedDate = DateTime.UtcNow,
                SysUserCreated = httpContextAccessor.HttpContext!.User
                    .FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                SysDateCreated = DateTime.UtcNow
            };

            db.Receipts.Add(receipt);
            await db.SaveChangesAsync(ct);

            return new Response(invoice.Id, receipt.Id, receipt.ReceiptNumber);
        }

        private static async Task<string> GenerateReceiptNumber(SewaRentDbContext db, CancellationToken ct)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"RCP-{year}-";
            var lastReceipt = await db.Receipts
                .Where(r => r.ReceiptNumber.StartsWith(prefix) && !r.IsDeleted)
                .OrderByDescending(r => r.ReceiptNumber)
                .Select(r => r.ReceiptNumber)
                .FirstOrDefaultAsync(ct);

            var sequence = 1;
            if (lastReceipt is not null)
            {
                var lastSequence = int.Parse(lastReceipt.Substring(lastReceipt.LastIndexOf('-') + 1));
                sequence = lastSequence + 1;
            }

            return $"{prefix}{sequence:D4}";
        }
    }
}
