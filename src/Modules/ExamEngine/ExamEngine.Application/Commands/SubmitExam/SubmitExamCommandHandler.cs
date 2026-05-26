using ExamEngine.Application.Interfaces;
using ExamEngine.Domain.Enums;
using ExamEngine.Domain.ValueObjects;
using Identity.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Quizora.SharedKernel;

namespace ExamEngine.Application.Commands.SubmitExam;

public sealed class SubmitExamCommandHandler(
    IExamRepository examRepo,
    IExamTimerService timerService,
    IPublisher publisher,
    ILogger<SubmitExamCommandHandler> logger)
    : IRequestHandler<SubmitExamCommand, Result>
{
    public async Task<Result> Handle(SubmitExamCommand request, CancellationToken ct)
    {
        var session = await examRepo.GetByIdAsync(ExamSessionId.From(request.SessionId), ct);
        if (session is null) return Result.Failure(Error.NotFound("ExamSession"));
        if (session.UserId != UserId.From(request.UserId)) return Result.Failure(Error.Unauthorized());
        if (session.Status == SessionStatus.Completed) return Result.Success();

        var result = session.Complete();
        if (result.IsFailure) return result;

        var domainEvents = session.DomainEvents.ToList();
        session.ClearDomainEvents();

        await examRepo.SaveChangesAsync(ct);
        await timerService.CancelAsync(session.Id.Value, ct);

        foreach (var ev in domainEvents)
        {
            try { await publisher.Publish(ev, ct); }
            catch (Exception ex) { logger.LogError(ex, "Failed to publish domain event {EventType}", ev.GetType().Name); }
        }

        return Result.Success();
    }
}
