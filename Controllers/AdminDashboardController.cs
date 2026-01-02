using System.Security.Claims;
using EcoRoute.Data;
using EcoRoute.Models.HelperClasses;
using EcoRoute.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRoute.Controllers
{
    [Route("api/admin-dashboard")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(IAdminDashboardService _adminDashboardService)
        {
            this._adminDashboardService = _adminDashboardService;
        }

        [HttpGet("stats")]
        [Authorize]
        public async Task<ActionResult> GetDashboardStatAsync([FromQuery] string EmissionsPeriod = "month", [FromQuery] string ShipmentsPeriod = "month", [FromQuery] string EmissionsSavedPeriod = "year")
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.Name)?.Value;

            if(userIdFromToken == null)
            {
                return BadRequest("User does not exist");
            }

            Console.WriteLine($"transport company Id --------------{userIdFromToken}");
            var adminDashDto = await _adminDashboardService.GetDashboardStat(userIdFromToken, EmissionsPeriod, ShipmentsPeriod, EmissionsSavedPeriod);

            return Ok(adminDashDto);
        } 

        [HttpGet("notifications")]
        public async Task<ActionResult<List<Notification>>> GetNotifications()
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.Name)?.Value;

            if(userIdFromToken == null)
            {
                return BadRequest("User does not exist");
            }

            var notifs = await _adminDashboardService.GetNotifications(userIdFromToken);

            return notifs;
        }
    }
}