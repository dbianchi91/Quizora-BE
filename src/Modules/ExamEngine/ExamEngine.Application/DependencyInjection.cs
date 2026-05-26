using ExamEngine.Application.Commands.StartExam;
using FluentValidation;
using Identity.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ExamEngine.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddExamEngineApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(StartExamCommandHandler).Assembly));
        services.AddValidatorsFromAssembly(typeof(StartExamCommandValidator).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
