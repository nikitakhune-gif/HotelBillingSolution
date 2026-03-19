using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HotelBilling.Domain.Entities;

namespace HotelBilling.Infrastructure.Data.Configurations
{
    public class BillConfig : IEntityTypeConfiguration<Bill>
    {
        public void Configure(EntityTypeBuilder<Bill> builder)
        {
            builder.ToTable("Bills");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                   .HasColumnType("decimal(18,2)");

            // Relationships
            builder.HasOne(b => b.Customer)
                   .WithMany(c => c.Bills)
                   .HasForeignKey(b => b.CustomerId);

            builder.HasOne(b => b.Room)
                   .WithMany(r => r.Bills)
                   .HasForeignKey(b => b.RoomId);
        }
    }
}