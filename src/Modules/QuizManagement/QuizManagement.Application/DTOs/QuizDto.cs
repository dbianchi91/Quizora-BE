namespace QuizManagement.Application.DTOs;

public sealed record ExamConfigDto(int TimeLimitSeconds, double PointsCorrect,
    double PointsWrong, double PointsSkipped, double? PassingScore,
    bool ShuffleQuestions, bool ShuffleOptions);

public sealed record QuizSummaryDto(Guid Id, string Title, string Slug, string? Description,
    string Status, Guid CategoryId, string CategoryName, int QuestionCount, ExamConfigDto ExamConfig,
    DateTime CreatedAt);

public sealed record QuizDetailDto(Guid Id, string Title, string Slug, string? Description,
    string Status, Guid CategoryId, string CategoryName, ExamConfigDto ExamConfig,
    IReadOnlyList<QuestionInQuizDto> Questions, IReadOnlyList<string> Tags, DateTime CreatedAt);

public sealed record QuestionInQuizDto(Guid Id, string Text, string? Explanation,
    string Difficulty, IReadOnlyList<OptionDto> Options, int OrderIndex);

// IsCorrect not exposed in public DTO — only via IQuizReader for server-side scoring
public sealed record OptionDto(Guid Id, string Text, int OrderIndex);

public sealed record QuestionDto(Guid Id, string Text, string? Explanation,
    string Difficulty, IReadOnlyList<OptionDto> Options, DateTime CreatedAt);

public sealed record CategoryDto(Guid Id, string Name, string Slug, Guid? ParentId, int OrderIndex);
