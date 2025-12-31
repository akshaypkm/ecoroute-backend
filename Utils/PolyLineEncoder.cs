using System.Text;
using EcoRoute.Models;

namespace EcoRoute.Utils
{
    public class PolyLineEncoder
    {
        public static class PolylineEncoder
{
    public static string Encode(List<RoutePoint> points)
    {
        var sb = new StringBuilder();
        int prevLat = 0, prevLng = 0;

        foreach (var p in points)
        {
            int lat = (int)Math.Round(p.Lat * 1e5);
            int lng = (int)Math.Round(p.Lng * 1e5);

            EncodeValue(lat - prevLat, sb);
            EncodeValue(lng - prevLng, sb);

            prevLat = lat;
            prevLng = lng;
        }

        return sb.ToString();
    }

    private static void EncodeValue(int value, StringBuilder sb)
    {
        value <<= 1;
        if (value < 0)
            value = ~value;

        while (value >= 0x20)
        {
            sb.Append((char)((0x20 | (value & 0x1f)) + 63));
            value >>= 5;
        }
        sb.Append((char)(value + 63));
    }
}

    }
}