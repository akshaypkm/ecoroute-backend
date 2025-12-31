using System.Text;
using EcoRoute.Data;
using EcoRoute.Models.Entities;
using EcoRoute.Repositories;
using EcoRoute.Services;
using EcoRoute.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<EcoRouteDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("https://gentle-river-078dc2800.2.azurestaticapps.net", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Source - https://stackoverflow.com/a
// Posted by preetham-p-m, modified by community. See post 'Timeline' for change history
// Retrieved 2025-12-04, License - CC BY-SA 4.0

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        }
    );
    option.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        }
    );
});


builder.Services.AddAuthentication(options =>
{
    // This tells .NET: "By default, use JWT tokens for everything."
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration.GetSection("AppSettings:Token").Value!)), // Matches your AppSettings
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClientDashboardService, ClientDashboardService>();
builder.Services.AddScoped<ICarbonQuoteService, CarbonQuoteService>();
builder.Services.AddScoped<IClientShipmentHistoryService, ClientShipmentHistoryService>();
builder.Services.AddScoped<IForeCastService,ForeCastService>();
builder.Services.AddScoped<IAdminShipmentService, AdminShipmentService>();
builder.Services.AddScoped<IAdminShipmentReviewService, AdminShipmentReviewService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminShipmentService, AdminShipmentService>();
builder.Services.AddScoped<RouteOptimizationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAutoApprovalService, AutoApprovalService>();

builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICreditRepository, CreditRepository>();
builder.Services.AddScoped<IEmissionRepository, EmissionRepository>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITruckRepository, TruckRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IForeCastRepository,ForeCastRepository>();
builder.Services.AddScoped<IEmailOtpRepository, EmailOtpRepository>();

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoRoute API v1");
}); 

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication(); 
app.UseAuthorization();



app.MapControllers();

app.Run();
