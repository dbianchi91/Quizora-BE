using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using MediatR;
using Quizora.SharedKernel;

namespace Identity.Application.Commands.Register;

public sealed class RegisterCommandHandler(IIdentityRepository repository, IJwtService jwtService)
    : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure) return Result.Failure<AuthResponseDto>(emailResult.Error);

        if (await repository.EmailExistsAsync(request.Email, cancellationToken))
            return Result.Failure<AuthResponseDto>(Error.Conflict("Email"));

        var hash = await repository.HashPasswordAsync(request.Password);

        var userResult = User.Create(emailResult.Value, request.UserName, hash);
        if (userResult.IsFailure) return Result.Failure<AuthResponseDto>(userResult.Error);

        var user = userResult.Value;
        var refreshTokenStr = jwtService.GenerateRefreshToken();
        user.AddRefreshToken(refreshTokenStr, DateTime.UtcNow.AddDays(7));

        await repository.AddAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        var accessToken = jwtService.GenerateAccessToken(user);
        var accessExpiry = jwtService.GetAccessTokenExpiry();

        return Result.Success(new AuthResponseDto(
            accessToken,
            refreshTokenStr,
            accessExpiry,
            new UserDto(user.Id.Value, user.Email.Value, user.UserName,
                user.IsEmailConfirmed, user.IsCreator, user.IsAdmin, user.CreatedAt, user.LastLoginAt)));
    }
}
