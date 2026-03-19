using Microsoft.EntityFrameworkCore;
using HotelBilling.Domain.Entities;
using HotelBilling.Domain.Common;

namespace HotelBilling.Infrastructure.Data.DbContext
{
    public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Bill> Bills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        // Auto handle CreatedDate / UpdatedDate
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedDate = DateTime.UtcNow;

                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedDate = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}