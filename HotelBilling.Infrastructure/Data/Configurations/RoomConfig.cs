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

            // Basic Room Information
            builder.Property(x => x.RoomNumber)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(x => x.RoomName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.RoomType)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.Floor)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.BedType)
                   .IsRequired()
                   .HasMaxLength(50);

            // Room Size
            builder.Property(x => x.RoomSize)
                   .HasPrecision(18, 2);

            // Capacity
            builder.Property(x => x.CapacityAdults);

            builder.Property(x => x.CapacityChildren);

            // Pricing
            builder.Property(x => x.PricePerNight)
                   .HasPrecision(18, 2);

            builder.Property(x => x.WeekendPrice)
                   .HasPrecision(18, 2);

            builder.Property(x => x.ExtraPersonCharge)
                   .HasPrecision(18, 2);

            builder.Property(x => x.Discount)
                   .HasPrecision(18, 2);

            builder.Property(x => x.Tax)
                   .HasPrecision(18, 2);

            // Availability & Status
            builder.Property(x => x.Availability)
                   .IsRequired()
                   .HasMaxLength(30);

            // Description
            builder.Property(x => x.RoomDescription)
                   .HasColumnType("nvarchar(max)");

            builder.Property(x => x.SpecialNotes)
                   .HasColumnType("nvarchar(max)");

            // Housekeeping
            builder.Property(x => x.HousekeepingStatus)
                   .HasMaxLength(50);

            builder.Property(x => x.MaintenanceStatus)
                   .HasMaxLength(50);

            // Images
            builder.Property(x => x.ImagePaths)
                   .HasColumnType("nvarchar(max)");
        }
    }
}