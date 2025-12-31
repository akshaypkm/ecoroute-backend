using EcoRoute.Models.DTOs;
using EcoRoute.Repositories;

namespace EcoRoute.Services
{
      public interface IForeCastService
    {
        Task<ForecastDto> GetCompanyEmissionForecastAsync(string companyName,DateTime asOfDate);
    }
    public class ForeCastService : IForeCastService
    {
        private readonly IForeCastRepository _forecastRepository;
        public ForeCastService(IForeCastRepository forecastRepository)
        {
            _forecastRepository = forecastRepository;
        }
        public async Task<ForecastDto> GetCompanyEmissionForecastAsync(string companyName,DateTime asOfDate)
        {
            var companyId = await _forecastRepository.GetCompanyIdByName(companyName);
            if (companyId == null)
                throw new Exception($"Company '{companyName}' not found");
            int id = companyId.Value;
            var companyCreatedAt = await _forecastRepository.GetCompanyCreationDate(id);
            var companySector = await _forecastRepository.GetCompanySector(id)
                                ?? throw new Exception("Company sector not found");
            var annualCreditLimit =await _forecastRepository.GetAnnualCreditLimitForSector(companySector);
            int monthsActive =((asOfDate.Year - companyCreatedAt.Year) * 12) +(asOfDate.Month - companyCreatedAt.Month) + 1;
            DateTime monthStart = new DateTime(asOfDate.Year, asOfDate.Month, 1);
            DateTime yearStart = new DateTime(asOfDate.Year, 1, 1);
            int daysInMonth = DateTime.DaysInMonth(asOfDate.Year, asOfDate.Month);
            int daysPassed = asOfDate.Day;
            int daysRemainingInMonth = daysInMonth - daysPassed;
            int daysRemainingInYear = (new DateTime(asOfDate.Year, 12, 31) - asOfDate).Days;
            double emissionsMTD =await _forecastRepository.GetTotalEmissionsForCompanyBetweenDates(id, monthStart, asOfDate);
            double emissionsYTD =await _forecastRepository.GetTotalEmissionsForCompanyBetweenDates(id, yearStart, asOfDate);
            double dailyAverageEmission;
            if (monthsActive < 1)
            {
                dailyAverageEmission = emissionsMTD / Math.Max(daysPassed, 1);
            }
            else if (monthsActive < 3)
            {
                var monthlyStats = await _forecastRepository.GetMonthlyEmissionsForLastNMonths(id, monthsActive);
                double totalEmission = monthlyStats.Sum(m => m.TotalEmissions);
                dailyAverageEmission = totalEmission / Math.Max(monthlyStats.Count * 30, 1);
            }
            else
            {
                var last3MonthsStats = await _forecastRepository.GetMonthlyEmissionsForLastNMonths(id, 3);
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
    }
}
