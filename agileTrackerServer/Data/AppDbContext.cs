using Microsoft.EntityFrameworkCore;

namespace agileTrackerServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Exemplo: DbSet<Project> Projects { get; set; }
    }
}