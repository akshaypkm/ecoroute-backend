using System.Security.Claims;
using EcoRoute.Data;
using EcoRoute.Models;
using EcoRoute.Models.Entities;
using EcoRoute.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
namespace EcoRoute.Controllers
{
    [Route("api/admin/shipments")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminShipmentController : ControllerBase
    {
        private readonly IAdminShipmentService _AdminShipmentService;

        public AdminShipmentController(IAdminShipmentService shipmentService)
        {
            _AdminShipmentService = shipmentService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AdminShipmentDto>>> GetAllCompanyNamesOfShipmentCodes()
        {
            
            var userIdFromToken = User.FindFirst(ClaimTypes.Name)?.Value;

            if(userIdFromToken == null)
            {
                return BadRequest("User does not exist");
            }

            var result = await _AdminShipmentService.GetAllCompanyShipmentsAsync(userIdFromToken);

            return Ok(result);
        }
    }
    
}