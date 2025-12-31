using EcoRoute.Data;
using EcoRoute.Models.Entities;
using Microsoft.EntityFrameworkCore;
using EcoRoute.Models.DTOs;
using EcoRoute.Repositories;
using System.Net;
using System.Net.Mail;
namespace EcoRoute.Services;
public interface IEmailService
{
    Task SendOtpEmailAsync(string email, string otp);
    Task ForgotSendOtpEmailAsync(string email,string otp,string userId,string companyName);

}
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendOtpEmailAsync(string email, string otp)
    {
        var smtp = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(
                _config["EmailSettings:Email"],
                _config["EmailSettings:Password"]
            ),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(_config["EmailSettings:Email"], "EcoRoute Team"),
            Subject = "EcoRoute - Verify Your Email Address",
            IsBodyHtml = true,
            Body = $@"
            <div style='font-family: Arial, sans-serif; color: #333;'>
              <h2 style='color: #2d6a4f;'>Hello,</h2>
              <p>Thank you for registering with <strong>EcoRoute</strong>.</p>
              <p>Your OTP for email verification is:</p>
              <h3 style='color: #1b4332;'>{otp}</h3>
              <p style='font-size: 14px; color: #555;'>This OTP will expire in 5 minutes.</p>
              <br/>
              <p>Best regards,<br/>
              <strong>EcoRoute Team</strong><br/>
              <a href='https://gentle-river-078dc2800.2.azurestaticapps.net/' target='_blank'>www.ecoroute.com</a></p>
              <hr/>
              <p style='font-size: 12px; color: #999;'>If you did not request this, please ignore this email.</p>
            </div>"
        };
        message.To.Add(email);
        await smtp.SendMailAsync(message);
    }
    public async Task ForgotSendOtpEmailAsync(string email,string otp,string userId,string companyName)
    {
        var smtp = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(
                _config["EmailSettings:Email"],
                _config["EmailSettings:Password"]
            ),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(_config["EmailSettings:Email"], "EcoRoute Team"),
            Subject = "EcoRoute - Verify Your Email Address",
            IsBodyHtml = true,
            Body = $@"
            <div style='font-family: Arial;'>
              <h2>Password Reset Request</h2>
              <p><strong>Company:</strong> {companyName}</p>
              <p><strong>User ID:</strong> {userId}</p>
              <p>Your OTP is:</p>
              <h3 style='color:#2d6a4f'>{otp}</h3>
              <p>This OTP expires in 5 minutes.</p>
              <br/>
              <p>If you didn’t request this, please ignore this email.</p>
              <br/>
              <strong>EcoRoute Team</strong>
            </div>"
        };
        message.To.Add(email);
        await smtp.SendMailAsync(message);
    }
    
}
