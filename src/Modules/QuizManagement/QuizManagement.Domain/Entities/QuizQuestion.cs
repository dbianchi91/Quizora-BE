using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Domain.Entities;

public sealed class QuizQuestion
{
    public QuizId QuizId { get; private set; } = default!;
    public QuestionId QuestionId { get; private set; } = default!;
    public int OrderIndex { get; private set; }

    private QuizQuestion() { }

    public static QuizQuestion Create(QuizId quizId, QuestionId questionId, int orderIndex) =>
        new() { QuizId = quizId, QuestionId = questionId, OrderIndex = orderIndex };
}
