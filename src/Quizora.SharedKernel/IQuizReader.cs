namespace Quizora.SharedKernel;

public interface IQuizReader
{
    Task<QuizForExamDto?> GetQuizForExamAsync(Guid quizId, CancellationToken ct);
}

public sealed record QuizForExamDto(
    Guid QuizId,
    string Title,
    Guid CategoryId,
    int TotalQuestions,
    int TimeLimitSeconds,
    double PointsCorrect,
    double PointsWrong,
    double PointsSkipped,
    double? PassingScore,
    bool ShuffleQuestions,
    bool ShuffleOptions,
    IReadOnlyList<ExamQuestionDto> Questions);

public sealed record ExamQuestionDto(
    Guid QuestionId,
    string Text,
    string? Explanation,
    IReadOnlyList<ExamOptionDto> Options);

// IsCorrect included server-side for scoring; never sent to client during exam
public sealed record ExamOptionDto(Guid OptionId, string Text, bool IsCorrect);
