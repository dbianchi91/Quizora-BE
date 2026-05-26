namespace Analytics.Infrastructure.Persistence.Entities;

public sealed class UserStats
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int TotalExams { get; set; }
    public int TotalCorrect { get; set; }
    public int TotalAnswered { get; set; }
    public double AverageScore { get; set; }
    public double BestScore { get; set; }
    public int TotalTimeSpentSeconds { get; set; }
    public DateTime UpdatedAt { get; set; }
}
