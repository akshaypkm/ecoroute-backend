using System.ComponentModel.DataAnnotations;

namespace EcoRoute.Models
{
    public class UserSignUpDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string CompanyName { get; set; }

        public string CompanySector { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
