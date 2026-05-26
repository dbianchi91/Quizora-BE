namespace Analytics.Infrastructure.Persistence.Entities;

public sealed class CategoryStats
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public int TotalExams { get; set; }
    public double AverageScore { get; set; }
    public double WeakAreaScore { get; set; }
    public DateTime UpdatedAt { get; set; }
}
