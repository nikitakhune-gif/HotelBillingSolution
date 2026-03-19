using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HotelBilling.Domain.Entities;

namespace HotelBilling.Infrastructure.Data.Configurations
{
    public class CustomerConfig : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Email)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.Phone)
                   .HasMaxLength(15);
        }
    }
}