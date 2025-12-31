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
    [Route("api/calculate-carbon-quote")]
    [ApiController]
    [Authorize(Roles = "client")]
    public class CarbonQuoteCalculatorController : ControllerBase
    {
        private readonly EcoRouteDbContext dbContext;

        private readonly IConfiguration _configuration;

        private readonly ICarbonQuoteService _carbonQuoteService;

        public CarbonQuoteCalculatorController(EcoRouteDbContext dbContext, IConfiguration _configuration,
                                            ICarbonQuoteService _carbonQuoteService)
        {
            this.dbContext = dbContext;
            this._configuration = _configuration;
            this._carbonQuoteService = _carbonQuoteService;
        }

        [HttpPost("calc")]
        [Authorize]
        public async Task<ActionResult<List<OrderDto>?>> PostDataToCalculate([FromBody] OrderRequestDto orderRequestDto)
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.Name)?.Value;

            var companyClaim = User.FindFirst("CompanyName");

            if(companyClaim == null)
            {
                return Unauthorized("token is missing the right company name");
            }
            Console.WriteLine($"ORder request: DATE::::: {orderRequestDto.OrderDate}");
            if (orderRequestDto.OrderLength > 18.75 &&
                orderRequestDto.OrderWidth > 5.0 &&
                orderRequestDto.OrderHeight > 4.0)
            {
                return BadRequest("provide valid dimensions for the payload");
            }

            string companyName = companyClaim.Value;

            var res = await _carbonQuoteService.PostDataToCalculate(companyName, orderRequestDto);

            if (!res.Success)
            {
                return null;
            }
            return Ok(res.OrderDto);
        }

        [HttpPost("place-order")]
        [Authorize]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderDto orderDto)
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.Name)?.Value;
            var companyClaim = User.FindFirst("CompanyName");

            if(companyClaim == null)
            {
                return Unauthorized("token is missing the right company name");
            } 
            string companyName = companyClaim.Value;

            var res = await _carbonQuoteService.PlaceOrder(companyName, orderDto);
            if (!res.Success)
            {
                return BadRequest($"order request failed due to : {res.Message}");
            }

            return Ok("Order request has been placed");
        }

        [HttpGet("transport-providers")]
        [Authorize]
        public async Task<ActionResult<List<string>>> GetTransportProviders()
        {
            var list = await _carbonQuoteService.GetTranportProviders();

            return Ok(list);
        }
    }
}