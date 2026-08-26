using SewaRent_Api.Shared.Domain.Car;
using Microsoft.EntityFrameworkCore;

namespace SewaRent_Api.Shared.Infrastructure.Persistence
{
    public class CarDbContext : DbContext
    {
        public CarDbContext(DbContextOptions<CarDbContext> options) : base(options)
        {
        }
        public DbSet<CarEntities> CarEntities { get; set; }
       
    }
    
}
