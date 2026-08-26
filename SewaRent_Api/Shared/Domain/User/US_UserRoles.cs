namespace SewaRent_Api.Shared.Domain.User;

public class UserRoleEntity
{
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public RoleEntity Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
