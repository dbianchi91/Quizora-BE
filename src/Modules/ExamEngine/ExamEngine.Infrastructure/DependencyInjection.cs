using ExamEngine.Application;
using ExamEngine.Application.Interfaces;
using ExamEngine.Infrastructure.Endpoints;
using ExamEngine.Infrastructure.Persistence;
using ExamEngine.Infrastructure.Repositories;
using ExamEngine.Infrastructure.Services;
using ExamEngine.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quizora.SharedKernel;

namespace ExamEngine.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddExamEngineModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ExamEngineDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(ExamEngineDbContext).Assembly.FullName)));

        services.AddScoped<IExamRepository, ExamRepository>();
        services.AddScoped<IExamTimerService, ExamTimerService>();
        services.AddHostedService<ExamTimerWorker>();
        services.AddExamEngineApplication();
        services.AddSingleton<IModuleEndpoints, ExamEngineEndpoints>();

        return services;
    }
}
