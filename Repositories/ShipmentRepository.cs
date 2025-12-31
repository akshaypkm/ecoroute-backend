using AutoMapper;
using EcoRoute.Data;
using EcoRoute.Models;
using EcoRoute.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoRoute.Repositories
{
    public interface IShipmentRepository
    {
        Task<int> GetTotalShipmentsCompanyAndDateWise(int companyId, DateTime ShipmentStartDate, DateTime ShipmentEndDate);

        Task<string> GetShipmentCodeByShipmentId(int shipmentId);




        
        // CONTRACTS FOR ADMIN

        Task<int> GetAdminDashTotalShipments(int TransportCompanyId, DateTime ShipmentStartDate, DateTime ShipmentEndDate);

        Task<int> GetSoFarReviewedShipmentCount(int TransportCompanyId);

        Task<List<Order>> GetShipmentsForReview(int TransportCompanyId);

        Task<int> CreateShipment(OrderDto orderDto);

        Task<List<Shipment>> GetAllAdminShipmentsAsync(int TransportCompanyId);

        Task AddShipmentAsync(Shipment shipment);

        Task SaveChangesAsync();
        Task DeleteShipmentAsync(int? shipmentId);
    }
    public class ShipmentRepository(EcoRouteDbContext dbContext, IMapper _mapper) : IShipmentRepository
    {
        private readonly EcoRouteDbContext dbContext = dbContext;
        private readonly IMapper _mapper = _mapper;

        public async Task<int> GetTotalShipmentsCompanyAndDateWise(int companyId, DateTime ShipmentStartDate, DateTime ShipmentEndDate)
        {
            return await dbContext.Shipments.Where(s => s.OrderList.Any(o => o.CompanyId == companyId) 
                                            && s.ShipmentDate >= ShipmentStartDate && s.ShipmentDate <= ShipmentEndDate).CountAsync();
        }

        public async Task<string> GetShipmentCodeByShipmentId(int shipmentId)
        {
            string shipmentCode = await dbContext.Shipments.Where(s => s.Id == shipmentId)
                                            .Select(s => s.ShipmentCode).FirstOrDefaultAsync();

            Console.WriteLine($"shipment code: {shipmentCode}");
            return shipmentCode;
        }




        // METHODS FOR ADMIN

        public async Task<int> GetAdminDashTotalShipments(int TransportCompanyId, DateTime ShipmentStartDate, DateTime ShipmentEndDate)
        {
            return await dbContext.Shipments.Where(s => s.TransportCompanyId == TransportCompanyId && s.ShipmentDate >= ShipmentStartDate && s.ShipmentDate <= ShipmentEndDate).CountAsync();
        }

        public async Task<int> GetSoFarReviewedShipmentCount(int TransportCompanyId)
        {
            return await dbContext.Shipments.Where(s => s.TransportCompanyId == TransportCompanyId).CountAsync();
        }

        public async Task<List<Order>> GetShipmentsForReview(int TransportCompanyId)
        {
            return await dbContext.Orders.Where(o => o.TransportCompanyId == TransportCompanyId && ( o.IsAutoApproved == true || o.OrderStatus == "processing" 
                                        || o.OrderStatus == "planned")).OrderByDescending(c => c.Id).ToListAsync();
        }

        public async Task<int> CreateShipment(OrderDto orderDto)
        {
            var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderDto.OrderId);

            if(order == null)
            {
                throw new Exception("order not found while creating a new shipment");
            }

            var shipmentToAdd = new Shipment()
            {
                ShipmentCO2Emission = orderDto.OrderCO2Emission,
                ShipmentDate = orderDto.OrderDate,
                ShipmentTotalItems = orderDto.OrderTotalItems,
                ShipmentWeightKg = orderDto.OrderWeightKg,
                ShipmentLength = orderDto.OrderLength,
                ShipmentWidth = orderDto.OrderWidth,
                ShipmentHeight= orderDto.OrderHeight,
                ShipmentOrgin = orderDto.OrderOrigin,
                ShipmentDestination = orderDto.OrderDestination,
                ShipmentDistance = orderDto.OrderDistance,
                Vehicle = orderDto.TransportVehicle,
                ShipmentMode = "dedicated",
                TransportCompanyId = orderDto.TransportCompanyId,
                
            };
            Console.WriteLine($"++++++++++++++++++COMPANY ID IN ORDERDTO: {orderDto.CompanyId}");

            // shipmentToAdd.OrderList.Add(order);
            order.Shipment = shipmentToAdd;
            await dbContext.AddAsync(shipmentToAdd);
            await dbContext.SaveChangesAsync();

            int shipId = shipmentToAdd.Id;

            return shipId;
        }

        public async Task<List<Shipment>> GetAllAdminShipmentsAsync(int TransportCompanyId)
        {
            // Eager load OrderList and Company for each order
            return await dbContext.Shipments.Where(s => s.TransportCompanyId == TransportCompanyId)
                .Include(s => s.OrderList)
                    .ThenInclude(o => o.Company).OrderByDescending(c => c.Id)
                .ToListAsync();
        }

        public async Task AddShipmentAsync(Shipment shipment)
        {
            await dbContext.AddAsync(shipment);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteShipmentAsync(int? shipmentId)
        {
            await dbContext.Shipments.Where(s => s.Id == shipmentId).ExecuteDeleteAsync();
        }

    }
}