using Microsoft.AspNetCore.Http;

namespace Quizora.SharedKernel;

public static class HttpErrorMapper
{
    public static IResult ToHttpResult(Error error) => error.Code switch
    {
        var c when c.Contains("NotFound")     => Results.Problem(detail: error.Description, statusCode: 404, title: "Not Found"),
        var c when c.Contains("Conflict")     => Results.Problem(detail: error.Description, statusCode: 409, title: "Conflict"),
        var c when c.Contains("Unauthorized") => Results.Problem(detail: error.Description, statusCode: 401, title: "Unauthorized"),
        var c when c.Contains("Validation")   => Results.Problem(detail: error.Description, statusCode: 422, title: "Validation Error"),
        _                                     => Results.Problem(detail: error.Description, statusCode: 400, title: "Bad Request")
    };
}
