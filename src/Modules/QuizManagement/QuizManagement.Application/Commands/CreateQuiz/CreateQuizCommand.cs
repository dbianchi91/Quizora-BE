using MediatR;
using Quizora.SharedKernel;
using QuizManagement.Application.DTOs;

namespace QuizManagement.Application.Commands.CreateQuiz;

public record CreateQuizCommand(
    Guid CreatorId,
    string Title,
    string Slug,
    Guid CategoryId,
    string? Description,
    int TimeLimitSeconds,
    double PointsCorrect,
    double PointsWrong,
    double PointsSkipped,
    double? PassingScore,
    bool ShuffleQuestions,
    bool ShuffleOptions) : IRequest<Result<QuizSummaryDto>>;
