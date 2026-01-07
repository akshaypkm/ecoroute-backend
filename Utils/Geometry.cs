using EcoRoute.Models;

namespace EcoRoute.Services
{
    public class Geometry
    {

        // CONSTANTS
        private static readonly double R = 6371000.0;

        internal static (List<DensePoint> densePoints, RoutePoint OriginRP, RoutePoint DestinationRP) Densify(List<RoutePoint> route, double maxSegmentMeters)
        {
            var outPts = new List<DensePoint>();

            if(route == null || route.Count == 0) return (outPts, null, null);

            outPts.Add(new DensePoint
            {
                Lat = route[0].Lat,
                Lng = route[0].Lng
            });

            RoutePoint OriginRP = new RoutePoint()
            {
                Lat = route[0].Lat,
                Lng = route[0].Lng
            };

            RoutePoint DestinationRP = new RoutePoint()
            {
                Lat = route[route.Count-1].Lat,
                Lng = route[route.Count - 1].Lng
            }; 

            for(int i = 0; i<route.Count - 1; i++)
            {
                var a = route[i];
                var b = route[i+1];

                double segLen = HaversineMeters(a.Lat, a.Lng, b.Lat, b.Lng);
                if(segLen <= maxSegmentMeters)
                {
                    outPts.Add(new DensePoint
                    {
                        Lat = b.Lat,
                        Lng = b.Lng
                    });
                }
                else
                {
                    int steps = (int) Math.Ceiling(segLen / maxSegmentMeters);
                    for(int s = 1; s<=steps; s++)
                    {
                        double f = (double) s/(double) steps;
                        var ip = IntermediateGreatCircle((a.Lat, a.Lng), (b.Lat, b.Lng), f);
                        outPts.Add(new DensePoint
                        {   
                            Lat = ip.Lat,
                            Lng = ip.Lng
                        });
                    }
                }
            }

            for(int i = 0; i< outPts.Count - 1; i++)
            {
                outPts[i].SegmentMeters = HaversineMeters(outPts[i].Lat, outPts[i].Lng, outPts[i + 1].Lat, outPts[i + 1].Lng);
            }
            if (outPts.Count > 0) outPts[outPts.Count - 1].SegmentMeters = 0.0;

            return (outPts, OriginRP, DestinationRP);
        }

        internal static double HaversineMeters(double lat1, double lng1, double lat2, double lng2)
        {
            double φ1 = ToRad(lat1), φ2 = ToRad(lat2);
            double dφ = ToRad(lat2 - lat1), dλ = ToRad(lng2 - lng1);
            double a = Math.Sin(dφ / 2.0) * Math.Sin(dφ / 2.0) +
                       Math.Cos(φ1) * Math.Cos(φ2) * Math.Sin(dλ / 2.0) * Math.Sin(dλ / 2.0);
            return 2.0 * R * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double ToRad(double deg) => deg * Math.PI / 180.0;
        private static double ToDeg(double rad) => rad * 180.0 / Math.PI;

        private static (double Lat, double Lng) IntermediateGreatCircle(
            (double Lat, double Lng) start,
            (double Lat, double Lng) end,
            double fraction)
        {
            double lat1 = ToRad(start.Lat);
            double lon1 = ToRad(start.Lng);
            double lat2 = ToRad(end.Lat);
            double lon2 = ToRad(end.Lng);

            double cosLat1 = Math.Cos(lat1);
            double cosLat2 = Math.Cos(lat2);
            double sinLat1 = Math.Sin(lat1);
            double sinLat2 = Math.Sin(lat2);

            double delta = 2.0 * Math.Asin(
                Math.Min(
                    1.0,
                    Math.Sqrt(
                        Math.Pow(Math.Sin((lat2 - lat1) / 2.0), 2) +
                        cosLat1 * cosLat2 *
                        Math.Pow(Math.Sin((lon2 - lon1) / 2.0), 2)
                    )
                )
            );

            if (delta < 1e-12)
                return (start.Lat, start.Lng);

            double factorA = Math.Sin((1 - fraction) * delta) / Math.Sin(delta);
            double factorB = Math.Sin(fraction * delta) / Math.Sin(delta);

            double x =
                factorA * cosLat1 * Math.Cos(lon1) +
                factorB * cosLat2 * Math.Cos(lon2);

            double y =
                factorA * cosLat1 * Math.Sin(lon1) +
                factorB * cosLat2 * Math.Sin(lon2);

            double z =
                factorA * sinLat1 +
                factorB * sinLat2;

            double interpolatedLat = Math.Atan2(z, Math.Sqrt(x * x + y * y));
            double interpolatedLon = Math.Atan2(y, x);

            return (ToDeg(interpolatedLat), ToDeg(interpolatedLon));
        }

    }

    public class DensePoint
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double Elevation { get; set; } = double.NaN;
        // the following fields are computed later for convenience
        public double SegmentMeters { get; set; } = 0.0; // distance to next point
        public double DeltaH { get; set; } = 0.0; // elevation(next)-elevation(current)
    }
}