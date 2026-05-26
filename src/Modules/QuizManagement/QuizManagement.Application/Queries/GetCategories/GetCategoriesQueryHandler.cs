using Dapper;
using MediatR;
using Quizora.SharedKernel;
using QuizManagement.Application.DTOs;

namespace QuizManagement.Application.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler(IDbConnectionFactory db)
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<CategoryDto>(
            "SELECT Id, Name, Slug, ParentId, OrderIndex FROM quiz.Categories ORDER BY OrderIndex");
        return rows.ToList().AsReadOnly();
    }
}
