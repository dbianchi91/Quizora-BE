using ExamEngine.Application.Interfaces;
using ExamEngine.Domain.ValueObjects;
using MediatR;
using Quizora.SharedKernel;

namespace ExamEngine.Application.Commands.AutoSubmitExam;

public sealed class AutoSubmitExamCommandHandler(IExamRepository examRepo)
    : IRequestHandler<AutoSubmitExamCommand, Result>
{
    public async Task<Result> Handle(AutoSubmitExamCommand request, CancellationToken ct)
    {
        var session = await examRepo.GetByIdAsync(ExamSessionId.From(request.SessionId), ct);
        if (session is null) return Result.Success();
        session.AutoSubmit();
        await examRepo.SaveChangesAsync(ct);
        return Result.Success();
    }
}
