using ExamEngine.Domain.Enums;
using ExamEngine.Domain.Events;
using ExamEngine.Domain.ValueObjects;
using Identity.Domain.ValueObjects;
using Quizora.SharedKernel;

namespace ExamEngine.Domain.Entities;

public sealed class ExamSession : BaseEntity<ExamSessionId>
{
    public Guid QuizId { get; private set; }
    public UserId UserId { get; private set; } = default!;
    public SessionType Type { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public double? Score { get; private set; }
    public double? NormalizedScore { get; private set; }
    public int TotalQuestions { get; private set; }
    public int CorrectCount { get; private set; }
    public int WrongCount { get; private set; }
    public int SkippedCount { get; private set; }
    public ConfigSnapshot Config { get; private set; } = default!;

    private readonly List<SessionAnswer> _answers = [];
    public IReadOnlyList<SessionAnswer> Answers => _answers.AsReadOnly();

    private ExamSession() { }

    public static ExamSession Start(Guid quizId, UserId userId, SessionType type,
        ConfigSnapshot config, int totalQuestions)
    {
        var now = DateTime.UtcNow;
        return new ExamSession
        {
            Id = ExamSessionId.New(),
            QuizId = quizId,
            UserId = userId,
            Type = type,
            Status = SessionStatus.InProgress,
            StartedAt = now,
            Config = config,
            TotalQuestions = totalQuestions,
            ExpiresAt = type != SessionType.Study
                ? now.AddSeconds(config.TimeLimitSeconds)
                : null
        };
    }

    public Result<SessionAnswer> RecordAnswer(Guid questionId, Guid? selectedOptionId,
        bool isCorrect, int timeSpentSeconds)
    {
        if (Status != SessionStatus.InProgress)
            return Result.Failure<SessionAnswer>(Error.Validation("Status", "Session is not in progress."));

        if (_answers.Any(a => a.QuestionId == questionId))
            return Result.Failure<SessionAnswer>(Error.Conflict("Answer"));

        double points = selectedOptionId is null
            ? Config.PointsSkipped
            : isCorrect ? Config.PointsCorrect : Config.PointsWrong;

        var answer = SessionAnswer.Create(Id, questionId, selectedOptionId, isCorrect, points, timeSpentSeconds);
        _answers.Add(answer);
        return Result.Success(answer);
    }

    public Result Complete()
    {
        if (Status != SessionStatus.InProgress)
            return Result.Failure(Error.Validation("Status", "Session is not in progress."));

        FinalizeScoring(SessionStatus.Completed);
        var totalTimeSpent = _answers.Sum(a => a.TimeSpentSeconds);
        RaiseDomainEvent(new ExamCompletedEvent(
            Guid.NewGuid(), DateTime.UtcNow,
            Id, UserId,
            QuizId,
            Config.CategoryId,
            Config.QuizTitle,
            Score!.Value,
            NormalizedScore!.Value,
            CorrectCount,
            WrongCount,
            SkippedCount,
            totalTimeSpent,
            Type.ToString()));
        return Result.Success();
    }

    public void AutoSubmit()
    {
        if (Status != SessionStatus.InProgress) return;
        FinalizeScoring(SessionStatus.TimedOut);
        RaiseDomainEvent(new ExamTimedOutEvent(Guid.NewGuid(), DateTime.UtcNow, Id, UserId));
    }

    public void Abandon()
    {
        if (Status != SessionStatus.InProgress) return;
        Status = SessionStatus.Abandoned;
        CompletedAt = DateTime.UtcNow;
    }

    private void FinalizeScoring(SessionStatus finalStatus)
    {
        var answeredIds = _answers.Select(a => a.QuestionId).ToHashSet();
        var maxScore = TotalQuestions * Config.PointsCorrect;

        double raw = _answers.Sum(a => a.PointsAwarded);
        Score = Math.Max(0, raw);
        NormalizedScore = maxScore > 0 ? Math.Round(Score.Value / maxScore * 100, 2) : 0;

        CorrectCount = _answers.Count(a => a.IsCorrect);
        WrongCount = _answers.Count(a => !a.IsCorrect && a.SelectedOptionId.HasValue);
        SkippedCount = TotalQuestions - answeredIds.Count;

        Status = finalStatus;
        CompletedAt = DateTime.UtcNow;
    }

    public int GetRemainingSeconds() =>
        ExpiresAt.HasValue
            ? Math.Max(0, (int)(ExpiresAt.Value - DateTime.UtcNow).TotalSeconds)
            : int.MaxValue;
}
