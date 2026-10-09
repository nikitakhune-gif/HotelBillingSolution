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

            // Bill entity uses SubTotal/DiscountAmount/TaxAmount/ServiceCharge/TotalAmount
            builder.Property(x => x.SubTotal)
                   .HasColumnType("decimal(18,2)");

            builder.Property(x => x.DiscountAmount)
                   .HasColumnType("decimal(18,2)");

            builder.Property(x => x.TaxAmount)
                   .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ServiceCharge)
                   .HasColumnType("decimal(18,2)");

            builder.Property(x => x.TotalAmount)
                   .HasColumnType("decimal(18,2)");

            // Relationships
            builder.HasOne(b => b.Customer)
                   .WithMany(c => c.Bills)
                   .HasForeignKey(b => b.CustomerId);

            //builder.HasOne(b => b.Room)
            //       .WithMany(r => r.Bills)
            //       .HasForeignKey(b => b.RoomId);
        }
    }
}