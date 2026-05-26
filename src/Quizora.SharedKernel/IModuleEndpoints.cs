using Microsoft.AspNetCore.Routing;

namespace Quizora.SharedKernel;

public interface IModuleEndpoints
{
    void MapEndpoints(IEndpointRouteBuilder app);
}
