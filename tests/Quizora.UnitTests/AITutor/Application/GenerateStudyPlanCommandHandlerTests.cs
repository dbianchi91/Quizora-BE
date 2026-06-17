using AITutor.Application.Commands.GenerateStudyPlan;
using AITutor.Application.Interfaces;
using AITutor.Application.Services;
using AITutor.Domain.Entities;
using FluentAssertions;
using IdentityDomain = global::Identity.Domain.ValueObjects;
using NSubstitute;

namespace Quizora.UnitTests.AITutor.Application;

public class GenerateStudyPlanCommandHandlerTests
{
    private readonly IAIProvider _ai = Substitute.For<IAIProvider>();
    private readonly IAITutorRepository _repo = Substitute.For<IAITutorRepository>();
    private readonly IUserContextBuilder _contextBuilder = Substitute.For<IUserContextBuilder>();
    private readonly GenerateStudyPlanCommandHandler _sut;

    public GenerateStudyPlanCommandHandlerTests()
    {
        _sut = new GenerateStudyPlanCommandHandler(_ai, _repo, _contextBuilder);
    }

    [Fact]
    public async Task Handle_NoExistingPlan_CreatesNewPlan()
    {
        var userId = Guid.NewGuid();
        var command = new GenerateStudyPlanCommand(userId, Automatic: false);
        var planJson = """{"priorityAreas":["Math"],"suggestedQuizIds":[],"weeklyGoals":[],"tips":[]}""";

        _contextBuilder.BuildSystemPromptAsync(userId, null, Arg.Any<CancellationToken>())
            .Returns("system-prompt");

        _ai.StreamChatAsync(Arg.Any<IReadOnlyList<ChatMessageDto>>(), "system-prompt", Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable(planJson));

        _repo.GetStudyPlanAsync(Arg.Any<IdentityDomain.UserId>(), Arg.Any<CancellationToken>())
            .Returns((StudyPlan?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddOrUpdateStudyPlanAsync(Arg.Any<StudyPlan>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingPlan_UpdatesPlan()
    {
        var userId = Guid.NewGuid();
        var command = new GenerateStudyPlanCommand(userId, Automatic: true);
        var existingPlan = StudyPlan.Create(IdentityDomain.UserId.From(userId), "{}", false);
        var newJson = """{"priorityAreas":["Science"]}""";

        _contextBuilder.BuildSystemPromptAsync(userId, null, Arg.Any<CancellationToken>())
            .Returns("system-prompt");

        _ai.StreamChatAsync(Arg.Any<IReadOnlyList<ChatMessageDto>>(), "system-prompt", Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable(newJson));

        _repo.GetStudyPlanAsync(Arg.Any<IdentityDomain.UserId>(), Arg.Any<CancellationToken>())
            .Returns(existingPlan);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existingPlan.ContentJson.Should().Be(newJson);
        existingPlan.UpdatedAutomatically.Should().BeTrue();
        await _repo.DidNotReceive().AddOrUpdateStudyPlanAsync(Arg.Any<StudyPlan>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AiStreamsMultipleChunks_ConcatenatesCorrectly()
    {
        var userId = Guid.NewGuid();
        var command = new GenerateStudyPlanCommand(userId, Automatic: false);

        _contextBuilder.BuildSystemPromptAsync(userId, null, Arg.Any<CancellationToken>())
            .Returns("system-prompt");

        _ai.StreamChatAsync(Arg.Any<IReadOnlyList<ChatMessageDto>>(), "system-prompt", Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable("{\"priority", "Areas\":[\"Math\"]}"));

        _repo.GetStudyPlanAsync(Arg.Any<IdentityDomain.UserId>(), Arg.Any<CancellationToken>())
            .Returns((StudyPlan?)null);

        StudyPlan? captured = null;
        await _repo.AddOrUpdateStudyPlanAsync(Arg.Do<StudyPlan>(p => captured = p), Arg.Any<CancellationToken>());

        await _sut.Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.ContentJson.Should().Be("{\"priorityAreas\":[\"Math\"]}");
    }

    [Fact]
    public async Task Handle_AiReturnsJsonWithMarkdownFence_SavesCleanJson()
    {
        var userId = Guid.NewGuid();
        var command = new GenerateStudyPlanCommand(userId, Automatic: false);

        _contextBuilder.BuildSystemPromptAsync(userId, null, Arg.Any<CancellationToken>())
            .Returns("system-prompt");

        _ai.StreamChatAsync(Arg.Any<IReadOnlyList<ChatMessageDto>>(), "system-prompt", Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable("```json\n{\"priorityAreas\":[\"Math\"]}\n```"));

        _repo.GetStudyPlanAsync(Arg.Any<IdentityDomain.UserId>(), Arg.Any<CancellationToken>())
            .Returns((StudyPlan?)null);

        StudyPlan? captured = null;
        await _repo.AddOrUpdateStudyPlanAsync(Arg.Do<StudyPlan>(p => captured = p), Arg.Any<CancellationToken>());

        await _sut.Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.ContentJson.Should().Be("{\"priorityAreas\":[\"Math\"]}");
    }

    private static async IAsyncEnumerable<string> AsyncEnumerable(params string[] chunks)
    {
        foreach (var chunk in chunks)
        {
            yield return chunk;
            await Task.Yield();
        }
    }
}
