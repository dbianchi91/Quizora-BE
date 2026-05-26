using Dapper;
using ExamEngine.Application.DTOs;
using MediatR;
using Quizora.SharedKernel;

namespace ExamEngine.Application.Queries.GetExamHistory;

public record GetExamHistoryQuery(Guid UserId) : IRequest<IReadOnlyList<ExamHistoryDto>>;

public sealed class GetExamHistoryQueryHandler(IDbConnectionFactory db)
    : IRequestHandler<GetExamHistoryQuery, IReadOnlyList<ExamHistoryDto>>
{
    public async Task<IReadOnlyList<ExamHistoryDto>> Handle(GetExamHistoryQuery request, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>("""
            SELECT s.Id, s.QuizId, q.Title AS QuizTitle, s.Type, s.Status,
                   s.NormalizedScore, s.StartedAt, s.CompletedAt
            FROM exam.ExamSessions s
            JOIN quiz.Quizzes q ON q.Id = s.QuizId
            WHERE s.UserId = @UserId ORDER BY s.StartedAt DESC
            """, new { UserId = request.UserId });
        return rows.Select(r => new ExamHistoryDto(
            (Guid)r.Id, (Guid)r.QuizId, (string)r.QuizTitle,
            (string)r.Type, (string)r.Status,
            (double?)r.NormalizedScore, (DateTime)r.StartedAt, (DateTime?)r.CompletedAt))
            .ToList().AsReadOnly();
    }
}
