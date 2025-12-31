using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoRoute.Data; // Your DbContext namespace
using EcoRoute.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using EcoRoute.Models.Entities;
using EcoRoute.Repositories;
using EcoRoute.Services;
using EcoRoute.Models.DTOs;

namespace EcoRoute.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private IAuthService _authService;

        public AuthenticationController(IAuthService _authService)
        {
            this._authService = _authService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] UserSignUpDto userSignUpDto)
        {
            
            var res = await _authService.RegisterUserAsync(userSignUpDto);
        
            bool resBool = res.Success;
            string resMessge = res.Message;

            if (!resBool)
            {
                return BadRequest(resMessge);
            }

            return Ok(resMessge);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            
            var res = await _authService.Login(userLoginDto);

            if (!res.Success)
            {
                return BadRequest(res.Message);    
            }
            
            return Ok(new
            {
                res.Token,
                res.Role,
                res.CompanyName
            });
        }
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
        {
            var res = await _authService.SendOtpAsync(dto.Email);
            return res.Success ? Ok(res.Message) : BadRequest(res.Message);
        }
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var res = await _authService.VerifyOtpAsync(dto.Email, dto.Otp);
            return res.Success ? Ok(res.Message) : BadRequest(res.Message);
        }
        [HttpPost("forgot-password/send-otp")]
        public async Task<IActionResult> ForgotSendOtp([FromBody] SendOtpDto dto)
        {
            var res = await _authService.ForgotSendOtpAsync(dto.Email);
            return res.Success ? Ok(res.Message) : BadRequest(res.Message);
        }
        [HttpPost("forgot-password/verify-otp")]
        public async Task<IActionResult> ForgotVerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var res = await _authService.ForgotVerifyOtpAsync(dto.Email, dto.Otp);
            return res.Success ? Ok(res.Message) : BadRequest(res.Message);
        }
        [HttpPost("forgot-password/reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ForgotPasswordDto dto)
        {
            var res = await _authService.ResetPasswordAsync(dto.Email, dto.NewPassword);
            return res.Success ? Ok(res.Message) : BadRequest(res.Message);
        }
    }
}