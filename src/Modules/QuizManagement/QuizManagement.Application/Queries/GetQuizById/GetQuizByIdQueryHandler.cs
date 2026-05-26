using Dapper;
using MediatR;
using Quizora.SharedKernel;
using QuizManagement.Application.DTOs;

namespace QuizManagement.Application.Queries.GetQuizById;

public sealed class GetQuizByIdQueryHandler(IDbConnectionFactory db)
    : IRequestHandler<GetQuizByIdQuery, QuizDetailDto?>
{
    public async Task<QuizDetailDto?> Handle(GetQuizByIdQuery request, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var sql = """
            SELECT q.Id, q.Title, q.Slug, q.Description, q.Status, q.CategoryId, c.Name AS CategoryName,
                   q.TimeLimitSeconds, q.PointsCorrect, q.PointsWrong, q.PointsSkipped,
                   q.PassingScore, q.ShuffleQuestions, q.ShuffleOptions, q.CreatedAt
            FROM quiz.Quizzes q
            JOIN quiz.Categories c ON c.Id = q.CategoryId
            WHERE q.Id = @QuizId AND q.IsDeleted = 0;

            SELECT qst.Id, qst.Text, qst.Explanation, qst.Difficulty, qq.OrderIndex
            FROM quiz.Questions qst
            JOIN quiz.QuizQuestions qq ON qq.QuestionId = qst.Id
            WHERE qq.QuizId = @QuizId ORDER BY qq.OrderIndex;

            SELECT qo.Id, qo.QuestionId, qo.Text, qo.OrderIndex
            FROM quiz.QuestionOptions qo
            JOIN quiz.QuizQuestions qq ON qq.QuestionId = qo.QuestionId
            WHERE qq.QuizId = @QuizId;
            """;

        using var multi = await conn.QueryMultipleAsync(sql, new { QuizId = request.QuizId });
        var header = await multi.ReadFirstOrDefaultAsync<dynamic>();
        if (header is null) return null;

        var questions = (await multi.ReadAsync<dynamic>()).ToList();
        var options = (await multi.ReadAsync<dynamic>()).ToLookup(o => (Guid)o.QuestionId);

        var questionDtos = questions.Select(q => new QuestionInQuizDto(
            (Guid)q.Id, (string)q.Text, (string?)q.Explanation, (string)q.Difficulty,
            options[(Guid)q.Id].Select(o => new OptionDto((Guid)o.Id, (string)o.Text, (int)o.OrderIndex))
                .OrderBy(o => o.OrderIndex).ToList().AsReadOnly(),
            (int)q.OrderIndex)).ToList().AsReadOnly();

        return new QuizDetailDto(
            (Guid)header.Id, (string)header.Title, (string)header.Slug, (string?)header.Description,
            (string)header.Status, (Guid)header.CategoryId, (string)header.CategoryName,
            new ExamConfigDto((int)header.TimeLimitSeconds, (double)header.PointsCorrect,
                (double)header.PointsWrong, (double)header.PointsSkipped, (double?)header.PassingScore,
                (bool)header.ShuffleQuestions, (bool)header.ShuffleOptions),
            questionDtos, [], (DateTime)header.CreatedAt);
    }
}
