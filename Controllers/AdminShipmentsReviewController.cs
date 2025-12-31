using System.Security.Claims;
using EcoRoute.Models;
using EcoRoute.Models.DTOs;
using EcoRoute.Repositories;
using EcoRoute.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoRoute.Controllers
{

    [Route("api/admin-shipments-review")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AdminShipmentsReviewController : ControllerBase
    {
        private readonly IAdminShipmentReviewService _adminShipmentReviewService;

        public AdminShipmentsReviewController(IAdminShipmentReviewService _adminShipmentReviewService)
        {
            this._adminShipmentReviewService = _adminShipmentReviewService;
        }

        [HttpGet("get-review-shipments")]
        [Authorize]
        public async Task<IActionResult> GetShipmentsForReview()
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.Name)?.Value;

            if(userIdFromToken == null)
            {
                return BadRequest("User does not exist");
            }
            
            var result = await _adminShipmentReviewService.GetShipmentsForReview(userIdFromToken);


            return Ok(result);
        }

        [HttpPost("approve")]
        public async Task<IActionResult> ApproveShipment([FromBody] OrderDto orderDto)
        {

            await _adminShipmentReviewService.ApproveShipment(orderDto);

            return Ok("inserted shipment and updated order status");
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelShipment([FromBody] OrderDto orderDto)
        {

            await _adminShipmentReviewService.CancelShipment(orderDto);

            return Ok();
        }

        [HttpPost("improvise-shipment")]
        public async Task<IActionResult> ImproviseShipment([FromBody] List<OrderDto> orderDtos)
        {
            var result = await _adminShipmentReviewService.ImproviseShipments(orderDtos);
            return Ok(result);
        }

        [HttpPost("improvise-shipment-approve")]
        public async Task<IActionResult> ApproveGroup([FromBody] ImproviseShipmentGroupDto groupDto)
        {
            await _adminShipmentReviewService.ApproveGroupedShipment(groupDto);

            return Ok("order statuses changed");
        }

        [HttpPost("collapse-auto-approve")]
        public async Task CollapseAutoApprove()
        {
            var companyClaim = User.FindFirst("CompanyName");

            if(companyClaim == null)
            {
                Unauthorized("token is missing the right company name");
            } 

            string companyName = companyClaim.Value;

            Console.WriteLine($"THIS IS THE COMPANY NAME: ++++++++++++++++++{companyName}");
            await _adminShipmentReviewService.CollapseAutoApprove(companyName);
        }
    }   
}