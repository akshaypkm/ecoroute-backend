using AutoMapper;
using EcoRoute.Data;
using EcoRoute.Models;
using EcoRoute.Models.Entities;
using EcoRoute.Models.HelperClasses;
using EcoRoute.Repositories;

namespace EcoRoute.Services
{
    public interface IAutoApprovalService
    {
        Task AutoApprove(int transportCompanyId);
    }
    public class AutoApprovalService : IAutoApprovalService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IShipmentRepository _shipmentRepo;
        private readonly INotificationRepository _notifRepo;
        private readonly EcoRouteDbContext dbContext;
        private readonly IMapper _mapper;


        public AutoApprovalService(IOrderRepository _orderRepo, IShipmentRepository _shipmentRepo,
                                    EcoRouteDbContext dbContext, IMapper _mapper,
                                    INotificationRepository _notifRepo)
        {
            this._orderRepo = _orderRepo;
            this._shipmentRepo = _shipmentRepo;
            this._notifRepo = _notifRepo;
            this.dbContext = dbContext;
            this._mapper = _mapper;
        }

        public async Task AutoApprove(int transportCompanyId)
        {
            Console.WriteLine("INSIDE THE AUTO APPROVE METHOD##################!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            
            var orders = await _orderRepo.GetAutoApprovalOrdersByTransportCompanyId(transportCompanyId);
            Console.WriteLine($"########### TRANSPORT COMPANY ID : {transportCompanyId} ########333");

            Console.WriteLine($"########### ORDERS COUNT : {orders.Count} ########333");
            if(orders.Count < 2)
            {
                Console.WriteLine("less than two!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                return;
            }
            else
            {
                Console.WriteLine("everything is fine!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                
            }

            foreach(var order in orders)
            {
                Console.WriteLine("inside loop for orders ------------------_!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                var transaction = await dbContext.Database.BeginTransactionAsync();
                order.IsAutoApproved = true;
                order.OrderStatus = "placed";
                order.IsRender = true;

                var orderDto = _mapper.Map<OrderDto>(order);
                int shipId = await _shipmentRepo.CreateShipment(orderDto);

                await _orderRepo.InsertShipmentIdInOrder(orderDto.OrderId, shipId);

                var notification = new Notification()
                {
                    Message = $"Your order for ID({orderDto.OrderCode}) from {orderDto.OrderOrigin} --> {orderDto.OrderDestination} has been auto approved and placed successfully!",
                    IsRead = false,
                    TargetCompanyId = orderDto.CompanyId
                };

                await _notifRepo.AddNotificationAsync(notification);

                await _notifRepo.SaveChangesAsync();
                await _orderRepo.SaveChangesAsync();
                await _shipmentRepo.SaveChangesAsync();

                await transaction.CommitAsync();
            }

            var notificationAdmin = new Notification()
            {
                Message = $"{orders.Count} orders were found to match the criteria for auto-approval and have been auto approved.",
                IsRead = false,
                TargetCompanyId = transportCompanyId                    
            };

            await _notifRepo.AddNotificationAsync(notificationAdmin);
            await _notifRepo.SaveChangesAsync();
            
        }
    }
}