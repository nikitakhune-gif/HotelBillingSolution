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
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<HotelBillingSolution.Domain.Entities.HousekeepingTask> HousekeepingTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Ensure AppUser configuration from previous HotelBillingDbContext is applied
            // AppUserConfig is located in HotelBilling.Infrastructure.Data.Configurations
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