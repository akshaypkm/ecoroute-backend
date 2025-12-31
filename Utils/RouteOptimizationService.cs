using System.Text;
using System.Text.Json;
using EcoRoute.Models; // For OrderDto
using EcoRoute.Models.DTOs;
using EcoRoute.Services;
using EcoRoute.Utils;
using Microsoft.Extensions.Configuration;

namespace EcoRoute.Utils
{
    public class RouteOptimizationService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public RouteOptimizationService(IConfiguration config)
        {
            _http = new HttpClient();
            _apiKey = config.GetConnectionString("GoogleAPIKey");
        }

        public async Task<OptimizedRouteResultDto> GenerateOptimizedRoute(List<OrderDto> orders)
        {
            if (orders == null || !orders.Any()) return null;

            // -----------------------------
            // STEP 1: Build PICKUP stops
            // -----------------------------
            var pickupStops = orders.Select(o => new RouteStopDto
            {
                OrderId = o.OrderId,
                OrderCode = o.OrderCode,
                StopType = "PICKUP",
                Lat = o.OriginRP.Lat,
                Lng = o.OriginRP.Lng
            }).ToList();

            // -----------------------------
            // STEP 2: Optimize PICKUPS only (TSP is VALID here)
            // -----------------------------
            var pickupPoints = pickupStops
                .Select(p => new RoutePoint { Lat = p.Lat, Lng = p.Lng })
                .ToList();

            var pickupMatrix = await GetDistanceMatrixAsync(pickupPoints);
            var pickupOrder = TspSolver.Solve(pickupMatrix);

            var orderedPickups = pickupOrder
                .Select(i => pickupStops[i])
                .ToList();

            // -----------------------------
            // STEP 3: Build DROPS
            // -----------------------------
            var dropStops = orders.Select(o => new RouteStopDto
            {
                OrderId = o.OrderId,
                OrderCode = o.OrderCode,
                StopType = "DROP",
                Lat = o.DestinationRP.Lat,
                Lng = o.DestinationRP.Lng
            }).ToList();

            // -----------------------------
            // STEP 4: Order DROPS from last pickup (NO BACKTRACKING)
            // -----------------------------
            var lastPickup = orderedPickups.Last();

            var orderedDrops = dropStops
                .OrderBy(d => Geometry.HaversineMeters(
                    lastPickup.Lat, lastPickup.Lng,
                    d.Lat, d.Lng))
                .ToList();

            // -----------------------------
            // STEP 5: Assign SEQUENCE
            // -----------------------------
            int seq = 1;
            foreach (var p in orderedPickups) p.Sequence = seq++;
            foreach (var d in orderedDrops) d.Sequence = seq++;

            var finalStops = orderedPickups.Concat(orderedDrops).ToList();

            // -----------------------------
            // STEP 6: Generate POLYLINE
            // -----------------------------
            var routePoints = finalStops
                .Select(s => new RoutePoint { Lat = s.Lat, Lng = s.Lng })
                .ToList();

            var result = await GetDirectionsAsync(routePoints);
            return new OptimizedRouteResultDto
            {
                Polyline = result.polyline,
                Stops = finalStops,
                TotalDistanceMeters = result.distanceMeters / 1000
            };
        }



        // --- PRIVATE HELPER METHODS ---

        private async Task<RoutePoint> GeocodeAsync(string address)
        {
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";
            var response = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            
            if (doc.RootElement.GetProperty("status").GetString() == "OK")
            {
                var location = doc.RootElement.GetProperty("results")[0]
                                              .GetProperty("geometry")
                                              .GetProperty("location");
                return new RoutePoint 
                { 
                    Lat = location.GetProperty("lat").GetDouble(), 
                    Lng = location.GetProperty("lng").GetDouble() 
                };
            }
            return null;
        }

        private async Task<double[][]> GetDistanceMatrixAsync(List<RoutePoint> points)
        {
            int n = points.Count;
            var matrix = new double[n][];
            for (int i = 0; i < n; i++) matrix[i] = new double[n];

            // Build coordinate string: "lat,lng|lat,lng..."
            var coordString = string.Join("|", points.Select(p => $"{p.Lat},{p.Lng}"));

            var url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={coordString}&destinations={coordString}&key={_apiKey}";
            
            var response = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);

            if (doc.RootElement.GetProperty("status").GetString() != "OK") 
                throw new Exception("Distance Matrix API failed");

            var rows = doc.RootElement.GetProperty("rows");
            for (int i = 0; i < n; i++)
            {
                var elements = rows[i].GetProperty("elements");
                for (int j = 0; j < n; j++)
                {
                    var el = elements[j];
                    if (el.GetProperty("status").GetString() == "OK")
                    {
                        matrix[i][j] = el.GetProperty("distance").GetProperty("value").GetDouble();
                    }
                    else
                    {
                        matrix[i][j] = double.MaxValue; // Unreachable
                    }
                }
            }
            return matrix;
        }

        private async Task<(string polyline, double distanceMeters)> GetDirectionsAsync(List<RoutePoint> orderedPoints)
        {
            var origin = orderedPoints.First();
            var dest = orderedPoints.Last();
            
            // Middle points are waypoints
            var waypoints = string.Join("|", orderedPoints.Skip(1).Take(orderedPoints.Count - 2).Select(p => $"{p.Lat},{p.Lng}"));

            var url = $"https://maps.googleapis.com/maps/api/directions/json?origin={origin.Lat},{origin.Lng}&destination={dest.Lat},{dest.Lng}&waypoints={waypoints}&key={_apiKey}";
            
            var response = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);

            if (doc.RootElement.GetProperty("status").GetString() != "OK")
            {
                return (null, 0);
            }
            
            var route = doc.RootElement.GetProperty("routes")[0];

            var polyline = route
                .GetProperty("overview_polyline")
                .GetProperty("points")
                .GetString();
            
            double totalDistance = 0;
            foreach (var leg in route.GetProperty("legs").EnumerateArray())
            {
                totalDistance += leg
                    .GetProperty("distance")
                    .GetProperty("value")
                    .GetDouble(); // meters
            }

            return (polyline,totalDistance);
        }

        private bool IsSameLocation(RoutePoint a, RoutePoint b, double meters = 50)
        {
            return Geometry.HaversineMeters(a.Lat, a.Lng, b.Lat, b.Lng) <= meters;
        }

    }
}