using SewaRent_Api.Shared.Domain;

namespace SewaRent_Api.Shared.Domain.User;

public class UserEntity : BaseClass
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}

public class RoleEntity : BaseClass
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}

public class UserRoleEntity
{
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public RoleEntity Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
