using EcoRoute.Data;
using EcoRoute.Models.Entities;
using Microsoft.EntityFrameworkCore;
namespace EcoRoute.Repositories;
public interface IEmailOtpRepository
{
    Task<EmailOtp> GetOtpByEmailAsync(string email);
    Task AddOtpAsync(EmailOtp emailOtp);
    Task UpdateOtpAsync();
    Task DeleteOtpsByEmailAsync(string email);
}
public class EmailOtpRepository : IEmailOtpRepository
{
    private readonly EcoRouteDbContext _context;

    public EmailOtpRepository(EcoRouteDbContext context)
    {
        _context = context;
    }

    public async Task<EmailOtp> GetOtpByEmailAsync(string email)
    {
        return await _context.EmailOtps.OrderByDescending(x => x.Id).FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task AddOtpAsync(EmailOtp otp)
    {
        _context.EmailOtps.Add(otp);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateOtpAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task DeleteOtpsByEmailAsync(string email)
    {
        var otps = _context.EmailOtps.Where(e => e.Email == email);
        _context.EmailOtps.RemoveRange(otps);
        await _context.SaveChangesAsync();
    }
}   