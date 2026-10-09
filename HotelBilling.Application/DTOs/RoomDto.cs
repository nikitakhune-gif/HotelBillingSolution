using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBilling.Application.DTOs.Room
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        public decimal? PricePerNight { get; set; }
        public decimal? WeekendPrice { get; set; }
        public decimal? ExtraPersonCharge { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Tax { get; set; }
        public string Availability { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int? CapacityAdults { get; set; }
        public int? CapacityChildren { get; set; }
        public string? BedType { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? HousekeepingStatus { get; set; }
        public string? ImagePaths { get; set; }
        // Derived / additional fields used by the Edit view
        public decimal? RoomSize { get; set; }
        public DateTime? LastCleanedDate { get; set; }
        public string? MaintenanceStatus { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public List<string>? Amenities { get; set; }
        public IEnumerable<string>? ImageUrls {
            get {
                if (string.IsNullOrWhiteSpace(ImagePaths)) return null;
                return ImagePaths.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            }
        }
        public string? RoomDescription { get; set; }
        public string? SpecialNotes { get; set; }
        // Amenities
        public bool HasWifi { get; set; }
        public bool HasAirConditioning { get; set; }
        public bool HasTelevision { get; set; }
        public bool HasMiniBar { get; set; }
        public bool HasTelephone { get; set; }
        public bool HasRoomService { get; set; }
        public bool HasBalcony { get; set; }
        public bool HasSafeLocker { get; set; }
        public bool HasCoffeeMachine { get; set; }
        public bool HasWorkDesk { get; set; }
        public bool HasHairDryer { get; set; }
        public bool HasWardrobe { get; set; }
        public bool HasIron { get; set; }
        public bool HasBathtub { get; set; }
        public bool HasHeating { get; set; }
        public bool HasSmartTv { get; set; }
    }

    public class CreateRoomDto
    {
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        // Pricing
        public decimal? PricePerNight { get; set; }
        public decimal? WeekendPrice { get; set; }
        public decimal? ExtraPersonCharge { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Tax { get; set; }

        // Capacity & layout
        public string? BedType { get; set; }
        public decimal? RoomSize { get; set; }
        public int? CapacityAdults { get; set; }
        public int? CapacityChildren { get; set; }

        // Availability & housekeeping
        public string? Availability { get; set; }
        public bool IsActive { get; set; } = true;

        // Images / description
        public string? ImagePaths { get; set; }
        public string? RoomDescription { get; set; }
        public string? SpecialNotes { get; set; }
        // Housekeeping & maintenance
        public string? HousekeepingStatus { get; set; }
        public DateTime? LastCleanedDate { get; set; }
        public string? MaintenanceStatus { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }

        // Amenities
        public bool HasWifi { get; set; }
        public bool HasAirConditioning { get; set; }
        public bool HasTelevision { get; set; }
        public bool HasMiniBar { get; set; }
        public bool HasTelephone { get; set; }
        public bool HasRoomService { get; set; }
        public bool HasBalcony { get; set; }
        public bool HasSafeLocker { get; set; }
        public bool HasCoffeeMachine { get; set; }
        public bool HasWorkDesk { get; set; }
        public bool HasHairDryer { get; set; }
        public bool HasWardrobe { get; set; }
        public bool HasIron { get; set; }
        public bool HasBathtub { get; set; }
        public bool HasHeating { get; set; }
        public bool HasSmartTv { get; set; }
    }

    public class UpdateRoomDto : CreateRoomDto
    {
        public int Id { get; set; }
    }
}
