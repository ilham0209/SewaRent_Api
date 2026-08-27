using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Profile;

public static class UpdateBankDetails
{
    public record Command(string? BankName, string? BankAccountNumber) : IRequest<Response>;

    public record Response(Guid Id, string? BankName, string? BankAccountNumber);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.BankName).MaximumLength(100);
            RuleFor(x => x.BankAccountNumber).MaximumLength(50);
        }
    }

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var userId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var role = httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Role);

            if (role != "Landlord")
                throw new InvalidOperationException("Only landlords can update bank details.");

            var user = await db.Users.FindAsync([userId], ct)
                ?? throw new InvalidOperationException("User not found.");

            user.BankName = request.BankName;
            user.BankAccountNumber = request.BankAccountNumber;
            user.SysUserModified = user.Email;
            user.SysDateModified = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);

            return new Response(user.Id, user.BankName, user.BankAccountNumber);
        }
    }
}
