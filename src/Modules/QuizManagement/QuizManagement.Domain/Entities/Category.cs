using Quizora.SharedKernel;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Domain.Entities;

public sealed class Category : BaseEntity<CategoryId>
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public CategoryId? ParentId { get; private set; }
    public int OrderIndex { get; private set; }

    private Category() { }

    public static Category Create(string name, string slug, CategoryId? parentId = null, int orderIndex = 0)
    {
        return new Category
        {
            Id = CategoryId.New(),
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            ParentId = parentId,
            OrderIndex = orderIndex
        };
    }
}
