using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.ValueObjects;
using MediatR;
using Quizora.SharedKernel;

namespace Identity.Application.Queries.GetMe;

public sealed class GetMeQueryHandler(IIdentityRepository repository)
    : IRequestHandler<GetMeQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(UserId.From(request.UserId), cancellationToken);
        if (user is null) return Result.Failure<UserDto>(Error.NotFound("User"));

        return Result.Success(new UserDto(
            user.Id.Value, user.Email.Value, user.UserName,
            user.IsEmailConfirmed, user.IsCreator, user.IsAdmin, user.CreatedAt, user.LastLoginAt));
    }
}
