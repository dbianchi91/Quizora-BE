using Analytics.Application.EventHandlers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalyticsApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ExamCompletedAnalyticsHandler).Assembly));
        return services;
    }
}
