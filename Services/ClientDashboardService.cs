using System.Data.Common;
using System.Transactions;
using EcoRoute.Data;
using EcoRoute.Models;
using EcoRoute.Models.Entities;
using EcoRoute.Models.HelperClasses;
using EcoRoute.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace EcoRoute.Services
{

    public interface IClientDashboardService
    {
        public Task<(bool Success, string Message, ClientDashboardDto? clientDashboardDto)> GetClientDashboardStatAsync(string CompanyName, string EmissionPeriod, string ShipmentPeriod, string EmissionsSavedPeriod);
    
        public Task<double> GetCreditMarketPrice(); 

        public Task<List<CreditListingDto>> GetListing(string companyName);

        public Task<(bool Success, string Message)> PutSale(string companyName, double saleUnits);

        public Task<(bool Success, string Message)> BuyCredits(string companyName, BuyCreditDto buyCreditDto);
        public Task<List<Notification>> ShowNotifications(string companyName);
    }

    public class ClientDashboardService : IClientDashboardService
    {
        private readonly EcoRouteDbContext _dbContext;
        private readonly IUserRepository _userRepo;
        private readonly ICompanyRepository _companyRepo;
        private readonly IEmissionRepository _emissionRepo;
        private readonly IShipmentRepository _shipmentRepo;
        private readonly ICreditRepository _creditRepo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IForeCastRepository _forecastRepo;
        public ClientDashboardService(EcoRouteDbContext _dbContext, IUserRepository _userRepo, 
                            ICompanyRepository _companyRepo, IEmissionRepository _emissionRepo,
                            IShipmentRepository _shipmentRepo, ICreditRepository _creditRepo,
                            INotificationRepository _notificationRepo, IForeCastRepository _forecastRepo)
        {
            this._dbContext = _dbContext;
            this._userRepo = _userRepo;
            this._companyRepo = _companyRepo;
            this._emissionRepo = _emissionRepo;
            this._shipmentRepo = _shipmentRepo;
            this._creditRepo = _creditRepo;
            this._notificationRepo = _notificationRepo;
            this._forecastRepo = _forecastRepo;
        }

        public async Task<(bool Success, string Message, ClientDashboardDto? clientDashboardDto)> GetClientDashboardStatAsync(string CompanyName, string EmissionPeriod, string ShipmentPeriod, string EmissionsSavedPeriod)
        {
            var company = await _companyRepo.GetCompanyByNameAsync(CompanyName);

            if(company == null)
            {
                return (false, $"No company found with the name : {CompanyName}", null);
            }

            DateTime EmissionStartDate;
            DateTime EmissionEndDate = DateTime.Now;

            DateTime ShipmentStartDate;
            DateTime ShipmentEndDate = DateTime.Now;

            DateTime EmissionsSavedStartDate;
            DateTime EmissionsSavedEndDate = DateTime.Now;

            DateTime GraphYearStart = new(DateTime.Now.Year,1,1);
            DateTime GraphNowDate = DateTime.Now;

            switch (EmissionPeriod.ToLower())
            {
                case "year":
                    EmissionStartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    break;
                case "today":
                    EmissionStartDate = DateTime.Today;
                    break;
                case "month":
                default:
                    EmissionStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    break;
            }

            switch (ShipmentPeriod.ToLower())
            {
                case "year":
                    ShipmentStartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    break;
                case "today":
                    ShipmentStartDate = DateTime.Today;
                    break;
                case "month":
                default:
                    ShipmentStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    break;
            }

            switch (EmissionsSavedPeriod.ToLower())
            {
                case "year":
                    EmissionsSavedStartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    break;
                case "today":
                    EmissionsSavedStartDate = DateTime.Today;
                    break;
                case "month":
                default:
                    EmissionsSavedStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    break;
            }      

            var totalEmissions = await _emissionRepo.GetTotalEmissionsFromOrdersCompanyAndDateWise(company.Id, EmissionStartDate, EmissionEndDate);

            var totalEmissionsSaved = await _emissionRepo.GetTotalEmissionsSavedFromOrdersCompanyAndDateWise(company.Id, EmissionsSavedStartDate, EmissionsSavedEndDate);

            var totalShipments = await _shipmentRepo.GetTotalShipmentsCompanyAndDateWise(company.Id, ShipmentStartDate, ShipmentEndDate);

            var forecastDto = await GetCompanyEmissionForecastAsync(company.Id, DateTime.Now); // USED IN THE SECTION BELOW EMISSION CREDIT SYSTEM

            var totalForecastedEmissionsYear = forecastDto.EstimatedYearEndEmission; // USED IN EMISSION CREDIT SYSTEM

            var totalForecastedEmissionsMonth = forecastDto.EstimatedMonthEndEmission;
            
            var creditMarketPrice = await GetCreditMarketPrice();

            var rawData = await _emissionRepo.GetEmissionsDataForGraph(company.Id, GraphYearStart, GraphNowDate);

            
            double[] finalGraphData = new double[12];

            foreach(var rd in rawData)
            {
                finalGraphData[rd.Month - 1] = rd.TotalEmissions;
            }

            var returnDto = new ClientDashboardDto{
                CompanyCode = company.CompanyCode,
                Shipments = totalShipments,
                CompanyCredits = await ThisMonthCompanyCredits(company),
                CreditMarketPrice = creditMarketPrice, 
                TotalEmissions = totalEmissions,
                ForecastedEmissions = totalForecastedEmissionsMonth,
                TotalForecastedEmissions = totalForecastedEmissionsYear, 
                EmissionsSaved = totalEmissionsSaved, 
                GraphData = finalGraphData,
                CompanyEmissionBudget = company.CompanyEmissionBudget,
                RemainingCredits = company.RemainingCredits
            };

            return (true, "stats retrieved successfully", returnDto);
        }

        private async Task<double> ThisMonthCompanyCredits(Company company)
        {
            double creditsUsedThisMonth = await _emissionRepo.GetCurrentMonthEmissionsByCompanyId(company.Id) / 1000;

            double monthlyPlanned = company.CompanyCredits / 12;

            return monthlyPlanned - creditsUsedThisMonth;
        }

        public async Task<ForecastDto> GetCompanyEmissionForecastAsync(int id,DateTime asOfDate)
        {
            // var companyId = await _forecastRepository.GetCompanyIdByName(companyName);
            // if (companyId == null)
            //     throw new Exception($"Company '{companyName}' not found");
            // int id = companyId.Value;
            var companyCreatedAt = await _forecastRepo.GetCompanyCreationDate(id);
            var companySector = await _forecastRepo.GetCompanySector(id)
                                ?? throw new Exception("Company sector not found");
            var annualCreditLimit =await _forecastRepo.GetAnnualCreditLimitForSector(companySector);
            int monthsActive =((asOfDate.Year - companyCreatedAt.Year) * 12) +(asOfDate.Month - companyCreatedAt.Month) + 1;
            DateTime monthStart = new DateTime(asOfDate.Year, asOfDate.Month, 1);
            DateTime yearStart = new DateTime(asOfDate.Year, 1, 1);
            int daysInMonth = DateTime.DaysInMonth(asOfDate.Year, asOfDate.Month);
            int daysPassed = asOfDate.Day;
            int daysRemainingInMonth = daysInMonth - daysPassed;
            int daysRemainingInYear = (new DateTime(asOfDate.Year, 12, 31) - asOfDate).Days;
            double emissionsMTD =await _forecastRepo.GetTotalEmissionsForCompanyBetweenDates(id, monthStart, asOfDate);
            double emissionsYTD =await _forecastRepo.GetTotalEmissionsForCompanyBetweenDates(id, yearStart, asOfDate);
            double dailyAverageEmission;
            if (monthsActive < 1)
            {
                dailyAverageEmission = emissionsMTD / Math.Max(daysPassed, 1);
            }
            else if (monthsActive < 3)
            {
                var monthlyStats = await _forecastRepo.GetMonthlyEmissionsForLastNMonths(id, monthsActive);
                double totalEmission = monthlyStats.Sum(m => m.TotalEmissions);
                dailyAverageEmission = totalEmission / Math.Max(monthlyStats.Count * 30, 1);
            }
            else
            {
                var last3MonthsStats = await _forecastRepo.GetMonthlyEmissionsForLastNMonths(id, 3);
                double totalEmission = last3MonthsStats.Sum(m => m.TotalEmissions);
                dailyAverageEmission = totalEmission / (3 * 30);
            }
            double forecastRemainingMonth = dailyAverageEmission * daysRemainingInMonth;
            double forecastRemainingYear = dailyAverageEmission * daysRemainingInYear;
            double estimatedMonthEndEmission = emissionsMTD + forecastRemainingMonth;
            double estimatedYearEndEmission = emissionsYTD + forecastRemainingYear;
            double creditsRemaining = annualCreditLimit - estimatedYearEndEmission;
            string recommendation =creditsRemaining < 0 ? "BUY" :creditsRemaining > annualCreditLimit * 0.15 ? "SELL" : "SAFE";
            return new ForecastDto
            {
                EmissionsMTD = emissionsMTD,
                EmissionsYTD = emissionsYTD,
                ForecastRemainingMonth = forecastRemainingMonth,
                ForecastRemainingYear = forecastRemainingYear,
                EstimatedMonthEndEmission = estimatedMonthEndEmission,
                EstimatedYearEndEmission = estimatedYearEndEmission,
                AnnualCreditLimit = annualCreditLimit,
                CreditsRemaining = creditsRemaining,
                Recommendation = recommendation
            };
        }

        private async Task<double> CalculateForecastedEmissions(int companyId)
        {
            return 10.0;
        }

        public async Task<double> GetCreditMarketPrice()
        {
            var marketStatus = await _creditRepo.GetMarketSupplyAndDemand();
            
            double marketSupplyTonnes = marketStatus.TotalSupply;
            double marketDeficitTonnes = marketStatus.TotalDeficit;

            double basePrice = 1500.0;
            double minPrice = 500.0;
            double maxPrice = 4000.0;

            double marketPressure = 1.0;

            if(marketSupplyTonnes <= 0)
            {
                marketPressure = 2.0;
            }else if(marketDeficitTonnes <= 0)
            {
                marketPressure = 0.5;
            }
            else
            {
                marketPressure = marketDeficitTonnes / marketSupplyTonnes;
            }

            double creditMarketPrice = basePrice * marketPressure;

            if(creditMarketPrice > maxPrice)
            {
                creditMarketPrice = maxPrice;
            }
            else if(creditMarketPrice < minPrice)
            {
                creditMarketPrice = minPrice;
            }

            return Math.Round(creditMarketPrice, 2);
        }

        public async Task<List<CreditListingDto>> GetListing(string companyName)
        {
            var creditListing = await _creditRepo.GetListingAsync(companyName);

            return creditListing;
        }

        public async Task<(bool Success, string Message)> PutSale(string companyName, double saleUnits)
        {
            var company = await _companyRepo.GetCompanyByNameAsync(companyName);

            int companyId = await _companyRepo.GetCompanyIdByName(companyName);
            if(saleUnits >= company.CompanyCredits)
            {
                return (false,"Low credit balance for the given request!");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var creditMarketPrice = await GetCreditMarketPrice();

            company.CompanyCredits -= saleUnits;
            
            var creditListing = new CreditListing
            {
                CompanyName = companyName,
                SellerCompanyId = companyId,
                CreditsListed = saleUnits,
                PricePerCredit = creditMarketPrice,
                Status = "available"
            };

            await _creditRepo.AddCreditListingAsync(creditListing);

            await _companyRepo.SaveChangesAsync();
            await _creditRepo.SaveChangesAsync();

            await transaction.CommitAsync();

            return (true, "credits listed");
        }

        public async Task<(bool Success, string Message)> BuyCredits(string companyName, BuyCreditDto buyCreditDto)
        {
            var company = await _companyRepo.GetCompanyByNameAsync(companyName);

            var tradedCredit = await _creditRepo.GetCreditListingByIdAsync(buyCreditDto.SaleUnitId);

            if(tradedCredit == null)
            {
                return (false, "The credits are not available to trade");
            }

            if(tradedCredit.Status == "sold" || tradedCredit.Status == "not available")
            {
                return (false, "This listing is already sold");
            }

            if(buyCreditDto.UnitsBought != tradedCredit.CreditsListed)
            {
                return (false, "You must buy the exact number of credits listed");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            tradedCredit.Status = "sold";

            tradedCredit.BuyerCompanyId = company.Id;

            company.CompanyCredits += buyCreditDto.UnitsBought;

            var notification = new Notification(){
                Message = $"Your listed credits of Id: {buyCreditDto.SaleUnitId} - {buyCreditDto.UnitsBought} units are bought by company - {company.CompanyName}",
                IsRead = false,
                TargetCompanyId = await _creditRepo.GetCompanyIdByCreditListingId(buyCreditDto.SaleUnitId)
            };

            try
            {
                await _notificationRepo.AddNotificationAsync(notification);
                await _notificationRepo.SaveChangesAsync();
                await _companyRepo.SaveChangesAsync();
                await _creditRepo.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return (false,"Someone else bought this listing just now");
            }
            
            return (true, "successfully bought credits");
        }

        public async Task<List<Notification>> ShowNotifications(string companyName)
        {
            int companyId = await _companyRepo.GetCompanyIdByName(companyName);

            return await _notificationRepo.GetNotificationsByCompanyIdAsync(companyId);
        }
    }
}