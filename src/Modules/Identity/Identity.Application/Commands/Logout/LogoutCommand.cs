using MediatR;
using Quizora.SharedKernel;

namespace Identity.Application.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;
