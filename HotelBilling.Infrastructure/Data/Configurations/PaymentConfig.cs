using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HotelBilling.Domain.Entities;

namespace HotelBilling.Infrastructure.Data.Configurations
{
    public class PaymentConfig : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.PaymentCode)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(p => p.PaymentId)
                   .HasMaxLength(100);

            builder.Property(p => p.TransactionId)
                   .HasMaxLength(50);

            builder.Property(p => p.ReferenceNumber)
                   .HasMaxLength(50);

            builder.Property(p => p.Notes)
                   .HasMaxLength(500);

            // Prevent multiple cascade paths by disabling cascade delete from Payments -> Bill and -> Customer
            builder.HasOne(p => p.Bill)
                   .WithMany()
                   .HasForeignKey(p => p.BillId)
                   .OnDelete(DeleteBehavior.NoAction)
                   .IsRequired();

            builder.HasOne(p => p.Customer)
                   .WithMany()
                   .HasForeignKey(p => p.CustomerId)
                   .OnDelete(DeleteBehavior.NoAction)
                   .IsRequired();
        }
    }
}
