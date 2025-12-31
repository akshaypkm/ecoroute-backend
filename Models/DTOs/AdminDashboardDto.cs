namespace EcoRoute.Models.DTOs
{
    public class AdminDashboardDto
    {
        public double TotalCO2Emissions{get; set;}

        public double TotalShipments{get; set;}

        public double TotalOrdersForReview{get; set;}

        public double TotalEmissionsSaved{get; set;}

        public double[] GraphData{get; set;} = new double[12];

        public int SoFarReviewedCount{get; set;}
    }
}