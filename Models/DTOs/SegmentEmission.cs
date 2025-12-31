namespace EcoRoute.Models.DTOs
{
    public class SegmentEmission
    {
        public int SegmentIndex { get; set; }

        public double DistanceMeters { get; set; }
        public double ElevationGainMeters { get; set; }
        public double EnergyJoules { get; set; }
        public double LitersUsed { get; set; }
        public double KgCO2 { get; set; }
    }
}