using FluentAssertions;
using Identity.Domain.ValueObjects;
using NSubstitute;
using Quizora.SharedKernel;
using QuizManagement.Application.Commands.PublishQuiz;
using QuizManagement.Application.Interfaces;
using QuizManagement.Domain.Entities;
using QuizManagement.Domain.ValueObjects;

namespace Quizora.UnitTests.QuizManagement.Application;

public class PublishQuizCommandHandlerTests
{
    private readonly IQuizManagementRepository _repo = Substitute.For<IQuizManagementRepository>();
    private readonly PublishQuizCommandHandler _sut;

    public PublishQuizCommandHandlerTests() => _sut = new PublishQuizCommandHandler(_repo);

    [Fact]
    public async Task Handle_QuizNotFound_ReturnsNotFound()
    {
        _repo.GetByIdAsync(Arg.Any<QuizId>(), Arg.Any<CancellationToken>()).Returns((Quiz?)null);
        var result = await _sut.Handle(new PublishQuizCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_WrongOwner_ReturnsUnauthorized()
    {
        var creatorId = UserId.New();
        var quiz = Quiz.Create("Q", "q", CategoryId.New(), creatorId).Value;
        _repo.GetByIdAsync(Arg.Any<QuizId>(), Arg.Any<CancellationToken>()).Returns(quiz);

        var result = await _sut.Handle(
            new PublishQuizCommand(quiz.Id.Value, Guid.NewGuid()), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task Handle_ValidOwnerWithQuestions_Publishes()
    {
        var creatorId = UserId.New();
        var quiz = Quiz.Create("Q", "q", CategoryId.New(), creatorId).Value;
        quiz.AddQuestion(QuestionId.New());
        _repo.GetByIdAsync(Arg.Any<QuizId>(), Arg.Any<CancellationToken>()).Returns(quiz);

        var result = await _sut.Handle(
            new PublishQuizCommand(quiz.Id.Value, creatorId.Value), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }
}
