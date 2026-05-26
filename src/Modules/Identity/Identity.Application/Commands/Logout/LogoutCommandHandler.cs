using Identity.Application.Interfaces;
using MediatR;
using Quizora.SharedKernel;

namespace Identity.Application.Commands.Logout;

public sealed class LogoutCommandHandler(IIdentityRepository repository)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user is null) return Result.Failure(Error.NotFound("RefreshToken"));

        user.RevokeRefreshToken(request.RefreshToken);
        repository.Update(user);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
