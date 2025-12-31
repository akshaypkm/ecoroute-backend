using EcoRoute.Data;
using EcoRoute.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoRoute.Repositories
{

    public interface ICompanyRepository
    {
        Task<bool> CompanyExistsByNameAsync(string CompanyName);

        Task<Company> GetCompanyByNameAsync(string CompanyName);

        Task AddCompanyAsync(Company company);

        Task SaveChangesAsync();

        Task<double> GetCompanyCreditsByNameAsync(string CompanyName);

        Task<int> GetCompanyIdByName(string CompanyName);

        Task<double> GetCompanyCreditsBySectorAsync(string CompanySector);

        Task UpdateCompanyCreditsAsync(int CompanyId, double OrderCO2Emission);

        Task<string> GetCompanyNameById(int CompanyId);

        Task RefundCompanyCredits(int companyId, double refundCredits);

        Task<int> GetTransportCompanyIdByCompanyName(string transportCompanyName);

        Task<int> GetCompanyIdByUserId(string userIdFromToken);

        Task<List<string>> GetTransportProviders();
        
    }
    public class CompanyRepository : ICompanyRepository
    {
        private readonly EcoRouteDbContext dbContext;
        public CompanyRepository(EcoRouteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Company> GetCompanyByNameAsync(string CompanyName)
        {
            return await dbContext.Companies.FirstOrDefaultAsync(c => c.CompanyName == CompanyName);
        }

        public async Task<bool> CompanyExistsByNameAsync(string CompanyName)
        {
            return await dbContext.Companies.AnyAsync(c => c.CompanyName == CompanyName);
        }

        public async Task AddCompanyAsync(Company company)
        {
            await dbContext.Companies.AddAsync(company);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public async Task<double> GetCompanyCreditsByNameAsync(string CompanyName)
        {
            return await dbContext.Companies.Where(c => c.CompanyName == CompanyName)
                                            .Select(c => c.CompanyCredits).FirstOrDefaultAsync();
        }

        public async Task<int> GetCompanyIdByName(string CompanyName)
        {
            return await dbContext.Companies.Where(c => c.CompanyName == CompanyName)
                                            .Select(c => c.Id).FirstOrDefaultAsync();
        }

        public async Task<double> GetCompanyCreditsBySectorAsync(string CompanySector)
        {
            var credits =  await dbContext.Credits.Where(c => c.Sector.ToLower().Equals(CompanySector.ToLower()))
                                            .Select(c => c.SectorCredits).FirstOrDefaultAsync();
            
            Console.WriteLine($"CREDITs::::::::::::::::::::::::::::{credits}");

            return credits;
        }

        public async Task UpdateCompanyCreditsAsync(int CompanyId, double OrderCO2Emission)
        {
            var company = await dbContext.Companies.Where(c => c.Id == CompanyId).FirstOrDefaultAsync();

            double creditsSpent = OrderCO2Emission / 1000;

            company.RemainingCredits -= creditsSpent;

            await dbContext.SaveChangesAsync();
        }

        public async Task<string> GetCompanyNameById(int CompanyId)
        {
            return await dbContext.Companies.Where(c => c.Id == CompanyId).Select(c => c.CompanyName).FirstOrDefaultAsync();
        }

        public async Task RefundCompanyCredits(int companyId, double refundCredits)
        {
            var company = await dbContext.Companies.Where(c => c.Id == companyId).FirstOrDefaultAsync();

            company.RemainingCredits += refundCredits;
            await dbContext.SaveChangesAsync();
        }

        public async Task<int> GetTransportCompanyIdByCompanyName(string transportCompanyName)
        {
            return await dbContext.Companies.Where(c => c.CompanyName == transportCompanyName)
                                                .Select(c => c.Id).FirstOrDefaultAsync();
        }

        public async Task<int> GetCompanyIdByUserId(string userIdFromToken)
        {
            string companyName = await dbContext.Users.Where(c => c.UserId == userIdFromToken).Select(c => c.CompanyName).FirstOrDefaultAsync();

            Console.WriteLine($"company name that is associated with the user id: ---------->->->->->->->->->->->->->-> {companyName}");
            int id = await GetCompanyIdByName(companyName);

            Console.WriteLine($"company id: ---------->->->->->->->->->->->->->-> {id}");

            return id;
        }

        public async Task<List<string>> GetTransportProviders()
        {
            return await dbContext.Companies.Where(c => c.CompanySector == null)
                                                .Select(c => c.CompanyName.Trim())
                                                    .Distinct()
                                                        .OrderBy(name => name)
                                                            .ToListAsync();
        }
    }
}
