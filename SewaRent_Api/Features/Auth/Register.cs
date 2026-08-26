using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Domain.User;
using SewaRent_Api.Shared.Infrastructure.Persistence;
using SewaRent_Api.Shared.Utilities;

namespace SewaRent_Api.Features.Auth;

public static class Register
{
    public record Command(
        string FullName,
        string Email,
        string Password,
        string? PhoneNumber,
        string Role) : IRequest<Response>;

    public record Response(Guid Id, string FullName, string Email, string Role);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.Role).NotEmpty().Must(r => r == "Tenant" || r == "Landlord")
                .WithMessage("Role must be Tenant or Landlord.");
        }
    }

    public class Handler(UserDbContext db)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            if (await db.Users.AnyAsync(u => u.Email == request.Email, ct))
                throw new InvalidOperationException("Email is already registered.");

            var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == request.Role, ct)
                ?? throw new InvalidOperationException($"Role '{request.Role}' does not exist.");

            var user = new UserEntity
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = JwtHelper.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                SysUserCreated = request.Email,
                SysDateCreated = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(ct);

            var userRole = new UserRoleEntity
            {
                UserId = user.Id,
                RoleId = role.Id,
                CreatedAt = DateTime.UtcNow
            };

            db.UserRoles.Add(userRole);
            await db.SaveChangesAsync(ct);

            return new Response(user.Id, user.FullName, user.Email, role.Name);
        }
    }
}
