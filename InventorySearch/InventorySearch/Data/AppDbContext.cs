using Microsoft.EntityFrameworkCore;
using Pgvector;

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
            modelBuilder.HasPostgresExtension("vector");  // Ensure pgvector extension

            modelBuilder.Entity<Image>(entity =>
            {
                entity.ToTable("images");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Embedding)                    
                    .HasColumnType("vector(512)");  // No conversion needed with UseVector()
            });
        }
    }
}
