using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quizora.SharedKernel;
using QuizManagement.Application;
using QuizManagement.Application.Interfaces;
using QuizManagement.Infrastructure.Endpoints;
using QuizManagement.Infrastructure.Persistence;
using QuizManagement.Infrastructure.Queries;
using QuizManagement.Infrastructure.Repositories;

namespace QuizManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddQuizManagementModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<QuizManagementDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(QuizManagementDbContext).Assembly.FullName)));

        services.AddScoped<IQuizManagementRepository, QuizManagementRepository>();
        services.AddScoped<IQuizReader, QuizReader>();
        services.AddQuizManagementApplication();
        services.AddSingleton<IModuleEndpoints, QuizManagementEndpoints>();

        return services;
    }
}
