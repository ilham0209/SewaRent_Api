using SewaRent_Api.Shared.Domain.Example;
using Microsoft.EntityFrameworkCore;

namespace SewaRent_Api.Shared.Infrastructure.Persistence
{
    public class ExampleDbContext : DbContext
    {
        public ExampleDbContext(DbContextOptions<ExampleDbContext> options) : base(options)
        {
        }
        public DbSet<ExampleEntities> ExampleEntities { get; set; } 
        
    }
}
