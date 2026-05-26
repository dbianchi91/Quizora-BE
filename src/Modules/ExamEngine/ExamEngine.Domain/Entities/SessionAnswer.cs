using ExamEngine.Domain.ValueObjects;
using Quizora.SharedKernel;

namespace ExamEngine.Domain.Entities;

public sealed class SessionAnswer : BaseEntity<SessionAnswerId>
{
    public ExamSessionId SessionId { get; private set; } = default!;
    public Guid QuestionId { get; private set; }
    public Guid? SelectedOptionId { get; private set; }
    public bool IsCorrect { get; private set; }
    public double PointsAwarded { get; private set; }
    public DateTime AnsweredAt { get; private set; }
    public int TimeSpentSeconds { get; private set; }

    private SessionAnswer() { }

    public static SessionAnswer Create(ExamSessionId sessionId, Guid questionId,
        Guid? selectedOptionId, bool isCorrect, double pointsAwarded, int timeSpentSeconds)
        => new()
        {
            Id = SessionAnswerId.New(),
            SessionId = sessionId,
            QuestionId = questionId,
            SelectedOptionId = selectedOptionId,
            IsCorrect = isCorrect,
            PointsAwarded = pointsAwarded,
            AnsweredAt = DateTime.UtcNow,
            TimeSpentSeconds = timeSpentSeconds
        };
}
