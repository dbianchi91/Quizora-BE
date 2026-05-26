using Identity.Domain.ValueObjects;
using Quizora.SharedKernel;
using QuizManagement.Domain.Enums;
using QuizManagement.Domain.Events;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Domain.Entities;

public sealed class Quiz : BaseEntity<QuizId>
{
    public string Title { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public QuizStatus Status { get; private set; }
    public CategoryId CategoryId { get; private set; } = default!;
    public UserId CreatorId { get; private set; } = default!;
    public ExamConfig ExamConfig { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<QuizQuestion> _questions = [];
    public IReadOnlyList<QuizQuestion> Questions => _questions.AsReadOnly();

    private readonly List<Tag> _tags = [];
    public IReadOnlyList<Tag> Tags => _tags.AsReadOnly();

    private Quiz() { }

    public static Result<Quiz> Create(string title, string slug, CategoryId categoryId,
        UserId creatorId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Quiz>(Error.Validation("Title", "Title cannot be empty."));

        return Result.Success(new Quiz
        {
            Id = QuizId.New(),
            Title = title.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Description = description?.Trim(),
            Status = QuizStatus.Draft,
            CategoryId = categoryId,
            CreatorId = creatorId,
            ExamConfig = ExamConfig.CreateDefault(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    public Result Publish()
    {
        if (_questions.Count == 0)
            return Result.Failure(Error.Validation("Questions", "Cannot publish a quiz with no questions."));
        if (Status == QuizStatus.Published)
            return Result.Failure(Error.Conflict("Quiz"));

        Status = QuizStatus.Published;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new QuizPublishedEvent(Guid.NewGuid(), DateTime.UtcNow, Id));
        return Result.Success();
    }

    public void AddQuestion(QuestionId questionId)
    {
        if (_questions.Any(q => q.QuestionId == questionId)) return;
        _questions.Add(QuizQuestion.Create(Id, questionId, _questions.Count));
        if (Status == QuizStatus.Published) Status = QuizStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string slug, string? description, CategoryId categoryId, ExamConfig config)
    {
        Title = title.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Description = description?.Trim();
        CategoryId = categoryId;
        ExamConfig = config;
        if (Status == QuizStatus.Published) Status = QuizStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete() { }
}
