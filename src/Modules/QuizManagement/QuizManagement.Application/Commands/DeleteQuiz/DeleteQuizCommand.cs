using MediatR;
using Quizora.SharedKernel;

namespace QuizManagement.Application.Commands.DeleteQuiz;

public record DeleteQuizCommand(Guid QuizId, Guid RequestingUserId) : IRequest<Result>;
