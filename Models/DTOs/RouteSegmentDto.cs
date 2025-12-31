namespace EcoRoute.Models.DTOs
{
    public class RouteSegment
    {
        public int Index { get; set; }

        public double DistanceMeters { get; set; }

        public double ElevationGainMeters { get; set; } // only positive gain

        public double StartLat { get; set; }
        public double StartLng { get; set; }

        public double EndLat { get; set; }
        public double EndLng { get; set; }
    }
}