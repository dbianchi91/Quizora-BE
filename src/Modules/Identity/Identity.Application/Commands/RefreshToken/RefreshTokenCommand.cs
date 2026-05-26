using Identity.Application.DTOs;
using MediatR;
using Quizora.SharedKernel;

namespace Identity.Application.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, string? IpAddress = null)
    : IRequest<Result<AuthResponseDto>>;
