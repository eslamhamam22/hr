using HrSystem.Api.Middleware;
using HrSystem.Api.Services;
using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.Services.Auth;
using HrSystem.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-that-must-be-at-least-32-characters";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "HrSystem";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "HrSystemUsers";

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add authorization policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireHRRole", policy =>
    {
        policy.RequireRole("HR", "Admin");
    })
    .AddPolicy("RequireManagerRole", policy =>
    {
        policy.RequireRole("Manager", "Admin");
    })
    .AddPolicy("RequireAdminRole", policy =>
    {
        policy.RequireRole("Admin");
    });

// Add infrastructure services
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(local);Database=HrSystem;Trusted_Connection=true;";
var smtpEmail = builder.Configuration["Smtp:Email"] ?? "noreply@hrsystem.com";
var smtpServer = builder.Configuration["Smtp:Server"] ?? "smtp.gmail.com";
var smtpPort = int.Parse(builder.Configuration["Smtp:Port"] ?? "587");
var smtpPassword = builder.Configuration["Smtp:Password"] ?? "";
var attendanceDbConnection = builder.Configuration.GetConnectionString("AttendanceDb") ?? connectionString;

builder.Services.AddInfrastructure(connectionString, smtpEmail, smtpServer, smtpPort, smtpPassword, attendanceDbConnection);

// Register application services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAuthService, HrSystem.Application.Services.Auth.AuthService>();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<JwtMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
