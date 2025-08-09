using Microsoft.EntityFrameworkCore;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<VolunteerTask> VolunteerTasks => Set<VolunteerTask>();
        public DbSet<Parent> Parents => Set<Parent>();
        public DbSet<Teacher> Teachers => Set<Teacher>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VolunteerTask>()
                .Property(p => p.RowVersion)
                .IsRowVersion();
        }
    }
}
