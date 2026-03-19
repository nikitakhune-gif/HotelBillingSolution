using System;
using System.ComponentModel.DataAnnotations;

namespace HotelBilling.Web.Models
{
    public class BillingViewModel
    {
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Room Number is required")]
        public int RoomNumber { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, 100000)]
        public decimal RoomCharge { get; set; }

        [Range(0, 100000)]
        public decimal FoodCharge { get; set; }

        [Range(0, 100000)]
        public decimal OtherCharges { get; set; }

        // Calculated Property (UI only)
        public decimal TotalAmount => RoomCharge + FoodCharge + OtherCharges;
    }
}