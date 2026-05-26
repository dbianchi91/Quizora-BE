using Dapper;
using ExamEngine.Application.DTOs;
using MediatR;
using Quizora.SharedKernel;

namespace ExamEngine.Application.Queries.GetExamState;

public record GetExamStateQuery(Guid SessionId, Guid UserId) : IRequest<Result<ExamStateDto>>;

public sealed class GetExamStateQueryHandler(IDbConnectionFactory db)
    : IRequestHandler<GetExamStateQuery, Result<ExamStateDto>>
{
    public async Task<Result<ExamStateDto>> Handle(GetExamStateQuery request, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var session = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT Id, QuizId, UserId, Type, Status, ExpiresAt, TotalQuestions FROM exam.ExamSessions WHERE Id = @Id",
            new { Id = request.SessionId });
        if (session is null) return Result.Failure<ExamStateDto>(Error.NotFound("ExamSession"));
        if ((Guid)session.UserId != request.UserId) return Result.Failure<ExamStateDto>(Error.Unauthorized());

        var answers = await conn.QueryAsync<dynamic>(
            "SELECT QuestionId, SelectedOptionId FROM exam.SessionAnswers WHERE SessionId = @Id",
            new { Id = request.SessionId });
        var answeredMap = answers.ToDictionary(a => (Guid)a.QuestionId, a => (Guid?)a.SelectedOptionId);

        var questions = await conn.QueryAsync<dynamic>("""
            SELECT q.Id AS QuestionId, q.Text,
                   qo.Id AS OptionId, qo.Text AS OptionText, qq.OrderIndex
            FROM quiz.QuizQuestions qq
            JOIN quiz.Questions q ON q.Id = qq.QuestionId
            JOIN quiz.QuestionOptions qo ON qo.QuestionId = q.Id
            WHERE qq.QuizId = @QuizId ORDER BY qq.OrderIndex, qo.OrderIndex
            """, new { QuizId = (Guid)session.QuizId });

        var questionDtos = questions
            .GroupBy(q => (Guid)q.QuestionId)
            .Select(g => new ExamQuestionStateDto(
                g.Key, (string)g.First().Text,
                g.Select(o => new ExamOptionStateDto((Guid)o.OptionId, (string)o.OptionText)).ToList().AsReadOnly(),
                answeredMap.TryGetValue(g.Key, out var ans) ? ans : null))
            .ToList().AsReadOnly();

        var remaining = session.ExpiresAt is DateTime exp
            ? Math.Max(0, (int)(exp - DateTime.UtcNow).TotalSeconds)
            : int.MaxValue;

        return Result.Success(new ExamStateDto(
            request.SessionId, (Guid)session.QuizId,
            (string)session.Type, (string)session.Status,
            remaining, (int)session.TotalQuestions, answeredMap.Count, questionDtos));
    }
}
