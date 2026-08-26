namespace SewaRent_Api.Shared.Domain.Favourite;

public class FavouriteEntity
{
    public Guid UserId { get; set; }
    public Guid PropertyId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
