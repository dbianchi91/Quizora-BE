using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using MediatR;
using Quizora.SharedKernel;

namespace Identity.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(IIdentityRepository repository, IJwtService jwtService)
    : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByRefreshTokenAsync(request.Token, cancellationToken);
        if (user is null) return Result.Failure<AuthResponseDto>(Error.Unauthorized());

        var oldToken = user.FindActiveRefreshToken(request.Token);
        if (oldToken is null) return Result.Failure<AuthResponseDto>(Error.Unauthorized());

        oldToken.Revoke();

        var newRefreshToken = jwtService.GenerateRefreshToken();
        user.AddRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7), request.IpAddress);

        repository.Update(user);
        await repository.SaveChangesAsync(cancellationToken);

        var accessToken = jwtService.GenerateAccessToken(user);
        var expiry = jwtService.GetAccessTokenExpiry();

        return Result.Success(new AuthResponseDto(
            accessToken,
            newRefreshToken,
            expiry,
            new UserDto(user.Id.Value, user.Email.Value, user.UserName,
                user.IsEmailConfirmed, user.IsCreator, user.IsAdmin, user.CreatedAt, user.LastLoginAt)));
    }
}
