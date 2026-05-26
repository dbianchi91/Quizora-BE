namespace ExamEngine.Domain.Entities;

public sealed class ConfigSnapshot
{
    public int TimeLimitSeconds { get; private set; }
    public double PointsCorrect { get; private set; }
    public double PointsWrong { get; private set; }
    public double PointsSkipped { get; private set; }
    public double? PassingScore { get; private set; }
    public bool ShuffleQuestions { get; private set; }
    public bool ShuffleOptions { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string? QuizTitle { get; private set; }

    private ConfigSnapshot() { }

    public static ConfigSnapshot From(double pointsCorrect, double pointsWrong,
        double pointsSkipped, int timeLimitSeconds, double? passingScore,
        bool shuffleQuestions, bool shuffleOptions,
        Guid? categoryId = null, string? quizTitle = null) => new()
    {
        PointsCorrect = pointsCorrect,
        PointsWrong = pointsWrong,
        PointsSkipped = pointsSkipped,
        TimeLimitSeconds = timeLimitSeconds,
        PassingScore = passingScore,
        ShuffleQuestions = shuffleQuestions,
        ShuffleOptions = shuffleOptions,
        CategoryId = categoryId,
        QuizTitle = quizTitle
    };
}
