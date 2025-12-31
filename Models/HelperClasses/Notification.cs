using System.ComponentModel.DataAnnotations.Schema;
using EcoRoute.Models.Entities;

namespace EcoRoute.Models.HelperClasses
{
    public class Notification
    {
        public int Id{get; set;}

        public string Message{get; set;} = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int TargetCompanyId{get; set;}
        [ForeignKey("TargetCompanyId")]
        public Company? Company{get; set;}
    }
}