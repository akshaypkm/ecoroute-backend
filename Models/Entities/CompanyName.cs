using System.ComponentModel.DataAnnotations;

namespace EcoRoute.Models.Entities
{
    public class CompanyName
    {
        public int Id{get; set;}

        public string CompName{get; set;} = string.Empty;
    }
}