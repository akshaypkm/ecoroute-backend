using System.ComponentModel.DataAnnotations;

namespace EcoRoute.Models.Entities
{
    public class User
    {
        [Key]
        public int Id{get; set;}

        [Required]
        public string UserId{get; set;} = string.Empty;

        [Required]
        public string CompanyName{get; set;} = string.Empty;

        [Required]
        [EmailAddress]
        public string Email{get; set;} = string.Empty;

        [Required]
        public string Role{get; set;} = string.Empty;

        [Required]
        public string PasswordHash{get; set;} = string.Empty;

        public DateTime UserCreatedAt{get; set;} = DateTime.Now;
    }
}