using Dapper;
using ExamEngine.Application.DTOs;
using MediatR;
using Quizora.SharedKernel;

namespace ExamEngine.Application.Queries.GetExamResults;

public record GetExamResultsQuery(Guid SessionId, Guid UserId) : IRequest<Result<ExamResultDto>>;

public sealed class GetExamResultsQueryHandler(IDbConnectionFactory db)
    : IRequestHandler<GetExamResultsQuery, Result<ExamResultDto>>
{
    public async Task<Result<ExamResultDto>> Handle(GetExamResultsQuery request, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var sql = """
            SELECT s.Id, s.QuizId, q.Title AS QuizTitle, s.Type, s.Status,
                   s.Score, s.NormalizedScore, s.PassingScore, s.TotalQuestions,
                   s.CorrectCount, s.WrongCount, s.SkippedCount, s.StartedAt, s.CompletedAt, s.UserId
            FROM exam.ExamSessions s
            JOIN quiz.Quizzes q ON q.Id = s.QuizId
            WHERE s.Id = @SessionId;

            SELECT sa.QuestionId, qst.Text, qst.Explanation,
                   sa.SelectedOptionId, sa.IsCorrect, sa.PointsAwarded,
                   (SELECT TOP 1 Id FROM quiz.QuestionOptions
                    WHERE QuestionId = sa.QuestionId AND IsCorrect = 1) AS CorrectOptionId
            FROM exam.SessionAnswers sa
            JOIN quiz.Questions qst ON qst.Id = sa.QuestionId
            WHERE sa.SessionId = @SessionId;

            SELECT RANK() OVER (ORDER BY NormalizedScore DESC, CompletedAt ASC) AS Rank,
                   s.UserId, u.UserName, s.NormalizedScore, s.CompletedAt
            FROM exam.ExamSessions s
            JOIN [identity].Users u ON u.Id = s.UserId
            WHERE s.QuizId = (SELECT QuizId FROM exam.ExamSessions WHERE Id = @SessionId)
              AND s.Type = 'Official'
              AND s.Status IN ('Completed', 'TimedOut')
            ORDER BY Rank
            OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;
            """;

        using var multi = await conn.QueryMultipleAsync(sql, new { SessionId = request.SessionId });
        var session = await multi.ReadFirstOrDefaultAsync<dynamic>();
        if (session is null) return Result.Failure<ExamResultDto>(Error.NotFound("ExamSession"));
        if ((Guid)session.UserId != request.UserId) return Result.Failure<ExamResultDto>(Error.Unauthorized());

        var answers = (await multi.ReadAsync<dynamic>()).Select(a => new QuestionResultDto(
            (Guid)a.QuestionId, (string)a.Text, (string?)a.Explanation,
            (Guid?)a.SelectedOptionId, (Guid)a.CorrectOptionId,
            (bool)a.IsCorrect, (double)a.PointsAwarded)).ToList().AsReadOnly();

        var leaderboard = (await multi.ReadAsync<dynamic>()).Select(l => new LeaderboardEntryDto(
            (int)l.Rank, (Guid)l.UserId, (string)l.UserName,
            (double)l.NormalizedScore, (DateTime)l.CompletedAt)).ToList().AsReadOnly();

        bool? passed = session.PassingScore is double ps
            ? (double)session.NormalizedScore >= ps
            : null;

        return Result.Success(new ExamResultDto(
            request.SessionId, (Guid)session.QuizId, (string)session.QuizTitle,
            (string)session.Type, (string)session.Status,
            (double)session.Score, (double)session.NormalizedScore,
            (double?)session.PassingScore, passed,
            (int)session.TotalQuestions, (int)session.CorrectCount,
            (int)session.WrongCount, (int)session.SkippedCount,
            (DateTime)session.StartedAt, (DateTime?)session.CompletedAt,
            answers, leaderboard));
    }
}
