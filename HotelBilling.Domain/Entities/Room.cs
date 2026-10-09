using HotelBilling.Domain.Common;
using HotelBilling.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBilling.Domain.Entities
{
    public class Room : BaseEntity
    {
     
            [Required]
            [StringLength(20)]
            public string RoomNumber { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            public string RoomName { get; set; } = string.Empty;

            [Required]
            [StringLength(50)]
            public string RoomType { get; set; } = string.Empty;

            [Required]
            [StringLength(50)]
            public string Floor { get; set; } = string.Empty;

            [Required]
            [StringLength(50)]
            public string BedType { get; set; } = string.Empty;

            public decimal? RoomSize { get; set; }

            public int? CapacityAdults { get; set; }

            public int? CapacityChildren { get; set; }


            // Pricing
            [Range(0, double.MaxValue)]
            public decimal? PricePerNight { get; set; }

            [Range(0, double.MaxValue)]
            public decimal? WeekendPrice { get; set; }

            [Range(0, double.MaxValue)]
            public decimal? ExtraPersonCharge { get; set; }

            [Range(0, 100)]
            public decimal? Discount { get; set; }

            [Range(0, 100)]
            public decimal? Tax { get; set; }


            // Availability & Status
            [StringLength(30)]
            public string Availability { get; set; } = "Available";

            public bool IsActive { get; set; } = true;


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


            // Description
            public string? RoomDescription { get; set; }

            public string? SpecialNotes { get; set; }


            // Housekeeping
            [StringLength(50)]
            public string? HousekeepingStatus { get; set; }

            public DateTime? LastCleanedDate { get; set; }

            [StringLength(50)]
            public string? MaintenanceStatus { get; set; }

            public DateTime? NextMaintenanceDate { get; set; }


            // Images
            public string? ImagePaths { get; set; }
        }
}
