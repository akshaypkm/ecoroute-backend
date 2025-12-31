using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks.Sources;
using EcoRoute.Data;
using EcoRoute.Models;
using EcoRoute.Models.Entities;
using EcoRoute.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using EcoRoute.Models.DTOs;
using System.Collections.Specialized;

namespace EcoRoute.Services;
    public interface IAuthService
    {
        Task<(bool Success, string Message)> RegisterUserAsync(UserSignUpDto userSignUpDto);

        Task<(bool Success, string Message, string? Token, string? Role, string? CompanyName)> Login(UserLoginDto userLoginDto);
        Task<(bool Success, string Message)> SendOtpAsync(string email);
        Task<(bool Success, string Message)> VerifyOtpAsync(string email, string otp);
        Task<(bool Success,string Message)> ForgotSendOtpAsync(string email);
        Task<(bool Success,string Message)> ForgotVerifyOtpAsync(string email, string otp);
        Task<(bool Success,string Message)> ResetPasswordAsync(string email, string newPassword);   

    }
    public class AuthService : IAuthService
    {
        private readonly EcoRouteDbContext _dbContext;
        private readonly IUserRepository _userRepo;

        private readonly ICompanyRepository _companyRepo;

        private readonly IConfiguration _configuration;
        private readonly IEmailOtpRepository _otpRepo;
        private readonly IEmailService _emailService;

        
        public AuthService(EcoRouteDbContext _dbContext, IUserRepository _userRepo, 
                            ICompanyRepository _companyRepo, IConfiguration _configuration, IEmailOtpRepository _otpRepo, IEmailService _emailService)
        {
            this._dbContext = _dbContext;
            this._userRepo = _userRepo;
            this._companyRepo = _companyRepo;
            this._configuration = _configuration;
            this._otpRepo = _otpRepo;
            this._emailService = _emailService;
            
        }
        private bool IsStrongPassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) &&
            password.Length >= 6 &&
            password.Any(char.IsUpper) &&
            password.Any(char.IsLower) &&
            password.Any(char.IsDigit) &&
            password.Any(ch => !char.IsLetterOrDigit(ch));
            }

        public async Task<(bool Success, string Message)> RegisterUserAsync(UserSignUpDto userSignUpDto)
        {
            if (await _userRepo.UserExistsUsingEmailAsync(userSignUpDto.Email))
            {
                return (false, "User email already taken!");
            }
            var otp = await _otpRepo.GetOtpByEmailAsync(userSignUpDto.Email);
            if (otp == null || !otp.IsVerified)
            {
                return (false, "Email not verified");
            }

            if (!IsStrongPassword(userSignUpDto.Password))
            {
                 return (false,"Password must be at least 6 characters and include uppercase, lowercase, number, and special character");
                 }
            if(await _userRepo.UserExistsAsync(userSignUpDto.UserId))
            {
                return (false, "User ID already taken!");
            }

            if(userSignUpDto.Role.ToLower() == "admin" && await _companyRepo.CompanyExistsByNameAsync(userSignUpDto.CompanyName))
            {
                return (false, $"Admin account is already created for the company : {userSignUpDto.CompanyName}");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try{            
                var user = new User
                {
                    UserId = userSignUpDto.UserId,
                    CompanyName = userSignUpDto.CompanyName,
                    Email = userSignUpDto.Email,
                    Role = userSignUpDto.Role,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userSignUpDto.Password)
                };

                if(!await _companyRepo.CompanyExistsByNameAsync(userSignUpDto.CompanyName) && !(userSignUpDto.Role == "admin"))
                {
                    var companyCredits = await CalculateCompanyCredits(userSignUpDto.CompanySector);
                    var company = new Company
                    {
                        CompanyName = userSignUpDto.CompanyName,
                        CompanySector = userSignUpDto.CompanySector,
                        CompanyCredits = companyCredits,
                        RemainingCredits = companyCredits / 12,
                        
                        CompanyEmissionBudget = companyCredits * 1000
                    };

                    await _companyRepo.AddCompanyAsync(company);
                    await _companyRepo.SaveChangesAsync();
    
                }
                else if(!await _companyRepo.CompanyExistsByNameAsync(userSignUpDto.CompanyName) && userSignUpDto.Role == "admin")
                {
                    var company =  new Company
                    {
                        CompanyName = userSignUpDto.CompanyName,
                        CompanySector = null,
                        CompanyCredits = 0,
                        CompanyEmissionBudget = 0,
                    };

                    await _companyRepo.AddCompanyAsync(company);
                    await _companyRepo.SaveChangesAsync();
                }
                
                await _userRepo.AddUserAsync(user);
                await _userRepo.SaveChangesAsync();
                
                await transaction.CommitAsync();
            

            return (true, "user registered successfully");
            }
            catch(Exception e)
            {
                await transaction.RollbackAsync();
                return (false, "User registration failed, please do try again");
            }
        }

        
        private async Task<double> CalculateCompanyCredits(string companySector)
        {   
            if(companySector == null)
            {
                return 0.0;
            }

            double companyCredits;
            switch (companySector)
            {
                case "Iron and Steel":
                    companyCredits = await _companyRepo.GetCompanyCreditsBySectorAsync(companySector);
                    break;
                case "Cement Industry":
                    companyCredits = await _companyRepo.GetCompanyCreditsBySectorAsync(companySector);
                    break;
                case "Agriculture":
                    companyCredits = await _companyRepo.GetCompanyCreditsBySectorAsync(companySector);
                    break;
                case "Solid Fuel Manufacturing":
                    companyCredits = await _companyRepo.GetCompanyCreditsBySectorAsync(companySector);
                    break;
                case "Industrial Engineering":
                    companyCredits = await _companyRepo.GetCompanyCreditsBySectorAsync(companySector);
                    break;
                case "Pulp and Paper Industries":
                    companyCredits = await _companyRepo.GetCompanyCreditsBySectorAsync(companySector);
                    break;
                case "Brick Manufacturing":
                    companyCredits = await _companyRepo.GetCompanyCreditsBySectorAsync(companySector);
                    break;
                case "Chemicals":
                    companyCredits = await _companyRepo.GetCompanyCreditsBySectorAsync(companySector);
                    break;
                default:
                    companyCredits = 0.0;
                    break;
            }

            Console.WriteLine($"company credits:============================= ");
            Console.WriteLine($"company credits:============================= {companyCredits}");
            Console.WriteLine($"company credits:============================= ");
            Console.WriteLine($"company credits:============================= {companyCredits}");
            Console.WriteLine($"company credits:============================= ");
            return companyCredits;
        }    

        public async Task<(bool Success, string Message, string? Token, 
        string? Role, string? CompanyName)> Login(UserLoginDto userLoginDto)
        {
            if(!await _userRepo.UserExistsAsync(userLoginDto.UserId))
            {
                return (false, "User Id does not exists",null, null, null);
            }

            var user = await _userRepo.GetUserByIdAsync(userLoginDto.UserId);

            if(!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
            {
                return (false, "Password is incorrect", null, null, null);
            }

            // Console.WriteLine($"ROLE::::::::::::::::{user.Role}");
            string token = CreateToken(user);

            return (true, "Login success", token, user.Role, user.CompanyName);
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserId),
                new Claim("role", user.Role),
                new Claim("CompanyName", user.CompanyName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!
            ));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<(bool Success, string Message)> SendOtpAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            return (false, "Email cannot be empty");
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email.Trim());
            }
            catch (FormatException)
            {
                return (false, "Invalid email format");
            }
            string otp = new Random().Next(100000, 999999).ToString();
            var otpEntity = new EmailOtp
            {
                Email = email.Trim(),
                OtpHash = BCrypt.Net.BCrypt.HashPassword(otp),
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false
            };
            if (await _userRepo.UserExistsUsingEmailAsync(email.Trim()))
            {
                return (false, "User email already taken!");
            }
            await _otpRepo.AddOtpAsync(otpEntity);
            await _emailService.SendOtpEmailAsync(email.Trim(), otp);
            return (true, "OTP sent to email");
        }
        public async Task<(bool Success, string Message)> VerifyOtpAsync(string email, string otp)
        {
            var record = await _otpRepo.GetOtpByEmailAsync(email);
            if (record == null || record.ExpiryTime < DateTime.UtcNow)
            {
                return (false, "OTP expired");
            }
            if (!BCrypt.Net.BCrypt.Verify(otp, record.OtpHash))
            {
                return (false, "Invalid OTP");
            }
            record.IsVerified = true;
            await _otpRepo.UpdateOtpAsync();
            return (true, "Email verified");
        }
        public async Task<(bool Success,string Message)> ForgotSendOtpAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            return (false, "Email cannot be empty");
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email.Trim());
            }
            catch (FormatException)
            {
                return (false, "Invalid email format");
            }
            var user = await _userRepo.GetUserByEmailAsync(email.Trim());
            if(user == null)
            {
                return (false, "Email not registered");
            }
            string otp = new Random().Next(100000, 999999).ToString();
            var otpEntity = new EmailOtp
            {
                Email = email.Trim(),
                OtpHash = BCrypt.Net.BCrypt.HashPassword(otp),
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false
            };
            await _otpRepo.AddOtpAsync(otpEntity);
            await _emailService.ForgotSendOtpEmailAsync(email.Trim(), otp, user.UserId, user.CompanyName);
            return (true, "OTP sent to email");
            }

        public async Task<(bool Success,string Message)> ForgotVerifyOtpAsync(string email, string otp)
        {
            var record = await _otpRepo.GetOtpByEmailAsync(email);
            if (record == null || record.ExpiryTime < DateTime.UtcNow)
            {
                return (false, "OTP expired");
            }
            if (!BCrypt.Net.BCrypt.Verify(otp, record.OtpHash))
            {
                return (false, "Invalid OTP");
            }
            record.IsVerified = true;
            await _otpRepo.UpdateOtpAsync();
            return (true, "OTP verified");
        }
        public async Task<(bool Success,string Message)> ResetPasswordAsync(String email, string newPassword)
        {
            var record = await _otpRepo.GetOtpByEmailAsync(email);
            if (record == null || !record.IsVerified)
            {
                return (false, "Email not verified");
            }

            if (!IsStrongPassword(newPassword))
            {
                 return (false,"Password must be at least 6 characters and include uppercase, lowercase, number, and special character");
            }

            var user = await _userRepo.GetUserByEmailAsync(email);
            if(user == null)
            {
                return (false, "User not found");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepo.UpdateUserAsync(user);
            await _otpRepo.DeleteOtpsByEmailAsync(email);
            return (true, "Password reset successfully");
        }
    

}

