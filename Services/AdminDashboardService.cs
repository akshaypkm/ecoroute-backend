using EcoRoute.Models.DTOs;
using EcoRoute.Models.Entities;
using EcoRoute.Repositories;

namespace EcoRoute.Services
{

    public interface IAdminDashboardService
    {
        Task<AdminDashboardDto> GetDashboardStat(string userIdFromToken, string EmissionsPeriod, string ShipmentsPeriod, string EmissionsSavedPeriod);
    }
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IEmissionRepository _emissionRepo;
        private readonly IShipmentRepository _shipmentRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly ICompanyRepository _companyRepo;

        public AdminDashboardService(IEmissionRepository _emissionRepo, IShipmentRepository _shipmentRepo,
                                    IOrderRepository _orderRepo, ICompanyRepository _companyRepo)
        {
            this._emissionRepo = _emissionRepo;
            this._shipmentRepo = _shipmentRepo;
            this._orderRepo = _orderRepo;
            this._companyRepo = _companyRepo;
        }
        public async Task<AdminDashboardDto> GetDashboardStat(string userIdFromToken, string EmissionsPeriod, string ShipmentsPeriod, string EmissionsSavedPeriod)
        {
            int TransportCompanyId = await _companyRepo.GetCompanyIdByUserId(userIdFromToken);

            Console.WriteLine($"$$$$$$$$$::::: TransportCompanyId {TransportCompanyId}");

            Console.WriteLine($"SHIPMENT PERIOD TIMELINE ---------------------------{ShipmentsPeriod}");
            DateTime EmissionsStartDate;
            DateTime EmissionsEndDate = DateTime.Now;

            DateTime ShipmentStartDate;
            DateTime ShipmentEndDate = DateTime.Now;

            DateTime EmissionsSavedStartDate;
            DateTime EmissionsSavedEndDate = DateTime.Now;

            DateTime GraphYearStart = new(DateTime.Now.Year,1,1);
            DateTime GraphNowDate = DateTime.Now;

            switch (EmissionsPeriod.ToLower())
            {
                case "year":
                    EmissionsStartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    break;
                case "today":
                    EmissionsStartDate = DateTime.Today;
                    break;
                case "month":
                default:
                    EmissionsStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    break;
            }

            switch (ShipmentsPeriod.ToLower())
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
            
            var rawData = await _emissionRepo.GetAdminDashGraphEmissionsData(TransportCompanyId, GraphYearStart, GraphNowDate);

            double[] finalGraphData = new double[12];

            foreach(var rd in rawData)
            {
                finalGraphData[rd.Month - 1] = rd.TotalEmissions;
            }


            var adminDashDto = new AdminDashboardDto
            {
                TotalCO2Emissions = await _emissionRepo.GetAdminDashTotalEmissions(TransportCompanyId, EmissionsStartDate, EmissionsEndDate),
                TotalShipments = await _shipmentRepo.GetAdminDashTotalShipments(TransportCompanyId, ShipmentStartDate, ShipmentEndDate),
                TotalOrdersForReview = await _orderRepo.GetAdminDashTotalOrdersForReview(TransportCompanyId),
                TotalEmissionsSaved = await _emissionRepo.GetAdminDashTotalEmissionsSaved(TransportCompanyId, EmissionsSavedStartDate, EmissionsSavedEndDate),
                GraphData = finalGraphData,
                SoFarReviewedCount = await _shipmentRepo.GetSoFarReviewedShipmentCount(TransportCompanyId)
            };

            return adminDashDto;
        }
    }
}