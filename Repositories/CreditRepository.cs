using System;
using EcoRoute.Data;
using EcoRoute.Models;
using EcoRoute.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoRoute.Repositories
{
    public interface ICreditRepository
    {

        Task<(double TotalSupply, double TotalDeficit)> GetMarketSupplyAndDemand();
        Task<List<CreditListingDto>> GetListingAsync(string CompanyName);

        Task AddCreditListingAsync(CreditListing creditListing);

        Task SaveChangesAsync();

        Task<CreditListing> GetCreditListingByIdAsync(int creditListingId);

        Task<int> GetCompanyIdByCreditListingId(int creditListingId);
    }
    public class CreditRepository(EcoRouteDbContext dbContext) : ICreditRepository
    {
        private readonly EcoRouteDbContext dbContext = dbContext;

        public async Task<(double TotalSupply, double TotalDeficit)> GetMarketSupplyAndDemand()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startofNextMonth = startOfMonth.AddMonths(1);

            int monthNumber = now.Month;
            var companyStandings = await dbContext.Companies.Select(c => new
            {
                RemainingCredits = c.RemainingCredits,
                MonthlyPlanned = c.CompanyCredits,
                ActualEmissions = dbContext.Orders.Where(o => o.CompanyId == c.Id
                                        && o.OrderStatus == "placed" 
                                        && o.OrderDate >= startOfMonth
                                        && o.OrderDate < startofNextMonth)
                                        .Sum(o => o.OrderCO2Emission)
            }).ToListAsync();

            double totalSurplusKg = 0.0;
            double totalDeficitKg = 0.0;

            foreach(var item in companyStandings)
            {
                double balance = item.MonthlyPlanned - item.ActualEmissions;

                if(balance > 0)
                {
                    totalSurplusKg += balance;
                }
                else
                {
                    totalDeficitKg += Math.Abs(balance);
                }
            }

            return (totalSurplusKg / 1000, totalDeficitKg / 1000);
        }


        

        public async Task<List<CreditListingDto>> GetListingAsync(string companyName)
        {
            var creditListings = await dbContext.CreditListings.
                                        Where(cl => cl.CompanyName != companyName
                                            && cl.Status == "available")
                                            .Select(cl => new CreditListingDto{
                                                SaleUnitId = cl.Id,
                                                SellerCompanyName = cl.CompanyName,
                                                CreditsListed = cl.CreditsListed,
                                                Status = cl.Status
                                            }).ToListAsync();
            return creditListings;
        }

        public async Task AddCreditListingAsync(CreditListing creditListing)
        {
            await dbContext.CreditListings.AddAsync(creditListing);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public async Task<CreditListing> GetCreditListingByIdAsync(int creditListingId)
        {
            return await dbContext.CreditListings.Where(cl => cl.Id == creditListingId).FirstOrDefaultAsync();
        }

        public async Task<int> GetCompanyIdByCreditListingId(int creditListingId)
        {
            return await dbContext.CreditListings.Where(cl => cl.Id == creditListingId)
                                                .Select(cl => cl.SellerCompanyId).FirstOrDefaultAsync();
        }
    }
}