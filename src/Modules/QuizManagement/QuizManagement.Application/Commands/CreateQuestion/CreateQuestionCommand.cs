using MediatR;
using Quizora.SharedKernel;
using QuizManagement.Application.DTOs;

namespace QuizManagement.Application.Commands.CreateQuestion;

public record OptionRequest(string Text, bool IsCorrect);

public record CreateQuestionCommand(
    Guid CreatorId,
    string Text,
    string Difficulty,
    string? Explanation,
    IReadOnlyList<OptionRequest> Options) : IRequest<Result<QuestionDto>>;
