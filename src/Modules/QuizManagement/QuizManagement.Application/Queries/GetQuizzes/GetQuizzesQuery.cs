using MediatR;
using QuizManagement.Application.DTOs;

namespace QuizManagement.Application.Queries.GetQuizzes;

public record GetQuizzesQuery(Guid? CategoryId, string? Search, int Page = 1, int PageSize = 20)
    : IRequest<IReadOnlyList<QuizSummaryDto>>;
