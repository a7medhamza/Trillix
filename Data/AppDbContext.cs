using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Trillix.Common;
using Trillix.Modules.Catalog;
using Trillix.Modules.Identity;

namespace Trillix.Data
{
    // Data/AppDbContext.cs


    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Admins> Admins => Set<Admins>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
 

        protected override void OnModelCreating(ModelBuilder b)
        {
           

        // inside OnModelCreating
        b.Entity<Admins>(e =>
{
    e.Property(a => a.Email).HasMaxLength(256).IsRequired();
        e.Property(a => a.FullName).HasMaxLength(200).IsRequired();
        e.Property(a => a.PasswordHash).HasMaxLength(255).IsRequired();
        e.HasIndex(a => a.Email).IsUnique();
    });

            b.Entity<Product>(e =>
            {
                e.Property(p => p.Name).HasMaxLength(200).IsRequired();
                e.Property(p => p.Slug).HasMaxLength(200).IsRequired();
                e.Property(p => p.Sku).HasMaxLength(50).IsRequired();
                e.Property(p => p.Price).HasPrecision(18, 2);
                e.Property(p => p.RowVersion).IsRowVersion();
                e.HasIndex(p => p.Sku).IsUnique();
                e.HasIndex(p => p.Slug).IsUnique();
            });

            b.Entity<ProductImage>(e =>
            {
                e.Property(i => i.FileName).HasMaxLength(100).IsRequired();
                e.Property(i => i.OriginalFileName).HasMaxLength(260).IsRequired();
                e.Property(i => i.ContentType).HasMaxLength(50).IsRequired();
                e.HasOne(i => i.Product).WithMany(p => p.Images)
                 .HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                else if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = DateTime.UtcNow;

            return base.SaveChangesAsync(ct);
        }
    }
}
