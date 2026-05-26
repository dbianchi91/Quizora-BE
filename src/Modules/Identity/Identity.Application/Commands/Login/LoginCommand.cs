using Identity.Application.DTOs;
using MediatR;
using Quizora.SharedKernel;

namespace Identity.Application.Commands.Login;

public record LoginCommand(string Email, string Password, string? IpAddress = null)
    : IRequest<Result<AuthResponseDto>>;
