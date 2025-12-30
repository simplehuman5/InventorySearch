using Microsoft.EntityFrameworkCore;

namespace InventorySearch.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Image> Images => Set<Image>();
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //note to self, skip fluent notation as it is bulkier
            modelBuilder.Entity<Image>(
                entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.HasIndex(e => e.Name).IsUnique();
                    entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                    entity.Property(e => e.Embedding).IsRequired().HasColumnType("vector(512)");
                }
            );
        }

    }
}
