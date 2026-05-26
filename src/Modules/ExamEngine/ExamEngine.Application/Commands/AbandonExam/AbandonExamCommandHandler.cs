using ExamEngine.Application.Interfaces;
using ExamEngine.Domain.ValueObjects;
using Identity.Domain.ValueObjects;
using MediatR;
using Quizora.SharedKernel;

namespace ExamEngine.Application.Commands.AbandonExam;

public sealed class AbandonExamCommandHandler(IExamRepository examRepo, IExamTimerService timerService)
    : IRequestHandler<AbandonExamCommand, Result>
{
    public async Task<Result> Handle(AbandonExamCommand request, CancellationToken ct)
    {
        var session = await examRepo.GetByIdAsync(ExamSessionId.From(request.SessionId), ct);
        if (session is null) return Result.Failure(Error.NotFound("ExamSession"));
        if (session.UserId != UserId.From(request.UserId)) return Result.Failure(Error.Unauthorized());
        session.Abandon();
        await examRepo.SaveChangesAsync(ct);
        await timerService.CancelAsync(session.Id.Value, ct);
        return Result.Success();
    }
}
