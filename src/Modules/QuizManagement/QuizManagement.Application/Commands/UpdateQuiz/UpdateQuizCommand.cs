using MediatR;
using Quizora.SharedKernel;

namespace QuizManagement.Application.Commands.UpdateQuiz;

public record UpdateQuizCommand(
    Guid QuizId,
    Guid RequestingUserId,
    string Title,
    string Slug,
    string? Description,
    Guid CategoryId,
    int TimeLimitSeconds,
    double PointsCorrect,
    double PointsWrong,
    double PointsSkipped,
    double? PassingScore,
    bool ShuffleQuestions,
    bool ShuffleOptions) : IRequest<Result>;
