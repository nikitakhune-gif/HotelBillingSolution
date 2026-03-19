//using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

namespace HotelBilling.Web.Models
{
    public class CustomerViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
    }
}