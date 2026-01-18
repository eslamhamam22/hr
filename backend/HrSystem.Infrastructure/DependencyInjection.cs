using HrSystem.Application.Common.Interfaces;
using HrSystem.Domain.Interfaces;
using HrSystem.Infrastructure.Data.Context;
using HrSystem.Infrastructure.Data.Repositories;
using HrSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrSystem.Infrastructure;

/// <summary>
/// Dependency injection extension for infrastructure services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string smtpEmail,
        string smtpServer,
        int smtpPort,
        string smtpPassword,
        string attendanceDbConnection)
    {
        // Register DbContext
        services.AddDbContext<HrDbContext>(options =>
            options.UseSqlServer(connectionString,
                builder => builder.EnableRetryOnFailure()));

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<LeaveRepository>();

        // Register application db context
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<HrDbContext>());

        // Register external services
        services.AddScoped<IEmailService>(provider =>
            new SmtpEmailService(smtpEmail, smtpServer, smtpPort, smtpPassword));

        services.AddScoped<IAttendanceProvider>(provider =>
            new SqlAttendanceProvider(attendanceDbConnection));

        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }
}
