using AITutor.Application.DTOs;
using AITutor.Application.Interfaces;
using Identity.Domain.ValueObjects;
using MediatR;

namespace AITutor.Application.Queries.GetStudyPlan;

public record GetStudyPlanQuery(Guid UserId) : IRequest<StudyPlanDto?>;

public sealed class GetStudyPlanQueryHandler(IAITutorRepository repo)
    : IRequestHandler<GetStudyPlanQuery, StudyPlanDto?>
{
    public async Task<StudyPlanDto?> Handle(GetStudyPlanQuery request, CancellationToken ct)
    {
        var plan = await repo.GetStudyPlanAsync(UserId.From(request.UserId), ct);
        if (plan is null) return null;
        return new StudyPlanDto(plan.ContentJson, plan.GeneratedAt, plan.UpdatedAutomatically);
    }
}
