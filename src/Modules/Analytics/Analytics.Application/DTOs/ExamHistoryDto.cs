namespace Analytics.Application.DTOs;

public sealed record ExamHistoryDto(
    Guid SessionId,
    string? QuizTitle,
    double Score,
    double NormalizedScore,
    int CorrectCount,
    int WrongCount,
    int SkippedCount,
    string SessionType,
    DateTime CompletedAt);
