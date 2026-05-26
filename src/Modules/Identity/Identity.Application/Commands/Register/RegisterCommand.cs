using Identity.Application.DTOs;
using MediatR;
using Quizora.SharedKernel;

namespace Identity.Application.Commands.Register;

public record RegisterCommand(string Email, string UserName, string Password)
    : IRequest<Result<AuthResponseDto>>;
