using Dapper;
using MediatR;
using Quizora.SharedKernel;
using QuizManagement.Application.DTOs;

namespace QuizManagement.Application.Queries.GetQuizzes;

public sealed class GetQuizzesQueryHandler(IDbConnectionFactory db)
    : IRequestHandler<GetQuizzesQuery, IReadOnlyList<QuizSummaryDto>>
{
    public async Task<IReadOnlyList<QuizSummaryDto>> Handle(GetQuizzesQuery request, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var sql = """
            SELECT q.Id, q.Title, q.Slug, q.Description, q.Status,
                   q.CategoryId, c.Name AS CategoryName,
                   COUNT(qq.QuestionId) AS QuestionCount,
                   q.TimeLimitSeconds, q.PointsCorrect, q.PointsWrong, q.PointsSkipped,
                   q.PassingScore, q.ShuffleQuestions, q.ShuffleOptions, q.CreatedAt
            FROM quiz.Quizzes q
            JOIN quiz.Categories c ON c.Id = q.CategoryId
            LEFT JOIN quiz.QuizQuestions qq ON qq.QuizId = q.Id
            WHERE q.Status = 'Published'
              AND q.IsDeleted = 0
              AND (@CategoryId IS NULL OR q.CategoryId = @CategoryId)
              AND (@Search IS NULL OR q.Title LIKE '%' + @Search + '%')
            GROUP BY q.Id, q.Title, q.Slug, q.Description, q.Status, q.CategoryId, c.Name,
                     q.TimeLimitSeconds, q.PointsCorrect, q.PointsWrong, q.PointsSkipped,
                     q.PassingScore, q.ShuffleQuestions, q.ShuffleOptions, q.CreatedAt
            ORDER BY q.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var rows = await conn.QueryAsync<dynamic>(sql, new
        {
            CategoryId = request.CategoryId,
            Search = request.Search,
            Offset = (request.Page - 1) * request.PageSize,
            PageSize = request.PageSize
        });

        return rows.Select(r => new QuizSummaryDto(
            (Guid)r.Id, (string)r.Title, (string)r.Slug, (string?)r.Description,
            (string)r.Status, (Guid)r.CategoryId, (string)r.CategoryName, (int)r.QuestionCount,
            new ExamConfigDto((int)r.TimeLimitSeconds, (double)r.PointsCorrect, (double)r.PointsWrong,
                (double)r.PointsSkipped, (double?)r.PassingScore, (bool)r.ShuffleQuestions, (bool)r.ShuffleOptions),
            (DateTime)r.CreatedAt)).ToList().AsReadOnly();
    }
}
