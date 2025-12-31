using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EcoRoute.Models.Entities
{
    public class CreditListing
    {
        [Key]
        public int Id{get; set;}

        public int SellerCompanyId{get; set;}

        public int? BuyerCompanyId{get; set;}
        
        public string CompanyName{get;set;}

        public double CreditsListed{get; set;}

        public double PricePerCredit{get; set;}

        public string Status{get; set;} = "available";

        public DateTime ListedAt{get; set;} = DateTime.Now;
    }
}