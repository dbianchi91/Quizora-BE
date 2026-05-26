using FluentValidation;
using Identity.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using QuizManagement.Application.Commands.CreateQuiz;

namespace QuizManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddQuizManagementApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateQuizCommandHandler).Assembly));

        services.AddValidatorsFromAssembly(typeof(CreateQuizCommandValidator).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
