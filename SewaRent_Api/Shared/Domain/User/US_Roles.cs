using SewaRent_Api.Shared.Domain;

namespace SewaRent_Api.Shared.Domain.User;

public class RoleEntity : BaseClass
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}
