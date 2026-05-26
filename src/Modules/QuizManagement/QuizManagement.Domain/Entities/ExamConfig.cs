namespace QuizManagement.Domain.Entities;

public sealed class ExamConfig
{
    public int TimeLimitSeconds { get; private set; }
    public double PointsCorrect { get; private set; }
    public double PointsWrong { get; private set; }
    public double PointsSkipped { get; private set; }
    public double? PassingScore { get; private set; }
    public bool ShuffleQuestions { get; private set; }
    public bool ShuffleOptions { get; private set; }

    private ExamConfig() { }

    public static ExamConfig CreateDefault() => new()
    {
        TimeLimitSeconds = 1800,
        PointsCorrect = 1.0,
        PointsWrong = -0.2,
        PointsSkipped = 0.0,
        ShuffleQuestions = false,
        ShuffleOptions = false
    };

    public static ExamConfig Create(int timeLimitSeconds, double pointsCorrect,
        double pointsWrong, double pointsSkipped, double? passingScore,
        bool shuffleQuestions, bool shuffleOptions) => new()
    {
        TimeLimitSeconds = timeLimitSeconds,
        PointsCorrect = pointsCorrect,
        PointsWrong = pointsWrong,
        PointsSkipped = pointsSkipped,
        PassingScore = passingScore,
        ShuffleQuestions = shuffleQuestions,
        ShuffleOptions = shuffleOptions
    };
}
