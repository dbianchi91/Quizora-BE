namespace ExamEngine.Application.DTOs;

public sealed record ExamStateDto(
    Guid SessionId, Guid QuizId, string Type, string Status,
    int RemainingSeconds, int TotalQuestions, int AnsweredCount,
    IReadOnlyList<ExamQuestionStateDto> Questions);

public sealed record ExamQuestionStateDto(
    Guid QuestionId, string Text,
    IReadOnlyList<ExamOptionStateDto> Options,
    Guid? AnsweredOptionId);

public sealed record ExamOptionStateDto(Guid OptionId, string Text);

public sealed record AnswerFeedbackDto(
    bool IsCorrect, double PointsAwarded,
    string? CorrectExplanation, Guid? CorrectOptionId);

public sealed record ExamResultDto(
    Guid SessionId, Guid QuizId, string QuizTitle, string Type, string Status,
    double Score, double NormalizedScore, double? PassingScore, bool? Passed,
    int TotalQuestions, int CorrectCount, int WrongCount, int SkippedCount,
    DateTime StartedAt, DateTime? CompletedAt,
    IReadOnlyList<QuestionResultDto> Questions,
    IReadOnlyList<LeaderboardEntryDto> Leaderboard);

public sealed record QuestionResultDto(
    Guid QuestionId, string Text, string? Explanation,
    Guid? SelectedOptionId, Guid CorrectOptionId,
    bool IsCorrect, double PointsAwarded);

public sealed record LeaderboardEntryDto(int Rank, Guid UserId, string UserName,
    double NormalizedScore, DateTime CompletedAt);

public sealed record ExamHistoryDto(
    Guid SessionId, Guid QuizId, string QuizTitle, string Type, string Status,
    double? NormalizedScore, DateTime StartedAt, DateTime? CompletedAt);
