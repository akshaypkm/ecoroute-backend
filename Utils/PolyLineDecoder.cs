
using EcoRoute.Models;

namespace EcoRoute.Services
{
    public class PolyLineDecoder
    {
        internal static List<RoutePoint> Decode(string? encodedPoints)
        {
            var poly = new List<RoutePoint>();

            if(string.IsNullOrEmpty(encodedPoints)) return poly;

            char[] polylineChars = encodedPoints.ToCharArray();

            int index = 0;
            int currentLat = 0;
            int currentLng = 0;

            while(index < polylineChars.Length)
            {
                // decode lat
                int sum = 0;
                int shifter = 0;
                int next5bits;
                do
                {
                    next5bits = polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5bits >= 32) break;
                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                // decode lng
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5bits >= 32) break;
                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                poly.Add(new RoutePoint
                {
                    Lat = currentLat/1e5,
                    Lng = currentLng / 1e5
                });
            }
            return poly;
        }

        
    }
}