using System.Text.RegularExpressions;
using AutoMapper;
using EcoRoute.Data;
using EcoRoute.Models;
using EcoRoute.Models.DTOs;
using EcoRoute.Models.Entities;
using EcoRoute.Models.HelperClasses;
using EcoRoute.Repositories;
using EcoRoute.Utils;

namespace EcoRoute.Services
{

    public interface IAdminShipmentReviewService
    {
        Task<List<OrderDto>> GetShipmentsForReview(string userIdFromToken);

        Task ApproveShipment(OrderDto orderDto);

        Task CancelShipment(OrderDto orderDto);

        Task<List<ImproviseShipmentGroupDto>> ImproviseShipments(List<OrderDto> orderDtos);

        Task ApproveGroupedShipment(ImproviseShipmentGroupDto groupDto);

        Task CollapseAutoApprove(string transportCompanyName);
    }
    public class AdminShipmentReviewService : IAdminShipmentReviewService
    {
        private readonly IShipmentRepository _shipmentRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly INotificationRepository _notifRepo;
        private readonly ICompanyRepository _companyRepo;
        private readonly ITruckRepository _truckRepo;

        private readonly IMapper _mapper;

        private readonly RouteOptimizationService _ROS;

        private readonly EcoRouteDbContext _dbcontext;
        private readonly IConfiguration _config;

        public AdminShipmentReviewService(IShipmentRepository _shipmentRepo, IMapper _mapper
                                        , IOrderRepository _orderRepo, INotificationRepository _notifRepo,
                                        ITruckRepository _truckRepo, RouteOptimizationService _ROS
                                        ,EcoRouteDbContext _dbcontext, ICompanyRepository _companyRepo,
                                        IConfiguration _config)
        {
            this._shipmentRepo = _shipmentRepo;
            this._mapper = _mapper;
            this._orderRepo = _orderRepo;
            this._notifRepo = _notifRepo;
            this._companyRepo = _companyRepo;
            this._truckRepo = _truckRepo;
            this._ROS = _ROS;
            this._dbcontext = _dbcontext;
            this._config = _config;
        }
        
        public async Task<List<OrderDto>> GetShipmentsForReview(string userIdFromToken)
        {
            int TransportCompanyId = await _companyRepo.GetCompanyIdByUserId(userIdFromToken);
            
            var orderDtos = new List<OrderDto>();

            var orders = await _shipmentRepo.GetShipmentsForReview(TransportCompanyId);

            foreach(var order in orders)
            {
                var orderDto = _mapper.Map<OrderDto>(order);

                orderDtos.Add(orderDto);
            }

            return orderDtos;
        }

        public async Task ApproveShipment(OrderDto orderDto)
        {
            string status = "placed";
            
            int shipId = await _shipmentRepo.CreateShipment(orderDto);

            await _orderRepo.ChangeOrderStatus(orderDto.OrderId, status);
            await _orderRepo.InsertShipmentIdInOrder(orderDto.OrderId, shipId);

            var notification = new Notification(){
                Message = $"Your order for ID({orderDto.OrderCode}) from {orderDto.OrderOrigin} --> {orderDto.OrderDestination} has been placed successfully!",
                IsRead = false,
                TargetCompanyId = orderDto.CompanyId
            };

            await _notifRepo.AddNotificationAsync(notification);
            await _notifRepo.SaveChangesAsync();
        }

        public async Task CancelShipment(OrderDto orderDto)
        {
            string status = "cancelled";

            var transaction = await _dbcontext.Database.BeginTransactionAsync();
            await _orderRepo.ChangeOrderStatus(orderDto.OrderId, status);

            var refundCredits = orderDto.OrderCO2Emission / 1000;
            await _companyRepo.RefundCompanyCredits(orderDto.CompanyId, refundCredits);
             
            var notification = new Notification(){
                Message = $"Your order for ID({orderDto.OrderCode}) from {orderDto.OrderOrigin} -> {orderDto.OrderDestination} has been cancelled by the admin",
                IsRead = false,
                TargetCompanyId = orderDto.CompanyId
            };

            if (orderDto.IsAutoApproved)
            {
                var shipId = orderDto.ShipmentId;

                var order = await _orderRepo.GetOrderByOrderId(orderDto.OrderId);
                order.ShipmentId = null;
                order.IsAutoApproved = false;

                await _orderRepo.SaveChangesAsync();
                await _shipmentRepo.DeleteShipmentAsync(shipId);
            }

            await _notifRepo.AddNotificationAsync(notification);
            await _notifRepo.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        public async Task<List<ImproviseShipmentGroupDto>> ImproviseShipments(List<OrderDto> orderDtos)
        {
            var API_KEY = _config.GetConnectionString("GoogleAPIKey");
            var gms = new GoogleMapsService(API_KEY);

            var optimizedGroups = new List<ImproviseShipmentGroupDto>();

            var processedOrderIds = new HashSet<int>();

           var routeGroups = orderDtos.Where(o => o.OrderMode.ToLower() == "shared")
                                        .GroupBy( o=> new
                                        {
                                            o.IsRefrigerated,
                                            
                                        });

            Console.WriteLine($"ROUTE GRoups LENGTH ----------------- {routeGroups.Count()}");
            
            foreach(var rG in routeGroups)
            {
                bool isRefrigerated = rG.Key.IsRefrigerated;

                var ordersInRoute = rG.ToList();

                for(int i = 0; i< ordersInRoute.Count; i++)
                {
                    var anchor = ordersInRoute[i];

                    if (processedOrderIds.Contains(anchor.OrderId))
                    {
                        continue;
                    }

                    var currentCluster = new List<OrderDto>
                    {
                        anchor
                    };
                    processedOrderIds.Add(anchor.OrderId);

                    for(int j = i+1; j< ordersInRoute.Count; j++)
                    {
                        var candidate = ordersInRoute[j];

                        if (processedOrderIds.Contains(candidate.OrderId))
                        {
                            continue;
                        }
                        Console.WriteLine("OUTSIDE THE IF CHECK");
                        if(await IsSpatiallyCompatible(anchor, candidate) && await IsTemporallyCompatible(anchor, candidate))
                        {
                        Console.WriteLine("INSIDE THE IF CHECK");
                            currentCluster.Add(candidate);
                            // processedOrderIds.Add(candidate.OrderId);
                        }
                    }
                    var packingInput = new ClusterPackingInputDto
                    {
                        TotalWeightKg = currentCluster.Sum(c => c.OrderWeightKg),
                        Items = currentCluster.Select(o => new PackableItem
                        {
                            Length = o.OrderLength,
                            Width  = o.OrderWidth,
                            Height = o.OrderHeight,
                            Quantity = o.OrderTotalItems
                        }).ToList()
                    };

                    var candidateTrucks = await _truckRepo.GetTruckTypeAsync(packingInput.TotalWeightKg, isRefrigerated);

                    TruckType truck = null;

                    foreach (var t in candidateTrucks)
                    {
                        if (CanFitCluster(packingInput.Items, t))
                        {
                            truck = t;
                            break;
                        }
                    }

                    Console.WriteLine($"TRUCK FOR COMBINED ORDER ::::::::::::::{truck.TruckName}");

                    if (truck != null)
                    {
                        foreach (var o in currentCluster)
                            processedOrderIds.Add(o.OrderId);
                    }
                    else
                    {
                        continue;
                    }

                                       
                    var optimizedRouteData = await _ROS.GenerateOptimizedRoute(currentCluster);
                    
                    var route = await gms.GetRoutePointsForGroupedOrdersAsync(optimizedRouteData.Polyline);
                    var densePointsWithCoors = Geometry.Densify(route, 50.0);
                    var densePoints = densePointsWithCoors.densePoints;

                    await gms.FillElevationAsync(densePoints);

                    var routeSegments = BuildRouteSegments(densePoints);

                    double massPerTonne_ = Physics.EffectiveMassPerTonne(truck.KerbWeight, packingInput.TotalWeightKg);

                    Console.WriteLine($"mass per tonne of grouped order : {massPerTonne_}");
                    var segmentEmissions = ComputeSegmentEmissions(
                        routeSegments,
                        massPerTonne_,
                        truck.EngineEfficiency
                    );

                    double totalCO2_from_segments = segmentEmissions.Sum(s => s.KgCO2);

                    Console.WriteLine($"CO2 via segments: {totalCO2_from_segments:F2}");

                    Console.WriteLine($"Segments built: {routeSegments.Count}");
                    Console.WriteLine($"Total distance (km): {routeSegments.Sum(s => s.DistanceMeters) / 1000}");
                    Console.WriteLine($"Total uphill (m): {routeSegments.Sum(s => s.ElevationGainMeters)}");

                    var orderWindows = new Dictionary<int, (int start, int end)>();

                    foreach (var order in currentCluster)
                    {
                        var pickup = optimizedRouteData.Stops
                                                .Where(s => s.OrderId == order.OrderId && s.StopType == "PICKUP")
                                                .OrderBy(s => s.Sequence)
                                                .FirstOrDefault();

                        var drop = optimizedRouteData.Stops
                                                .Where(s => s.OrderId == order.OrderId && s.StopType == "DROP")
                                                .OrderByDescending(s => s.Sequence)
                                                .FirstOrDefault();

                        if (pickup == null || drop == null)
                            throw new InvalidOperationException($"Missing pickup/drop for order {order.OrderId}");

                        int startSeg = FindNearestSegmentIndex(routeSegments, pickup.Lat, pickup.Lng);
                        int endSeg   = FindNearestSegmentIndex(routeSegments, drop.Lat, drop.Lng);

                        if (endSeg <= startSeg)
                            throw new InvalidOperationException("Drop occurs before pickup");

                        orderWindows[order.OrderId] = (startSeg, endSeg);
                    }

                    Console.WriteLine($"im not failing at the foreach loop!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1");


                    double[] payloadPerSegment = new double[routeSegments.Count];

                    for (int m = 0; m < routeSegments.Count; m++)
                    {
                        double payload = 0.0;

                        foreach (var order in currentCluster)
                        {
                            var (start, end) = orderWindows[order.OrderId];

                            if (m >= start && m < end)
                            {
                                payload += order.OrderWeightKg;
                            }
                        }

                        payloadPerSegment[m] = payload;
                    }

                    Console.WriteLine($"im not failing while calculating per segment payload!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1");

                    var orderCO2 = new Dictionary<int, double>();

                    foreach (var order in currentCluster)
                    {
                        orderCO2[order.OrderId] = 0.0;

                        var (start, end) = orderWindows[order.OrderId];

                        for (int n = start; n < end; n++)
                        {
                            double segmentCO2 = segmentEmissions[n].KgCO2; // already absolute
                            double totalPayloadKg = payloadPerSegment[n];

                            if (totalPayloadKg <= 0) continue;

                            double share = order.OrderWeightKg / totalPayloadKg;

                            orderCO2[order.OrderId] += segmentCO2 * share;

                            
                        }
                    }

                    Console.WriteLine($"im not failing while assiging segment wise emission for order!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1");

                    foreach (var orderDto in currentCluster)
                    {
                        double orderKgCO2 = orderCO2[orderDto.OrderId] * (orderDto.OrderWeightKg / 1000.0);

                        orderDto.OldCO2Emission = orderDto.OrderCO2Emission;
                        orderDto.OrderCO2Emission = Math.Round(orderKgCO2, 3);   // OrderCO2Emission NOW BECOMES ABSOLUTE!!!!!!!!!!!!!!!!
                        orderDto.TransportVehicle = truck.TruckName;
                    }

                    double sumOrders = orderCO2.Values.Sum();
                    double totalRouteCO2 = segmentEmissions.Sum(s => s.KgCO2);

                    Console.WriteLine($"Orders CO2 Sum: {sumOrders:F3}");
                    Console.WriteLine($"Route CO2 Total: {totalRouteCO2:F3}");
                    

                    Console.WriteLine($"ROUTE STOPS LENGTH _____----------____________{optimizedRouteData.Stops.Count()}");
                    optimizedGroups.Add(new ImproviseShipmentGroupDto
                    {
                        Orders = currentCluster,
                        OptimizedGroupPolyline = optimizedRouteData.Polyline,
                        TransportVehicle = truck.TruckName,
                        RouteStops = optimizedRouteData.Stops,
                        OptimizedDistance = optimizedRouteData.TotalDistanceMeters,

                        CombinedOrderWeightKg = packingInput.TotalWeightKg,

                        CombinedOrderTotalItems =
                            currentCluster.Sum(o => o.OrderTotalItems),

                        ApproxCombinedVolume =
                            currentCluster.Sum(o =>
                                o.OrderLength * o.OrderWidth * o.OrderHeight * o.OrderTotalItems),

                        TotalShipmentCO2Emissions = currentCluster.Sum(o => o.OrderCO2Emission),
                        TransportCompanyId = currentCluster.Select(o => o.TransportCompanyId).FirstOrDefault()
                    });

                    Console.WriteLine($"total shipment co2 emissions -> :::::{currentCluster.Sum(o => o.OrderCO2Emission)}");

                    foreach(var order in currentCluster)
                    {
                        order.TransportVehicle = truck.TruckName;
                    }   
                }
            }
            return optimizedGroups;
        }

        public async Task ApproveGroupedShipment(ImproviseShipmentGroupDto groupDto)
        {
            if(groupDto.Orders == null || !groupDto.Orders.Any())
            {
                throw new Exception("empty shipment group");
            }

            using var transaction = await _dbcontext.Database.BeginTransactionAsync();

            var earliestDate = groupDto.Orders.Min(o => o.OrderDate);

            var lastDrop = groupDto.RouteStops.Where(s => s.StopType == "DROP")
                                                .OrderByDescending(s => s.Sequence)
                                                    .FirstOrDefault();

            if(lastDrop == null)
            {
                throw new InvalidOperationException("no DROP found for grouped shipment");
            }

            var firstPickup = groupDto.RouteStops
                                            .Where(s => s.StopType == "PICKUP")
                                            .OrderBy(s => s.Sequence)
                                            .First();

                var originOrder = groupDto.Orders
                    .First(o => o.OrderId == firstPickup.OrderId);

            var destinationOrder = groupDto.Orders.First( o=> o.OrderId == lastDrop.OrderId);
            var shipment =  new Shipment
            {
                ShipmentDate = earliestDate,
                
                ShipmentOrgin = originOrder.OrderOrigin,
                
                ShipmentDestination = destinationOrder.OrderDestination,

                ShipmentTotalItems = groupDto.Orders.Sum(o => o.OrderTotalItems),
                ShipmentWeightKg = groupDto.CombinedOrderWeightKg,

                ShipmentLength = groupDto.Orders.Max(o => o.OrderLength),
                ShipmentWidth  = groupDto.Orders.Max(o => o.OrderWidth),
                ShipmentHeight = groupDto.Orders.Max(o => o.OrderHeight), 
                ShipmentDistance =  groupDto.OptimizedDistance,
                Vehicle = groupDto.TransportVehicle,
                ShipmentCO2Emission = groupDto.TotalShipmentCO2Emissions,
                ShipmentMode = "shared",
                TransportCompanyId = groupDto.TransportCompanyId,

                OrderList = new List<Order>()
            };

            foreach(var orderDto in groupDto.Orders)
            {
                var order = await _orderRepo.GetOrderByOrderId(orderDto.OrderId);

                order.OrderCO2Emission = orderDto.OrderCO2Emission;
                order.OrderStatus = "placed";
                order.TransportVehicle = groupDto.TransportVehicle;
                order.OrderDate = earliestDate;
                shipment.OrderList.Add(order);

                double refundCredits = (orderDto.OldCO2Emission - orderDto.OrderCO2Emission) / 1000;

                await _companyRepo.RefundCompanyCredits(orderDto.CompanyId, refundCredits); 
                
                var notif = new Notification()
                {
                    Message = $"Your order ({orderDto.OrderCode}) from {orderDto.OrderOrigin} to {orderDto.OrderDestination} has been set to a shared shipment on {order.OrderDate.ToShortDateString()}",
                    IsRead = false,
                    TargetCompanyId = orderDto.CompanyId
                };

                await _notifRepo.AddNotificationAsync(notif);
                await _notifRepo.SaveChangesAsync();
            }

            await _shipmentRepo.AddShipmentAsync(shipment);
            await _shipmentRepo.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        public async Task CollapseAutoApprove(string companyName)
        {
            var transportCompanyId = await _companyRepo.GetCompanyIdByName(companyName);

            Console.WriteLine($"COMPANY ID: {transportCompanyId}");

            await _orderRepo.CollapseAutoApprove(transportCompanyId);
        }

        private async Task<bool> IsSpatiallyCompatible(OrderDto a, OrderDto b)
        {
            double distOrigin = Geometry.HaversineMeters(a.OriginRP.Lat, a.OriginRP.Lng, b.OriginRP.Lat, b.OriginRP.Lng);
            Console.WriteLine($"DISTANCE BTW TWO ORIGINS _________: {distOrigin}");
            double distDestination = Geometry.HaversineMeters(a.DestinationRP.Lat, a.DestinationRP.Lng, b.DestinationRP.Lat, b.DestinationRP.Lng);
            Console.WriteLine($"DISTANCE BTW TWO DESTINATIONS _________: {distDestination}");

            return distOrigin <= 7500 && distDestination <= 7500;
        }

        private async  Task<bool> IsTemporallyCompatible(OrderDto a, OrderDto b)
        {
            var minDate = a.OrderDate.AddDays(-1);
            var maxDate = a.OrderDate.AddDays(3);

            return b.OrderDate >= minDate && b.OrderDate <= maxDate;
        }

        public bool CanFitCluster(
        List<PackableItem> items,
        TruckType truck)
        {
            double truckL = truck.CargoLengthMeters;
            double truckW = truck.CargoWidthMeters;
            double truckH = truck.CargoHeightMeters;

            double usedHeight = 0;

            foreach (var item in items)
            {
                // 1. Check base orientation
                bool fitsBase =
                    (item.Length <= truckL && item.Width <= truckW) ||
                    (item.Width  <= truckL && item.Length <= truckW);

                if (!fitsBase)
                    return false;

                // 2. Compute best floor fit (orientation-aware)
                double floorL = Math.Max(item.Length, item.Width);
                double floorW = Math.Min(item.Length, item.Width);

                int itemsPerRow = (int)(truckL / floorL);
                int itemsPerCol = (int)(truckW / floorW);

                int itemsPerLayer = Math.Max(1, itemsPerRow * itemsPerCol);

                // 3. Layers needed for this item type
                int layersNeeded =
                    (int)Math.Ceiling((double)item.Quantity / itemsPerLayer);

                usedHeight += layersNeeded * item.Height;

                // 4. Early exit
                if (usedHeight > truckH)
                    return false;
            }

            // 5. Safety margin
            return usedHeight <= truckH * 0.9;
        }


        //THIS FUNCTION USED IN IMPROVISE TO CALCULATE EACH SEGMENT'S PROPS, RETURNS LIST OF RS DTO
        private List<RouteSegment> BuildRouteSegments(List<DensePoint> densePoints)
        {
            var segments = new List<RouteSegment>();

            for (int i = 0; i < densePoints.Count - 1; i++)
            {
                var start = densePoints[i];
                var end   = densePoints[i + 1];

                double dh = end.Elevation - start.Elevation;
                double positiveGain = dh > 0 ? dh : 0;

                segments.Add(new RouteSegment
                {
                    Index = i,
                    DistanceMeters = end.SegmentMeters,
                    ElevationGainMeters = positiveGain,

                    StartLat = start.Lat,
                    StartLng = start.Lng,
                    EndLat   = end.Lat,
                    EndLng   = end.Lng
                });
            }

            return segments;
        }

        private List<SegmentEmission> ComputeSegmentEmissions(
            List<RouteSegment> segments,
            double massPerTonne,
            double engineEfficiency,
            double elevationNoiseThresholdMeters = 0.5
        )
        {
            var result = new List<SegmentEmission>();

            foreach (var s in segments)
            {
                double baseJ = massPerTonne * Physics.G * s.DistanceMeters * Physics.Crr;

                double uphillJ = 0.0;
                if (s.ElevationGainMeters > elevationNoiseThresholdMeters)
                {
                    uphillJ = massPerTonne * Physics.G * s.ElevationGainMeters;
                }

                double totalJ = baseJ + uphillJ;

                double liters = totalJ / (Physics.LHV_DIESEL_J_PER_L * engineEfficiency);
                double kgCO2  = liters * Physics.CO2_PER_L;

                result.Add(new SegmentEmission
                {
                    SegmentIndex = s.Index,
                    DistanceMeters = s.DistanceMeters,
                    ElevationGainMeters = s.ElevationGainMeters,
                    EnergyJoules = totalJ,
                    LitersUsed = liters,
                    KgCO2 = kgCO2
                });
            }

            return result;
        }

        private int FindNearestSegmentIndex(
            List<RouteSegment> segments,
            double lat,
            double lng
        )
        {
            double minDist = double.MaxValue;
            int nearestIndex = 0;

            foreach (var s in segments)
            {
                double d = Geometry.HaversineMeters(
                    lat, lng,
                    s.StartLat, s.StartLng
                );

                if (d < minDist)
                {
                    minDist = d;
                    nearestIndex = s.Index;
                }
            }

            return nearestIndex;
        }
    }
}