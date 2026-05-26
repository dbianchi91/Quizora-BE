using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Quizora.API.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = 500,
                Title = "An unexpected error occurred.",
                Detail = "An internal server error occurred. Please try again later.",
                Extensions = { ["traceId"] = context.TraceIdentifier }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
