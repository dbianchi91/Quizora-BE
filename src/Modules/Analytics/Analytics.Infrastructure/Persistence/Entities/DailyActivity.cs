namespace Analytics.Infrastructure.Persistence.Entities;

public sealed class DailyActivity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public int ExamsCount { get; set; }
    public int CorrectAnswers { get; set; }
    public int TimeSpentSeconds { get; set; }
}
