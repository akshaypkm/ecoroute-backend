using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks.Sources;
using EcoRoute.Data;
using EcoRoute.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoRoute.Repositories
{

    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(string UserId);
        Task<bool> UserExistsAsync(string UserId);
        Task AddUserAsync(User user);
        Task SaveChangesAsync();
        Task<User?> GetUserByEmailAsync(string email);
        Task UpdateUserAsync(User user);  
        Task<bool> UserExistsUsingEmailAsync(string email);  
        
    }
    public class UserRepository : IUserRepository
    {
        private readonly EcoRouteDbContext dbContext;
        public UserRepository(EcoRouteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await dbContext.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(string UserId)
        {
            return await dbContext.Users.Where(u => u.UserId == UserId).FirstOrDefaultAsync();
        }

        public async Task<bool> UserExistsAsync(string UserId)
        {
            return await dbContext.Users.AnyAsync(u => u.UserId == UserId);
        }

        public async Task AddUserAsync(User user)
        {
            await dbContext.Users.AddAsync(user);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await dbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
        }
        public async Task UpdateUserAsync(User user)
        {
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
        }
        public async Task<bool> UserExistsUsingEmailAsync(string email)
        {
            return await dbContext.Users.AnyAsync(u => u.Email == email);
        }

    }
}