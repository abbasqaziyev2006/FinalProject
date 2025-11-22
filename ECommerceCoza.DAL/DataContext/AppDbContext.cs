using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCoza.DAL.DataContext
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Color> Colors { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;

        public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
        public DbSet<Bio> Bios { get; set; } = null!;
        public DbSet<Social> Socials { get; set; } = null!;
        public DbSet<WishlistItem> WishlistItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public DbSet<DiscountCode> DiscountCodes { get; set; } = null!;
        public DbSet<Address> Addresses { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.BasePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.Price)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            UpdateTimeStamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimeStamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimeStamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is TimeStample &&
                (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var timeStamp = (TimeStample)entry.Entity;

                if (entry.State == EntityState.Added)
                    timeStamp.CreatedAt = DateTime.UtcNow.AddHours(4);

                timeStamp.UpdatedAt = DateTime.UtcNow.AddHours(4);
            }
        }

    }
}
