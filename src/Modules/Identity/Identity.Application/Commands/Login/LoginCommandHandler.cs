using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using MediatR;
using Quizora.SharedKernel;

namespace Identity.Application.Commands.Login;

public sealed class LoginCommandHandler(IIdentityRepository repository, IJwtService jwtService)
    : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null) return Result.Failure<AuthResponseDto>(Error.Unauthorized());

        var passwordValid = await repository.CheckPasswordAsync(user, request.Password);
        if (!passwordValid) return Result.Failure<AuthResponseDto>(Error.Unauthorized());

        user.RecordLogin();

        var refreshTokenStr = jwtService.GenerateRefreshToken();
        user.AddRefreshToken(refreshTokenStr, DateTime.UtcNow.AddDays(7), request.IpAddress);

        repository.Update(user);
        await repository.SaveChangesAsync(cancellationToken);

        var accessToken = jwtService.GenerateAccessToken(user);
        var expiry = jwtService.GetAccessTokenExpiry();

        return Result.Success(new AuthResponseDto(
            accessToken,
            refreshTokenStr,
            expiry,
            new UserDto(user.Id.Value, user.Email.Value, user.UserName,
                user.IsEmailConfirmed, user.IsCreator, user.IsAdmin, user.CreatedAt, user.LastLoginAt)));
    }
}
