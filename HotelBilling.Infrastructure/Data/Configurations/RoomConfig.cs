using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HotelBilling.Domain.Entities;

namespace HotelBilling.Infrastructure.Data.Configurations
{
    public class RoomConfig : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.ToTable("Rooms");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RoomNumber)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(x => x.Price)
                   .HasColumnType("decimal(18,2)");
        }
    }
}