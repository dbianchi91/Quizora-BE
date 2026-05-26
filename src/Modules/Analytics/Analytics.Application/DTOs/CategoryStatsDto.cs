namespace Analytics.Application.DTOs;

public sealed record CategoryStatsDto(
    Guid CategoryId,
    string CategoryName,
    int TotalExams,
    double AverageScore,
    double WeakAreaScore);
