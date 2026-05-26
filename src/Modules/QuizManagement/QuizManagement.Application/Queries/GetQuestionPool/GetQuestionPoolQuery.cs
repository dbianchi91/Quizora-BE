using Dapper;
using MediatR;
using Quizora.SharedKernel;
using QuizManagement.Application.DTOs;

namespace QuizManagement.Application.Queries.GetQuestionPool;

public record GetQuestionPoolQuery(Guid CreatorId) : IRequest<IReadOnlyList<QuestionDto>>;

public sealed class GetQuestionPoolQueryHandler(IDbConnectionFactory db)
    : IRequestHandler<GetQuestionPoolQuery, IReadOnlyList<QuestionDto>>
{
    public async Task<IReadOnlyList<QuestionDto>> Handle(GetQuestionPoolQuery request, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT Id, Text, Explanation, Difficulty, CreatedAt FROM quiz.Questions WHERE CreatorId = @CreatorId",
            new { CreatorId = request.CreatorId });
        return rows.Select(r => new QuestionDto(
            (Guid)r.Id, (string)r.Text, (string?)r.Explanation,
            (string)r.Difficulty, [], (DateTime)r.CreatedAt)).ToList().AsReadOnly();
    }
}
