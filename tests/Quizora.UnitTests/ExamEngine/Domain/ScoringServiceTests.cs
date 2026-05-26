using ExamEngine.Domain.Services;
using FluentAssertions;

namespace Quizora.UnitTests.ExamEngine.Domain;

public class ScoringServiceTests
{
    [Theory]
    [InlineData(10, 0, 0, 1.0, -0.2, 0.0, 10.0)]
    [InlineData(8, 2, 0, 1.0, -0.2, 0.0, 7.6)]
    [InlineData(0, 10, 0, 1.0, -0.2, 0.0, 0.0)]
    [InlineData(5, 3, 2, 1.0, -0.2, 0.0, 4.4)]
    public void Calculate_ReturnsExpectedScore(int correct, int wrong, int skipped,
        double pc, double pw, double ps, double expected)
    {
        var score = ScoringService.Calculate(correct, wrong, skipped, pc, pw, ps);
        score.Should().BeApproximately(expected, 0.001);
    }

    [Fact]
    public void Calculate_NegativeResult_ClampsToZero()
    {
        var score = ScoringService.Calculate(0, 10, 0, 1.0, -0.5, 0.0);
        score.Should().Be(0);
    }

    [Fact]
    public void Normalize_FullScore_Returns100()
    {
        var normalized = ScoringService.Normalize(10.0, 10, 1.0);
        normalized.Should().Be(100.0);
    }
}
