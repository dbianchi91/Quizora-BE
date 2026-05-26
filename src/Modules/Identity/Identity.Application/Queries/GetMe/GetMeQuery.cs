using Identity.Application.DTOs;
using MediatR;
using Quizora.SharedKernel;

namespace Identity.Application.Queries.GetMe;

public record GetMeQuery(Guid UserId) : IRequest<Result<UserDto>>;
