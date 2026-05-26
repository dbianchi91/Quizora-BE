using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using MediatR;
using Quizora.SharedKernel;

namespace Analytics.Application.Queries.GetMyHistory;

public sealed record GetMyHistoryQuery(Guid UserId, int Page, int PageSize)
    : IRequest<Result<PagedResult<ExamHistoryDto>>>;

public sealed class GetMyHistoryQueryHandler(IAnalyticsDapperQueries queries)
    : IRequestHandler<GetMyHistoryQuery, Result<PagedResult<ExamHistoryDto>>>
{
    public async Task<Result<PagedResult<ExamHistoryDto>>> Handle(
        GetMyHistoryQuery request, CancellationToken ct)
    {
        var result = await queries.GetExamHistoryAsync(
            request.UserId, request.Page, request.PageSize, ct);
        return Result.Success(result);
    }
}
