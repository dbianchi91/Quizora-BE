using ExamEngine.Application.Commands.StartExam;
using ExamEngine.Application.Interfaces;
using ExamEngine.Domain.Entities;
using FluentAssertions;
using IdentityDomain = global::Identity.Domain.ValueObjects;
using NSubstitute;
using Quizora.SharedKernel;

namespace Quizora.UnitTests.ExamEngine.Application;

public class StartExamCommandHandlerTests
{
    private readonly IExamRepository _examRepo = Substitute.For<IExamRepository>();
    private readonly IQuizReader _quizReader = Substitute.For<IQuizReader>();
    private readonly IExamTimerService _timer = Substitute.For<IExamTimerService>();
    private readonly StartExamCommandHandler _sut;

    public StartExamCommandHandlerTests() =>
        _sut = new StartExamCommandHandler(_examRepo, _quizReader, _timer);

    private static QuizForExamDto AnyQuiz() => new(
        Guid.NewGuid(), "Quiz", Guid.NewGuid(), 10, 1800, 1.0, -0.2, 0.0, null, false, false, []);

    [Fact]
    public async Task Handle_QuizNotFound_ReturnsNotFound()
    {
        _quizReader.GetQuizForExamAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((QuizForExamDto?)null);

        var result = await _sut.Handle(
            new StartExamCommand(Guid.NewGuid(), Guid.NewGuid(), "Official", "key-1"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_ValidOfficial_CreatesSessionAndSchedulesTimer()
    {
        _quizReader.GetQuizForExamAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(AnyQuiz());
        _examRepo.GetActiveOfficialSessionAsync(Arg.Any<Guid>(), Arg.Any<IdentityDomain.UserId>(),
            Arg.Any<CancellationToken>()).Returns((ExamSession?)null);

        var result = await _sut.Handle(
            new StartExamCommand(Guid.NewGuid(), Guid.NewGuid(), "Official", "key-1"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _timer.Received(1).ScheduleAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Study_DoesNotScheduleTimer()
    {
        _quizReader.GetQuizForExamAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(AnyQuiz());

        var result = await _sut.Handle(
            new StartExamCommand(Guid.NewGuid(), Guid.NewGuid(), "Study", "key-1"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _timer.DidNotReceive().ScheduleAsync(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }
}
