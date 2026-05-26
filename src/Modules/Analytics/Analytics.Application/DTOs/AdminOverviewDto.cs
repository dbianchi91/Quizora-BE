namespace Analytics.Application.DTOs;

public sealed record AdminOverviewDto(
    int TotalActiveUsers,
    int TotalExamsAllTime,
    int ExamsToday,
    double GlobalAverageScore);
