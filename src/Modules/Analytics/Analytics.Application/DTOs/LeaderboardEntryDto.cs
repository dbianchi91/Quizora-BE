namespace Analytics.Application.DTOs;

public sealed record LeaderboardEntryDto(
    long Rank,
    Guid UserId,
    string UserName,
    double BestScore,
    int TotalExams);
