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
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                
                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .IsRequired()
                    .HasMaxLength(255);
                entity.HasIndex(e => e.Name).IsUnique();
                
                entity.Property(e => e.Embedding)
                    .HasColumnName("embedding")
                    .HasColumnType("vector(512)");
                
                entity.Property(e => e.Filename)
                    .HasColumnName("filename");
                
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");
            });
        }
    }
}
