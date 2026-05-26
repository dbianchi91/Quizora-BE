namespace ExamEngine.Domain.Services;

public static class ScoringService
{
    public static double Calculate(int correct, int wrong, int skipped,
        double pointsCorrect, double pointsWrong, double pointsSkipped)
    {
        var raw = (correct * pointsCorrect) + (wrong * pointsWrong) + (skipped * pointsSkipped);
        return Math.Max(0, raw);
    }

    public static double Normalize(double score, int totalQuestions, double pointsCorrect)
    {
        var max = totalQuestions * pointsCorrect;
        return max > 0 ? Math.Round(score / max * 100, 2) : 0;
    }
}
