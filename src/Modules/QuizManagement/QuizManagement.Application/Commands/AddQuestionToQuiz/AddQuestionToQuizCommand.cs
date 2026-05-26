using MediatR;
using Quizora.SharedKernel;

namespace QuizManagement.Application.Commands.AddQuestionToQuiz;

public record AddQuestionToQuizCommand(Guid QuizId, Guid QuestionId, Guid RequestingUserId) : IRequest<Result>;
