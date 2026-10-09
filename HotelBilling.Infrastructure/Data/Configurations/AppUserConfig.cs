using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HotelBilling.Domain.Entities;

namespace HotelBilling.Infrastructure.Data.Configurations
{
    public class AppUserConfig : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.ToTable("AppUsers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Username)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.Email)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.PasswordHash)
                   .IsRequired();
        }
    }
}
