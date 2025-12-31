using EcoRoute.Data;
using EcoRoute.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EcoRoute.Repositories
{
    public interface IForeCastRepository
    {
        Task<int?> GetCompanyIdByName(string companyName);
        Task<double> GetTotalEmissionsForCompanyBetweenDates(int companyId,DateTime startDate,DateTime endDate);
        Task<List<MonthlyEmissionStatDto>> GetMonthlyEmissionsForLastNMonths(int companyId,int numberOfMonths);
        Task<string?> GetCompanySector(int companyId);
        Task<double> GetAnnualCreditLimitForSector(string sector);
        Task<DateTime> GetCompanyCreationDate(int companyId);
        Task<double> GetTotalEmissionsTillDate(int companyId,DateTime asOfDate
        );
    }
    public class ForeCastRepository : IForeCastRepository
    {
        private readonly EcoRouteDbContext _dbContext;
        public ForeCastRepository(EcoRouteDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<int?> GetCompanyIdByName(string companyName)
        {
            return await _dbContext.Companies.Where(c => c.CompanyName == companyName).Select(c => (int?)c.Id).FirstOrDefaultAsync();}
        public async Task<double> GetTotalEmissionsForCompanyBetweenDates(int companyId,DateTime startDate,DateTime endDate)
        {
            return await _dbContext.Orders.Where(o =>o.CompanyId == companyId && o.OrderStatus == "placed" &&
                    o.OrderDate >= startDate && o.OrderDate <= endDate).SumAsync(o => o.OrderCO2Emission);
        }
        public async Task<List<MonthlyEmissionStatDto>> GetMonthlyEmissionsForLastNMonths(int companyId,int numberOfMonths)
        {
            DateTime startDate = DateTime.UtcNow.AddMonths(-numberOfMonths);
            return await _dbContext.Orders.Where(o =>o.CompanyId == companyId &&o.OrderStatus == "placed" &&
                    o.OrderDate >= startDate).GroupBy(o => new{o.OrderDate.Year,o.OrderDate.Month}).Select(g => new MonthlyEmissionStatDto
                {Month = g.Key.Month,TotalEmissions = g.Sum(o => o.OrderCO2Emission)}).OrderBy(m => m.Month).ToListAsync();
        }
        public async Task<string?> GetCompanySector(int companyId)
        {
            return await _dbContext.Companies.Where(c => c.Id == companyId).Select(c => c.CompanySector).FirstOrDefaultAsync();
        }
        public async Task<double> GetAnnualCreditLimitForSector(string sector)
        {
            return await _dbContext.Credits.Where(c => c.Sector == sector).Select(c => c.SectorCredits).FirstOrDefaultAsync();
        }
        public async Task<DateTime> GetCompanyCreationDate(int companyId)
        {
            return await _dbContext.Companies.Where(c => c.Id == companyId).Select(c => c.CompanyCreatedAt).FirstAsync();
        }
        public async Task<double> GetTotalEmissionsTillDate(int companyId,DateTime asOfDate)
        {
            return await _dbContext.Orders.Where(o =>o.CompanyId == companyId &&o.OrderStatus == "placed" &&
                    o.OrderDate <= asOfDate).SumAsync(o => o.OrderCO2Emission);
        }
    }
}
