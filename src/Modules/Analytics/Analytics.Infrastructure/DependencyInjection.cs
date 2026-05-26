using Analytics.Application;
using Analytics.Application.Interfaces;
using Analytics.Infrastructure.Endpoints;
using Analytics.Infrastructure.Persistence;
using Analytics.Infrastructure.Queries;
using Analytics.Infrastructure.Repositories;
using Analytics.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quizora.SharedKernel;

namespace Analytics.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalyticsModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AnalyticsDbContext).Assembly.FullName)));

        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        services.AddScoped<IAnalyticsDapperQueries, AnalyticsDapperQueries>();
        services.AddAnalyticsApplication();
        services.AddSingleton<IModuleEndpoints, AnalyticsEndpoints>();

        return services;
    }
}
