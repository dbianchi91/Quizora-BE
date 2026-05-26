using AITutor.Domain.ValueObjects;
using Identity.Domain.ValueObjects;
using Quizora.SharedKernel;

namespace AITutor.Domain.Entities;

public sealed class StudyPlan : BaseEntity<StudyPlanId>
{
    public UserId UserId { get; private set; } = default!;
    public string ContentJson { get; private set; } = default!;
    public DateTime GeneratedAt { get; private set; }
    public bool UpdatedAutomatically { get; private set; }

    private StudyPlan() { }

    public static StudyPlan Create(UserId userId, string contentJson, bool auto) => new()
    {
        Id = StudyPlanId.New(),
        UserId = userId,
        ContentJson = contentJson,
        GeneratedAt = DateTime.UtcNow,
        UpdatedAutomatically = auto
    };

    public void Update(string contentJson, bool auto)
    {
        ContentJson = contentJson;
        GeneratedAt = DateTime.UtcNow;
        UpdatedAutomatically = auto;
    }
}
