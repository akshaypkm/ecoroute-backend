using System.Security.Claims;
using EcoRoute.Data;
using EcoRoute.Models;
using EcoRoute.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoRoute.Controllers
{
    [Route("api/client-shipment-history")]
    [ApiController]
    [Authorize(Roles = "client")]

    public class ClientShipmentHistoryController : ControllerBase
    {
        private readonly EcoRouteDbContext dbContext;

        private readonly IClientShipmentHistoryService _clientShipmentHistoryService;

        public ClientShipmentHistoryController(EcoRouteDbContext dbContext, IClientShipmentHistoryService _clientShipmentHistoryService)
        {
            this.dbContext = dbContext;
            this._clientShipmentHistoryService = _clientShipmentHistoryService;
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetShipmentHistory([FromQuery] string OrderPeriod = "month", string Filter = "bySavings")
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.Name)?.Value;

            var companyClaim = User.FindFirst("CompanyName");

            if(companyClaim == null)
            {
                return Unauthorized("token is missing the right company name");
            }

            string companyName = companyClaim.Value;

            var orderHistoryDtos = await _clientShipmentHistoryService.GetShipmentHistory(companyName, OrderPeriod, Filter);

            return Ok(orderHistoryDtos);
        }
    }
}