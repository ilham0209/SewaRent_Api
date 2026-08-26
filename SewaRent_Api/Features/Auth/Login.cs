using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;
using SewaRent_Api.Shared.Utilities;

namespace SewaRent_Api.Features.Auth;

public static class Login
{
    public record Command(string Email, string Password) : IRequest<Response>;

    public record Response(string AccessToken, DateTime ExpiresAt, UserDto User);

    public record UserDto(Guid Id, string Name, string Email, string Role);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class Handler(UserDbContext db, IConfiguration configuration)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var user = await db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email, ct)
                ?? throw new InvalidOperationException("Invalid email or password.");

            if (!user.IsActive)
                throw new InvalidOperationException("Account is suspended.");

            if (!JwtHelper.VerifyPassword(request.Password, user.PasswordHash))
                throw new InvalidOperationException("Invalid email or password.");

            var role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "Tenant";
            var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(configuration["Jwt:ExpiryInMinutes"]!));
            var token = JwtHelper.GenerateToken(user.Id, user.Email, role, configuration);

            return new Response(token, expiresAt, new UserDto(user.Id, user.FullName, user.Email, role));
        }
    }
}
