using System.Globalization;
using System.Text.Json;
using EcoRoute.Models;
using static EcoRoute.Utils.PolyLineEncoder;

namespace EcoRoute.Services
{
    public class GoogleMapsService
    {
        private readonly HttpClient _http;

        private readonly string API_KEY;

        public GoogleMapsService(string API_KEY)
        {
            _http = new HttpClient();

            this.API_KEY = API_KEY;
        }

        public async Task<List<(List<RoutePoint> Points, string Summary, double Duration, string encodedString)>> GetRoutePointsAsync(string origin, string destination, string API_KEY)
        {
            string url = $"https://maps.googleapis.com/maps/api/directions/json?origin={Uri.EscapeDataString(origin)}&destination={Uri.EscapeDataString(destination)}&alternatives=true&key={API_KEY}";

            var text = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;

            if(root.TryGetProperty("status", out var status) && status.GetString() != "OK")
            {
                string err = root.TryGetProperty("error_message", out var e) ? e.GetString() : "Unknown";
                throw new Exception($"Directions API error: {status.GetString()} - {err}");
            }

            var finalRoutes = new List<(List<RoutePoint> Points, string Summary, double Duration, string encodedString)>();

            if(root.TryGetProperty("routes", out var routes) && routes.GetArrayLength() > 0)
            {
                int numberOfRoutes = routes.GetArrayLength(); //[DEBUG]
                Console.WriteLine($"================Total routes found: {numberOfRoutes}");

                foreach(var routeJson in routes.EnumerateArray())
                {
                    string summary = routeJson.TryGetProperty("summary", out var s) ? s.GetString() : "";
                    double duration = 0.0;

                    var singleRoutePoints = new List<RoutePoint>();

                    if(routeJson.TryGetProperty("legs", out var legs))
                    {
                        foreach(var leg in legs.EnumerateArray())
                        {
                            if(leg.TryGetProperty("duration", out var d) && d.TryGetProperty("value", out var durationInSecs))
                            {
                                duration += durationInSecs.GetDouble();
                            }
                            

                            if(leg.TryGetProperty("steps", out var steps))
                            {
                                foreach(var step in steps.EnumerateArray())
                                {
                                    if(step.TryGetProperty("polyline", out var poly) &&
                                        poly.TryGetProperty("points", out var pts))
                                    {
                                        string encoded = pts.GetString();
                                        var decoded = PolyLineDecoder.Decode(encoded);
                                        foreach(var p in decoded)
                                        {
                                            singleRoutePoints.Add(new RoutePoint
                                            {
                                                Lat = p.Lat,
                                                Lng = p.Lng
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    var compact = new List<RoutePoint>();
                    for(int i =0; i< singleRoutePoints.Count; i++)
                    {
                        if(i == 0 || singleRoutePoints[i].Lat != singleRoutePoints[i - 1].Lat 
                                  || singleRoutePoints[i].Lng != singleRoutePoints[i - 1].Lng)
                        {
                            compact.Add(singleRoutePoints[i]);
                        }
                    }
                    var encodedString = PolylineEncoder.Encode(compact);
                    finalRoutes.Add((compact, summary, duration, encodedString));
                }
            }
            return finalRoutes ;
        }


        internal async Task FillElevationAsync(List<DensePoint> points, int batchSize = 400)
        {
            var idx = 0;
            int count = 1;

            while(idx < points.Count)
            {
                Console.WriteLine($"API COUNT!! -> : {count}");
                count++;

                var slice = points.Skip(idx).Take(batchSize).ToList();
                string locations = string.Join("|", slice.Select(p => $"{p.Lat.ToString(CultureInfo.InvariantCulture)},{p.Lng.ToString(CultureInfo.InvariantCulture)}"));
                string url = $"https://maps.googleapis.com/maps/api/elevation/json?locations={Uri.EscapeDataString(locations)}&key={API_KEY}";

                var text = await _http.GetStringAsync(url);
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                if (root.TryGetProperty("status", out var status) && status.GetString() != "OK")
                {
                    string err = root.TryGetProperty("error_message", out var e) ? e.GetString() : "Unknown";
                    throw new Exception($"Elevation API error: {status.GetString()} - {err}");
                }

                if(root.TryGetProperty("results", out var results))
                {
                    int j = 0;
                    foreach(var r in results.EnumerateArray())
                    {
                        double elev = r.TryGetProperty("elevation", out var ev) ? ev.GetDouble() : double.NaN;
                        slice[j].Elevation = elev;
                        j++;
                    }
                }

                for(int k = 0; k<slice.Count; k++)
                {
                    points[idx + k].Elevation = slice[k].Elevation;
                }
                idx += batchSize;
                // await Task.Delay(100);
            }
        }





        // THIS METHOD IS FOR ADMIN SIDE
        public async Task<List<RoutePoint> > GetRoutePointsForGroupedOrdersAsync(string polyline)
        {
            var singleRoutePoints = new List<RoutePoint>();

            var decoded = PolyLineDecoder.Decode(polyline);

            foreach(var p in decoded)
            {
                singleRoutePoints.Add(new RoutePoint
                {
                    Lat = p.Lat,
                    Lng = p.Lng
                });
            }

            var compact = new List<RoutePoint>();
            for(int i =0; i< singleRoutePoints.Count; i++)
            {
                if(i == 0 || singleRoutePoints[i].Lat != singleRoutePoints[i - 1].Lat 
                            || singleRoutePoints[i].Lng != singleRoutePoints[i - 1].Lng)
                {
                    compact.Add(singleRoutePoints[i]);
                }
            }

            return compact;
        }
    }
}