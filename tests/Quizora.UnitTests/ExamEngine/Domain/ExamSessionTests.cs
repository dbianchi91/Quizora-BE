using ExamEngine.Domain.Entities;
using ExamEngine.Domain.Enums;
using ExamEngine.Domain.Events;
using FluentAssertions;
using Identity.Domain.ValueObjects;

namespace Quizora.UnitTests.ExamEngine.Domain;

public class ExamSessionTests
{
    private static ConfigSnapshot AnyConfig() =>
        ConfigSnapshot.From(1.0, -0.2, 0.0, 1800, null, false, false);

    private static ExamSession CreateSession(SessionType type = SessionType.Official) =>
        ExamSession.Start(Guid.NewGuid(), UserId.New(), type, AnyConfig(), 10);

    [Fact]
    public void Start_Official_SetsExpiresAt()
    {
        var session = CreateSession(SessionType.Official);
        session.ExpiresAt.Should().NotBeNull();
        session.Status.Should().Be(SessionStatus.InProgress);
    }

    [Fact]
    public void Start_Study_HasNoExpiry()
    {
        var session = CreateSession(SessionType.Study);
        session.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void RecordAnswer_Correct_AddsPositivePoints()
    {
        var session = CreateSession();
        var result = session.RecordAnswer(Guid.NewGuid(), Guid.NewGuid(), isCorrect: true, 30);
        result.IsSuccess.Should().BeTrue();
        result.Value.PointsAwarded.Should().Be(1.0);
    }

    [Fact]
    public void RecordAnswer_Wrong_AddsNegativePoints()
    {
        var session = CreateSession();
        var result = session.RecordAnswer(Guid.NewGuid(), Guid.NewGuid(), isCorrect: false, 30);
        result.Value.PointsAwarded.Should().Be(-0.2);
    }

    [Fact]
    public void RecordAnswer_Skipped_AwardsZeroPoints()
    {
        var session = CreateSession();
        var result = session.RecordAnswer(Guid.NewGuid(), null, isCorrect: false, 0);
        result.Value.PointsAwarded.Should().Be(0.0);
    }

    [Fact]
    public void RecordAnswer_DuplicateQuestion_ReturnsConflict()
    {
        var session = CreateSession();
        var qId = Guid.NewGuid();
        session.RecordAnswer(qId, Guid.NewGuid(), true, 10);
        var result = session.RecordAnswer(qId, Guid.NewGuid(), true, 10);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Conflict");
    }

    [Fact]
    public void Complete_RaisesExamCompletedEvent()
    {
        var session = CreateSession();
        session.Complete();
        session.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ExamCompletedEvent>();
        session.Status.Should().Be(SessionStatus.Completed);
    }

    [Fact]
    public void AutoSubmit_SetsTimedOut_AndRaisesEvent()
    {
        var session = CreateSession();
        session.AutoSubmit();
        session.Status.Should().Be(SessionStatus.TimedOut);
        session.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ExamTimedOutEvent>();
    }

    [Fact]
    public void Complete_ScoreNeverNegative()
    {
        var session = CreateSession();
        for (int i = 0; i < 10; i++)
            session.RecordAnswer(Guid.NewGuid(), Guid.NewGuid(), false, 10);
        session.Complete();
        session.Score.Should().Be(0);
    }
}
