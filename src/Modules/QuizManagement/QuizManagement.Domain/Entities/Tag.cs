using Quizora.SharedKernel;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Domain.Entities;

public sealed class Tag : BaseEntity<TagId>
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;

    private Tag() { }

    public static Tag Create(string name, string slug) =>
        new() { Id = TagId.New(), Name = name.Trim(), Slug = slug.Trim().ToLowerInvariant() };
}
