using System.ComponentModel.DataAnnotations;

namespace HotelBilling.Application.DTOs
{
    public class LoginDto
    {
        [Required]
        [Display(Name = "Email or Username")]
        public string Identifier { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
