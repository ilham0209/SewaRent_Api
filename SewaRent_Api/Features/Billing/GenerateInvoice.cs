using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Domain.Billing;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Billing;

public static class GenerateInvoice
{
    public record Command(Guid RentalRequestId, int BillingPeriodMonth, int BillingPeriodYear) : IRequest<Response>;

    public record Response(Guid Id, string InvoiceNumber, decimal TotalAmount, DateTime DueDate);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.RentalRequestId).NotEmpty();
            RuleFor(x => x.BillingPeriodMonth).InclusiveBetween(1, 12);
            RuleFor(x => x.BillingPeriodYear).GreaterThan(2020);
        }
    }

    public class Handler(SewaRentDbContext db) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var rentalRequest = await db.RentalRequests
                .FirstOrDefaultAsync(r => r.Id == request.RentalRequestId && !r.IsDeleted, ct)
                ?? throw new InvalidOperationException("Rental request not found.");

            var property = await db.Properties
                .FirstOrDefaultAsync(p => p.Id == rentalRequest.PropertyId, ct)
                ?? throw new InvalidOperationException("Property not found.");

            var landlord = await db.Users
                .FirstOrDefaultAsync(u => u.Id == property.LandlordId, ct)
                ?? throw new InvalidOperationException("Landlord not found.");

            var existingInvoice = await db.Invoices
                .FirstOrDefaultAsync(i => i.RentalRequestId == request.RentalRequestId
                    && i.BillingPeriodMonth == request.BillingPeriodMonth
                    && i.BillingPeriodYear == request.BillingPeriodYear
                    && !i.IsDeleted, ct);

            if (existingInvoice is not null)
                throw new InvalidOperationException("An invoice for this billing period already exists.");

            var invoiceNumber = await GenerateInvoiceNumber(db, request.BillingPeriodYear, request.BillingPeriodMonth, ct);
            var dueDate = new DateTime(request.BillingPeriodYear, request.BillingPeriodMonth, 28, 0, 0, 0, DateTimeKind.Utc);

            var invoice = new InvoiceEntity
            {
                RentalRequestId = request.RentalRequestId,
                InvoiceNumber = invoiceNumber,
                BillingPeriodMonth = request.BillingPeriodMonth,
                BillingPeriodYear = request.BillingPeriodYear,
                RentAmount = property.MonthlyRent,
                TotalAmount = property.MonthlyRent,
                Status = "Unpaid",
                DueDate = dueDate,
                BankNameSnapshot = landlord.BankName,
                BankAccountNumberSnapshot = landlord.BankAccountNumber,
                SysUserCreated = "System",
                SysDateCreated = DateTime.UtcNow
            };

            db.Invoices.Add(invoice);
            await db.SaveChangesAsync(ct);

            return new Response(invoice.Id, invoice.InvoiceNumber, invoice.TotalAmount, invoice.DueDate);
        }

        private static async Task<string> GenerateInvoiceNumber(SewaRentDbContext db, int year, int month, CancellationToken ct)
        {
            var prefix = $"INV-{year}-{month:D2}-";
            var lastInvoice = await db.Invoices
                .Where(i => i.InvoiceNumber.StartsWith(prefix) && !i.IsDeleted)
                .OrderByDescending(i => i.InvoiceNumber)
                .Select(i => i.InvoiceNumber)
                .FirstOrDefaultAsync(ct);

            var sequence = 1;
            if (lastInvoice is not null)
            {
                var lastSequence = int.Parse(lastInvoice.Substring(lastInvoice.LastIndexOf('-') + 1));
                sequence = lastSequence + 1;
            }

            return $"{prefix}{sequence:D4}";
        }
    }
}
