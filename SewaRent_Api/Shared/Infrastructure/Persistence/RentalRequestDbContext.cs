using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Domain.RentalRequest;

namespace SewaRent_Api.Shared.Infrastructure.Persistence;

public class RentalRequestDbContext(DbContextOptions<RentalRequestDbContext> options) : DbContext(options)
{
    public DbSet<RentalRequestEntity> RentalRequests => Set<RentalRequestEntity>();
    public DbSet<RentalRequestStatusEntity> RentalRequestStatuses => Set<RentalRequestStatusEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RentalRequestEntity>(e =>
        {
            e.ToTable("RR_RentalRequests");
            e.HasKey(x => x.Id);
            e.Property(x => x.Message).HasMaxLength(1000);
            e.Property(x => x.DecisionNote).HasMaxLength(1000);
            e.HasIndex(x => x.TenantId);
            e.HasIndex(x => x.PropertyId);
            e.HasIndex(x => x.StatusId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<RentalRequestStatusEntity>(e =>
        {
            e.ToTable("RR_RentalRequestStatuses");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(50);
            e.Property(x => x.Description).HasMaxLength(255);
            e.HasIndex(x => x.Name).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}
