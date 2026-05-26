using Quizora.SharedKernel;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Domain.Entities;

public sealed class QuestionOption : BaseEntity<QuestionOptionId>
{
    public string Text { get; private set; } = default!;
    public bool IsCorrect { get; private set; }
    public int OrderIndex { get; private set; }

    private QuestionOption() { }

    public static QuestionOption Create(string text, bool isCorrect, int orderIndex) =>
        new() { Id = QuestionOptionId.New(), Text = text.Trim(), IsCorrect = isCorrect, OrderIndex = orderIndex };
}
