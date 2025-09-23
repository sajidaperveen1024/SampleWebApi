using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SampleWebApi.Model;

namespace SampleWebApi.Db
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProductConfiguration());

            // Seed (optional)
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Keyboard", Price = 29.99m, InStock = true },
                new Product { Id = 2, Name = "Mouse", Price = 14.99m, InStock = true },
                new Product { Id = 3, Name = "Monitor", Price = 199.00m, InStock = false },

                new Product { Id = 4, Name = "Test", Price = 14.99m, InStock = true },
                new Product { Id = 5, Name = "Test1", Price = 199.00m, InStock = false }
            );
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Product p)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            p.CreatedUtc = utcNow;
                            p.UpdatedUtc = utcNow;
                            break;

                        case EntityState.Modified:
                            // Keep CreatedUtc unchanged
                            entry.Property(nameof(Product.CreatedUtc)).IsModified = false;
                            p.UpdatedUtc = utcNow;
                            break;

                        case EntityState.Deleted:
                            // Soft delete → convert to update
                            entry.State = EntityState.Modified;
                            p.IsDeleted = true;
                            p.UpdatedUtc = utcNow;
                            break;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Product fluent configuration kept separate for clarity/testability.
    /// </summary>
    internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> b)
        {
            b.ToTable("Products");

            b.HasKey(p => p.Id);

            b.Property(p => p.Name)
             .IsRequired()
             .HasMaxLength(200);

            b.Property(p => p.Price)
             .HasPrecision(18, 2);

            // RowVersion for optimistic concurrency & ETag
            b.Property(p => p.RowVersion)
             .IsRowVersion()
             .IsConcurrencyToken();

            // Global filter to hide soft-deleted rows
            b.HasQueryFilter(p => !p.IsDeleted);

            // Helpful indexes for your queries
            b.HasIndex(p => p.Name);
            b.HasIndex(p => p.Price);

            // Seed has to include all required properties (timestamps set at runtime via SaveChanges)
            b.Property(p => p.CreatedUtc).HasColumnType("datetime2");
            b.Property(p => p.UpdatedUtc).HasColumnType("datetime2");
        }
    }
}
