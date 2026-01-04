using EcoRoute.Models.DTOs;
using EcoRoute.Models.Entities;
using EcoRoute.Models.HelperClasses;
using EcoRoute.Repositories;

namespace EcoRoute.Services
{

    public interface IAdminDashboardService
    {
        Task<AdminDashboardDto> GetDashboardStat(string userIdFromToken, string EmissionsPeriod, string ShipmentsPeriod, string EmissionsSavedPeriod);

        Task<List<Notification>> GetNotifications(string userIdFromToken);
    }
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IEmissionRepository _emissionRepo;
        private readonly IShipmentRepository _shipmentRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly ICompanyRepository _companyRepo;
        private readonly INotificationRepository _notifRepo;

        public AdminDashboardService(IEmissionRepository _emissionRepo, IShipmentRepository _shipmentRepo,
                                    IOrderRepository _orderRepo, ICompanyRepository _companyRepo,
                                    INotificationRepository _notifRepo)
        {
            this._emissionRepo = _emissionRepo;
            this._shipmentRepo = _shipmentRepo;
            this._orderRepo = _orderRepo;
            this._companyRepo = _companyRepo;
            this._notifRepo = _notifRepo;
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

            var _now = DateTime.Today;
            DateTime GraphNowDate = _now;
            DateTime GraphYearStart = new DateTime(_now.Year, _now.Month, 1).AddMonths(-11);

            switch (EmissionsPeriod.ToLower())
            {
                case "past 12 months":
                    var now = DateTime.Today;
                    EmissionsStartDate = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
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
                case "past 12 months":
                    var now = DateTime.Today;
                    ShipmentStartDate = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
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
                case "past 12 months":
                    var now = DateTime.Today;
                    EmissionsSavedStartDate = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
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

            for (int i = 0; i < 12; i++)
            {
                var monthDate = GraphYearStart.AddMonths(i);

                var match = rawData.FirstOrDefault(r =>
                    r.Year == monthDate.Year &&
                    r.Month == monthDate.Month
                );

                finalGraphData[i] = match?.TotalEmissions ?? 0;
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

        public async Task<List<Notification>> GetNotifications(string userIdFromToken)
        {
            var transportCompanyId = await _companyRepo.GetCompanyIdByUserId(userIdFromToken);

            var notifs = await _notifRepo.GetNotificationsByCompanyIdAsync(transportCompanyId);

            return notifs;
        }

    }
}