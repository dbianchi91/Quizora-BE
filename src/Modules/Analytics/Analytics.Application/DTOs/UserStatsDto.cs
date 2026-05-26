namespace Analytics.Application.DTOs;

public sealed record UserStatsDto(
    int TotalExams,
    int TotalCorrect,
    int TotalAnswered,
    double AverageScore,
    double BestScore,
    int TotalTimeSpentSeconds,
    DateTime UpdatedAt);
