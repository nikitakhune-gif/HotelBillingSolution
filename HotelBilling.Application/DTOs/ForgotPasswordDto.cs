using System.ComponentModel.DataAnnotations;

namespace HotelBilling.Application.DTOs
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
