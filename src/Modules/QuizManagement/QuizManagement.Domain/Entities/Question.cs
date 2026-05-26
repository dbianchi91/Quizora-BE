using Identity.Domain.ValueObjects;
using Quizora.SharedKernel;
using QuizManagement.Domain.Enums;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Domain.Entities;

public sealed class Question : BaseEntity<QuestionId>
{
    public string Text { get; private set; } = default!;
    public string? Explanation { get; private set; }
    public DifficultyLevel Difficulty { get; private set; }
    public UserId CreatorId { get; private set; } = default!;

    private readonly List<QuestionOption> _options = [];
    public IReadOnlyList<QuestionOption> Options => _options.AsReadOnly();

    private Question() { }

    public static Result<Question> Create(string text, DifficultyLevel difficulty,
        UserId creatorId, IEnumerable<(string Text, bool IsCorrect)> options, string? explanation = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Result.Failure<Question>(Error.Validation("Text", "Question text cannot be empty."));

        var optionList = options.ToList();
        if (optionList.Count < 2)
            return Result.Failure<Question>(Error.Validation("Options", "At least 2 options required."));
        if (!optionList.Any(o => o.IsCorrect))
            return Result.Failure<Question>(Error.Validation("Options", "At least one correct option required."));

        var question = new Question
        {
            Id = QuestionId.New(),
            Text = text.Trim(),
            Explanation = explanation?.Trim(),
            Difficulty = difficulty,
            CreatorId = creatorId
        };

        for (int i = 0; i < optionList.Count; i++)
            question._options.Add(QuestionOption.Create(optionList[i].Text, optionList[i].IsCorrect, i));

        return Result.Success(question);
    }

    public void UpdateText(string text, string? explanation)
    {
        Text = text.Trim();
        Explanation = explanation?.Trim();
    }
}
