using System.ComponentModel.DataAnnotations;
using EcoRoute.Data;
using EcoRoute.Models;
using EcoRoute.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoRoute.Repositories
{

    public interface IOrderRepository
    {
        Task AddOrdersAsync(Order order);
        Task SaveChangesAsync();
        IQueryable<Order> GetOrdersByCompanyId(int companyId);
        Task<IEnumerable<Order>> GetOrderByDateRange(DateTime startDate, DateTime endDate);




        // CONTRACTS FOR ADMIN
        Task<int> GetAdminDashTotalOrdersForReview(int TransportCompanyId);
        Task ChangeOrderStatus(int orderId, string status);

        Task InsertShipmentIdInOrder(int orderDto, int shipId);

        Task<Order> GetOrderByOrderId(int OrderId);

        Task<List<Order>> GetAutoApprovalOrdersByTransportCompanyId(int transportCompanyId);

        Task CollapseAutoApprove(int transportCompanyId);

    }
    public class OrderRepository(EcoRouteDbContext dbContext) : IOrderRepository
    {
        private readonly EcoRouteDbContext dbContext = dbContext;
        public async Task AddOrdersAsync(Order order)
        {
            await dbContext.Orders.AddAsync(order);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public IQueryable<Order> GetOrdersByCompanyId(int companyId)
        {
            return dbContext.Orders.Where(o => o.CompanyId == companyId);
        }

        public async Task<IEnumerable<Order>> GetOrderByDateRange(DateTime startDate, DateTime endDate)
        {
            return await dbContext.Orders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate).ToListAsync();
        }




        // METHODS FOR ADMIN
        public async Task<int>  GetAdminDashTotalOrdersForReview(int TransportCompanyId)
        {
            return await dbContext.Orders.Where(o => o.TransportCompanyId == TransportCompanyId && o.OrderStatus == "processing").CountAsync();
        }

        public async Task ChangeOrderStatus(int orderId, string status)
        {
            await dbContext.Orders.Where(o => o.Id == orderId).
                                        ExecuteUpdateAsync(o => o.SetProperty(o => o.OrderStatus, status));
        }

        public async Task InsertShipmentIdInOrder(int orderId, int shipId)
        {
            await dbContext.Orders.Where(o => o.Id == orderId).
                                    ExecuteUpdateAsync(o => o.SetProperty(o => o.ShipmentId, shipId));
        }

        public async Task<Order> GetOrderByOrderId(int OrderId)
        {
            return await dbContext.Orders.Where(o => o.Id == OrderId).FirstOrDefaultAsync();
        }

        public async Task<List<Order>> GetAutoApprovalOrdersByTransportCompanyId(int transportCompanyId)
        {
            DateTime now = DateTime.Today;
            Console.WriteLine($"%%%%%%%%%%%%%THE REPO IS RUNNING!!");
            return await dbContext.Orders.Where(o => o.TransportCompanyId == transportCompanyId
                                            && o.OrderMode.ToLower() == "dedicated"
                                                && (o.OrderStatus.Equals("planned") || o.OrderStatus.Equals("processing"))
                                                    && o.IsAutoApproved == false
                                                        && o.OrderDate >= now && o.OrderDate <= now.AddDays(3)
                                                            && o.OrderCO2Emission <= o.OrderStandardCO2Emissions
                                                                && o.OrderDistance <= 100.0).ToListAsync();

                                                           
        }

        public async Task CollapseAutoApprove(int transportCompanyId)
        {
            await dbContext.Orders.Where(o => o.TransportCompanyId == transportCompanyId).
                                        ExecuteUpdateAsync(o => o.SetProperty(o => o.IsRender, false));
        }

    }
}

