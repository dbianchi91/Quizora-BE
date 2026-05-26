using FluentAssertions;
using NSubstitute;
using Quizora.SharedKernel;
using QuizManagement.Application.Commands.CreateQuiz;
using QuizManagement.Application.Interfaces;
using QuizManagement.Domain.Entities;

namespace Quizora.UnitTests.QuizManagement.Application;

public class CreateQuizCommandHandlerTests
{
    private readonly IQuizManagementRepository _repo = Substitute.For<IQuizManagementRepository>();
    private readonly CreateQuizCommandHandler _sut;

    public CreateQuizCommandHandlerTests()
    {
        _sut = new CreateQuizCommandHandler(_repo);
    }

    private static CreateQuizCommand ValidCommand() => new(
        Guid.NewGuid(), "Test Quiz", "test-quiz", Guid.NewGuid(),
        null, 1800, 1.0, -0.2, 0.0, null, false, false);

    [Fact]
    public async Task Handle_WithUniqueSlug_ReturnsSuccess()
    {
        _repo.SlugExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Test Quiz");
        await _repo.Received(1).AddQuizAsync(Arg.Any<Quiz>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateSlug_ReturnsConflict()
    {
        _repo.SlugExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Conflict");
    }
}
