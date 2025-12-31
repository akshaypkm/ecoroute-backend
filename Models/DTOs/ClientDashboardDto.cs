using System.ComponentModel.DataAnnotations;
using EcoRoute.Models.Entities;

namespace EcoRoute.Models
{
    public class ClientDashboardDto
    {
        public string CompanyCode{get; set;}

        public int Shipments{get; set;}

        public double CompanyCredits{get; set;}

        public double CreditMarketPrice{get; set;}

        public double TotalEmissions{get; set;}

        public double ForecastedEmissions{get; set;} //this is forecast of emissions for the remaining months

        public double TotalForecastedEmissions{get; set;} //this is the total year's forecast
 
        public double EmissionsSaved{get; set;}

        public double CompanyEmissionBudget{get; set;}

        public double[] GraphData{get; set;} = new double[12];

        public double RemainingCredits{get; set;}

    }
}