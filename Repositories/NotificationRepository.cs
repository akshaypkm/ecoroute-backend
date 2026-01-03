using EcoRoute.Data;
using EcoRoute.Models.HelperClasses;
using Microsoft.EntityFrameworkCore;

namespace EcoRoute.Repositories
{
    public interface INotificationRepository
    {
        Task AddNotificationAsync(Notification notification);

        Task SaveChangesAsync();

        Task<List<Notification>> GetNotificationsByCompanyIdAsync(int companyId);

        Task<List<Notification>> GetUnreadNotificationsByCompanyIdAsync(int companyId);
        Task<int> GetUnreadCountByCompanyIdAsync(int companyId);
    }
    public class NotificationRepository(EcoRouteDbContext dbContext) : INotificationRepository
    {
        private readonly EcoRouteDbContext dbContext = dbContext;

        public async Task AddNotificationAsync(Notification notification)
        {
            await dbContext.Notifications.AddAsync(notification);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetNotificationsByCompanyIdAsync(int companyId)
        {
            return await dbContext.Notifications.Where(n => n.TargetCompanyId == companyId)
                                                    .OrderByDescending(n => n.CreatedAt)
                                                        .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadNotificationsByCompanyIdAsync(int companyId)
        {
            return await dbContext.Notifications
            .Where(n => n.TargetCompanyId == companyId && !n.IsRead)
            .ToListAsync();
        }
        public async Task<int> GetUnreadCountByCompanyIdAsync(int companyId)
        {
            return await dbContext.Notifications.CountAsync(n => n.TargetCompanyId == companyId && !n.IsRead);
        }
    }
}