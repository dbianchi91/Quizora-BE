using AITutor.Application;
using AITutor.Application.Interfaces;
using AITutor.Infrastructure.Endpoints;
using AITutor.Infrastructure.Persistence;
using AITutor.Infrastructure.Providers;
using AITutor.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quizora.SharedKernel;

namespace AITutor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAITutorModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AITutorDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AITutorDbContext).Assembly.FullName)));

        services.AddScoped<IAIProvider, ClaudeAIProvider>();
        services.AddScoped<IAITutorRepository, AITutorRepository>();
        services.AddAITutorApplication();
        services.AddSingleton<IModuleEndpoints, AITutorEndpoints>();

        return services;
    }
}
