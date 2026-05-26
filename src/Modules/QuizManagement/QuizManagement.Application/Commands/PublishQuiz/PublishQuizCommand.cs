using MediatR;
using Quizora.SharedKernel;

namespace QuizManagement.Application.Commands.PublishQuiz;

public record PublishQuizCommand(Guid QuizId, Guid RequestingUserId) : IRequest<Result>;
