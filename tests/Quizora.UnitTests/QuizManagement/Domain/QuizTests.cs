using FluentAssertions;
using Identity.Domain.ValueObjects;
using QuizManagement.Domain.Entities;
using QuizManagement.Domain.Enums;
using QuizManagement.Domain.Events;
using QuizManagement.Domain.ValueObjects;

namespace Quizora.UnitTests.QuizManagement.Domain;

public class QuizTests
{
    private static CategoryId AnyCategory() => CategoryId.New();
    private static UserId AnyCreator() => UserId.New();

    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var result = Quiz.Create("Test Quiz", "test-quiz", AnyCategory(), AnyCreator());
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public void Create_WithEmptyTitle_Fails()
    {
        var result = Quiz.Create("", "slug", AnyCategory(), AnyCreator());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Publish_WithNoQuestions_Fails()
    {
        var quiz = Quiz.Create("Q", "q", AnyCategory(), AnyCreator()).Value;
        var result = quiz.Publish();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Publish_WithQuestions_Succeeds_AndRaisesEvent()
    {
        var quiz = Quiz.Create("Q", "q", AnyCategory(), AnyCreator()).Value;
        quiz.AddQuestion(QuestionId.New());
        var result = quiz.Publish();
        result.IsSuccess.Should().BeTrue();
        quiz.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<QuizPublishedEvent>();
    }

    [Fact]
    public void AddQuestion_ChangesPublishedQuizToDraft()
    {
        var quiz = Quiz.Create("Q", "q", AnyCategory(), AnyCreator()).Value;
        quiz.AddQuestion(QuestionId.New());
        quiz.ClearDomainEvents();
        quiz.Publish();
        quiz.AddQuestion(QuestionId.New());
        quiz.Status.Should().Be(QuizStatus.Draft);
    }
}
