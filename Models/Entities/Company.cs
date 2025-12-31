using System.ComponentModel.DataAnnotations;

namespace EcoRoute.Models.Entities
{
    public class Company
    {
        [Key]
        public int Id{get; set;}

        public string CompanyCode{get; set;} = string.Empty;

        [Required]
        public string CompanyName{get; set;} = string.Empty;

        public double CompanyEmissionBudget{get; set;}

        public double CompanyCredits{get; set;}

        public double RemainingCredits{get; set;}

        public DateTime CompanyCreatedAt{get; set;} = DateTime.Now;

        public string? CompanySector{get; set;}

        public List<Order> OrderList{get; set;} = new List<Order>();
    }
}