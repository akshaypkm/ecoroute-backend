using System.ComponentModel.DataAnnotations;
namespace EcoRoute.Models.DTOs;
public class SendOtpDto
    {
        [Required]
        public string Email { get; set; }
    }