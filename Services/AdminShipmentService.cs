using EcoRoute.Models;
using EcoRoute.Models.DTOs;
using EcoRoute.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRoute.Services
{
    public interface IAdminShipmentService
    {
        Task<List<AdminShipmentDto>> GetAllCompanyShipmentsAsync(string userIdFromToken);
    }

    public class AdminShipmentService : IAdminShipmentService
    {
        private readonly IShipmentRepository _shipmentRepo;
        private readonly ICompanyRepository _companyRepo;

        public AdminShipmentService(IShipmentRepository _shipmentRepo, ICompanyRepository _companyRepo)
        {
            this._shipmentRepo = _shipmentRepo;
            this._companyRepo = _companyRepo;
        }

        public async Task<List<AdminShipmentDto>> GetAllCompanyShipmentsAsync(string userIdFromToken)
        {
            int TransportCompanyId = await _companyRepo.GetCompanyIdByUserId(userIdFromToken);
            // Directly get DTOs from repository
            var dtoList = await _shipmentRepo.GetAllAdminShipmentsAsync(TransportCompanyId)
                .ContinueWith(task => task.Result
                    .Select(s => new AdminShipmentDto
                    {
                        ShipmentCode = s.ShipmentCode,
                        ShipmentDate = s.ShipmentDate,
                        CompanyName = s.OrderList.Where(o => !string.IsNullOrWhiteSpace(o.CompanyName))
                                                    .Select(o => o.CompanyName)
                                                        .Distinct()
                                                            .ToArray(),
                        ShipmentOrigin = s.ShipmentOrgin,
                        ShipmentDestination = s.ShipmentDestination,
                        ShipmentTotalItems = s.OrderList.Sum(o => o.OrderTotalItems),
                        ShipmentCO2Emission = s.ShipmentCO2Emission,
                        ShipmentStatus = s.OrderList.FirstOrDefault()?.OrderStatus ?? string.Empty,
                        ShipmentMode = s.ShipmentMode
                    }).ToList()
                );

            return dtoList;
        }
    }
}
