using EcoRoute.Data;
using EcoRoute.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EcoRoute.Repositories
{

    public interface IEmissionRepository
    {
        Task<double> GetTotalEmissionsFromOrdersCompanyAndDateWise(int companyId, DateTime EmissionStartDate, DateTime EmisisonEndDate);
    
        Task<double> GetTotalEmissionsSavedFromOrdersCompanyAndDateWise(int companyId, DateTime EmissionsSavedStartDate, DateTime EmissionsSavedEndDate);

        Task<IEnumerable<MonthlyEmissionStatDto>> GetEmissionsDataForGraph(int companyId, DateTime GraphYearStart, DateTime GraphNowDate);

        Task<double> GetCurrentMonthEmissionsByCompanyId(int companyId);


        // CONTRACTS FOR ADMIN

        Task<double> GetAdminDashTotalEmissions(int TransportCompanyId, DateTime EmissionsStartDate, DateTime EmissionsEndDate);

        Task<double> GetAdminDashTotalEmissionsSaved(int TransportCompanyId, DateTime EmissionsSavedStartDate, DateTime EmissionsSavedEndDate);
    
        Task<IEnumerable<MonthlyEmissionStatDto>> GetAdminDashGraphEmissionsData(int TransportCompanyId, DateTime GraphYearStart, DateTime GraphNowDate);
    }
    public class EmissionRepository : IEmissionRepository
    {
        private readonly EcoRouteDbContext dbContext;

        public EmissionRepository(EcoRouteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<double> GetTotalEmissionsFromOrdersCompanyAndDateWise(int companyId, DateTime EmissionStartDate, DateTime EmissionEndDate)
        {
            return await dbContext.Orders.Where(o => o.CompanyId == companyId && o.OrderDate >= EmissionStartDate 
                                                            && o.OrderDate <= EmissionEndDate && o.OrderStatus == "placed" )
                                                            .SumAsync(o => o.OrderCO2Emission);
        }

        public async Task<double> GetTotalEmissionsSavedFromOrdersCompanyAndDateWise(int companyId, DateTime EmissionsSavedStartDate, DateTime EmissionsSavedEndDate)
        {
            return await dbContext.Orders.Where(o => o.CompanyId == companyId && o.OrderDate >= EmissionsSavedStartDate
                                                            && o.OrderDate <= EmissionsSavedEndDate && o.OrderStatus == "placed")
                                                            .SumAsync(o => o.OrderStandardCO2Emissions - o.OrderCO2Emission);
        }

        public async Task<IEnumerable<MonthlyEmissionStatDto>> GetEmissionsDataForGraph(int companyId, DateTime GraphYearStart, DateTime GraphNowDate)
        {
            return await dbContext.Orders.Where(o => o.CompanyId == companyId && o.OrderStatus == "placed"
                                                && o.OrderDate>= GraphYearStart && o.OrderDate <= GraphNowDate)
                                                    .GroupBy(o => o.OrderDate.Month)
                                                        .Select(s => new MonthlyEmissionStatDto
                                                        {
                                                            Month = s.Key,
                                                            TotalEmissions = s.Sum(s => s.OrderCO2Emission)
                                                        }).OrderBy(o => o.Month).ToListAsync();
        }


        
        public async Task<double> GetCurrentMonthEmissionsByCompanyId(int companyId)
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfNextMonth = startOfMonth.AddMonths(1);

            return await dbContext.Orders
                .Where(o =>
                    o.CompanyId == companyId &&
                    o.OrderStatus == "placed" &&
                    o.OrderDate >= startOfMonth &&
                    o.OrderDate < startOfNextMonth
                )
                .SumAsync(o => o.OrderCO2Emission);
        }






        // METHODS FOR ADMIN

        public async Task<double> GetAdminDashTotalEmissions(int TransportCompanyId, DateTime EmissionsStartDate, DateTime EmissionsEndDate)
        {
            return await dbContext.Shipments.Where(o => o.TransportCompanyId == TransportCompanyId &&
                                             o.ShipmentDate >= EmissionsStartDate
                                            && o.ShipmentDate <= EmissionsEndDate)
                                            .SumAsync(o => o.ShipmentCO2Emission);
        }

        public async Task<double> GetAdminDashTotalEmissionsSaved(int TransportCompanyId, DateTime EmissionsSavedStartDate, DateTime EmissionsSavedEndDate)
        {
            return await dbContext.Orders.Where(o => o.TransportCompanyId == TransportCompanyId && o.OrderStatus == "placed"
                                           && o.OrderDate >= EmissionsSavedStartDate
                                           && o.OrderDate <= EmissionsSavedEndDate)
                                           .SumAsync(o => o.OrderStandardCO2Emissions - o.OrderCO2Emission);
        }

        public async Task<IEnumerable<MonthlyEmissionStatDto>> GetAdminDashGraphEmissionsData(int TransportCompanyId, DateTime GraphYearStart, DateTime GraphNowDate)
        {
            return await dbContext.Shipments.Where(s => s.TransportCompanyId == TransportCompanyId &&
                                                s.ShipmentDate >= GraphYearStart
                                                    && s.ShipmentDate <= GraphNowDate)
                                                        .GroupBy(s => s.ShipmentDate.Month)
                                                            .Select(s => new MonthlyEmissionStatDto
                                                            {
                                                                Month = s.Key,
                                                                TotalEmissions = s.Sum(s => s.ShipmentCO2Emission)
                                                            }).OrderBy(s => s.Month).ToListAsync();

        }
    }
}