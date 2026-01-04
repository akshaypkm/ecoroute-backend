namespace EcoRoute.Models.DTOs
{
    public class MonthlyEmissionStatDto
    {
        public int Year{get; set;}
        public int Month{get; set;}

        public double TotalEmissions{get; set;}
    }
}