public class ForecastDto
{
    // CURRENT STATUS
    public double EmissionsMTD { get; set; }            // Month-to-date
    public double EmissionsYTD { get; set; }            // Year-to-date

    // FORECAST
    public double ForecastRemainingMonth { get; set; }
    public double ForecastRemainingYear { get; set; }

    // TOTALS
    public double EstimatedMonthEndEmission { get; set; }
    public double EstimatedYearEndEmission { get; set; }

    // BUDGET
    public double AnnualCreditLimit { get; set; }       // From sector
    public double CreditsRemaining { get; set; }

    // DECISION
    public string Recommendation { get; set; }          // BUY / SELL / SAFE
}
