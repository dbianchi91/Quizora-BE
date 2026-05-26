using Dapper;
using Quizora.SharedKernel;

namespace QuizManagement.Infrastructure.Queries;

internal sealed class QuizReader(IDbConnectionFactory db) : IQuizReader
{
    public async Task<QuizForExamDto?> GetQuizForExamAsync(Guid quizId, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var sql = """
            SELECT q.Id AS QuizId, q.Title, q.CategoryId, COUNT(qq.QuestionId) AS TotalQuestions,
                   q.TimeLimitSeconds, q.PointsCorrect, q.PointsWrong, q.PointsSkipped,
                   q.PassingScore, q.ShuffleQuestions, q.ShuffleOptions
            FROM quiz.Quizzes q
            LEFT JOIN quiz.QuizQuestions qq ON qq.QuizId = q.Id
            WHERE q.Id = @QuizId AND q.Status = 'Published' AND q.IsDeleted = 0
            GROUP BY q.Id, q.Title, q.CategoryId, q.TimeLimitSeconds, q.PointsCorrect, q.PointsWrong,
                     q.PointsSkipped, q.PassingScore, q.ShuffleQuestions, q.ShuffleOptions;

            SELECT qst.Id AS QuestionId, qst.Text, qst.Explanation
            FROM quiz.Questions qst
            JOIN quiz.QuizQuestions qq ON qq.QuestionId = qst.Id
            WHERE qq.QuizId = @QuizId ORDER BY qq.OrderIndex;

            SELECT qo.Id AS OptionId, qo.QuestionId, qo.Text, qo.IsCorrect
            FROM quiz.QuestionOptions qo
            JOIN quiz.QuizQuestions qq ON qq.QuestionId = qo.QuestionId
            WHERE qq.QuizId = @QuizId ORDER BY qo.OrderIndex;
            """;

        using var multi = await conn.QueryMultipleAsync(sql, new { QuizId = quizId });
        var header = await multi.ReadFirstOrDefaultAsync<dynamic>();
        if (header is null) return null;

        var questions = (await multi.ReadAsync<dynamic>()).ToList();
        var options = (await multi.ReadAsync<dynamic>()).ToLookup(o => (Guid)o.QuestionId);

        var questionDtos = questions.Select(q => new ExamQuestionDto(
            (Guid)q.QuestionId, (string)q.Text, (string?)q.Explanation,
            options[(Guid)q.QuestionId].Select(o =>
                new ExamOptionDto((Guid)o.OptionId, (string)o.Text, (bool)o.IsCorrect))
                .ToList().AsReadOnly())).ToList().AsReadOnly();

        return new QuizForExamDto(
            (Guid)header.QuizId, (string)header.Title, (Guid)header.CategoryId,
            (int)header.TotalQuestions, (int)header.TimeLimitSeconds,
            (double)header.PointsCorrect, (double)header.PointsWrong,
            (double)header.PointsSkipped, (double?)header.PassingScore,
            (bool)header.ShuffleQuestions, (bool)header.ShuffleOptions, questionDtos);
    }
}
