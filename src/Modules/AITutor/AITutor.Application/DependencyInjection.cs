using AITutor.Application.Commands.GenerateStudyPlan;
using AITutor.Application.Services;
using Identity.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AITutor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAITutorApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(GenerateStudyPlanCommandHandler).Assembly));
        services.AddScoped<IUserContextBuilder, UserContextBuilder>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return services;
    }
}
