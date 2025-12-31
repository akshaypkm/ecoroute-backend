namespace EcoRoute.Models.DTOs
{
    public class ImproviseShipmentGroupDto
    {
        public List<OrderDto> Orders { get; set; }

        public string OptimizedGroupPolyline { get; set; }

        public string TransportVehicle { get; set; }

        public int CombinedOrderTotalItems { get; set; }

        public double CombinedOrderWeightKg { get; set; }

        public double ApproxCombinedVolume { get; set; }

        public double OptimizedDistance{get; set;}

        public List<RouteStopDto> RouteStops { get; set; }

        public double TotalShipmentCO2Emissions{get; set;}

        public int TransportCompanyId{get; set;}

    }
}