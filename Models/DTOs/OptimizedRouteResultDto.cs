namespace EcoRoute.Models.DTOs
{
    public class OptimizedRouteResultDto
    {
        public string Polyline { get; set; }
        public List<RouteStopDto> Stops { get; set; }

        public double TotalDistanceMeters { get; set; }
    }

    public class RouteStopDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string StopType { get; set; } // PICKUP | DROP
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int Sequence { get; set; }
    }
}