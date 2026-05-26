using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Quizora.SharedKernel;

public static class ModuleEndpointsExtensions
{
    public static WebApplication MapAllModuleEndpoints(this WebApplication app)
    {
        foreach (var module in app.Services.GetServices<IModuleEndpoints>())
            module.MapEndpoints(app);
        return app;
    }
}
