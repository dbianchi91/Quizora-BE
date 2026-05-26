using MediatR;
using Quizora.SharedKernel;

namespace ExamEngine.Application.Commands.SubmitExam;

public record SubmitExamCommand(Guid SessionId, Guid UserId, string IdempotencyKey)
    : IRequest<Result>;
