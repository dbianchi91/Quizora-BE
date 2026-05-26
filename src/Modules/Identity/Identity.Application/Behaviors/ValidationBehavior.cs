using FluentValidation;
using MediatR;
using Quizora.SharedKernel;

namespace Identity.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var error = new Error($"Validation.{failures[0].PropertyName}", failures[0].ErrorMessage);
            return CreateFailureResult(error);
        }

        return await next(cancellationToken);
    }

    private static TResponse CreateFailureResult(Error error)
    {
        if (typeof(TResponse).IsGenericType)
        {
            var generic = typeof(Result).GetMethods()
                .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod);
            var constructed = generic.MakeGenericMethod(typeof(TResponse).GetGenericArguments()[0]);
            return (TResponse)constructed.Invoke(null, [error])!;
        }
        var method = typeof(Result).GetMethod(nameof(Result.Failure), [typeof(Error)])!;
        return (TResponse)method.Invoke(null, [error])!;
    }
}
