using MediatR;
using Quizora.SharedKernel;

namespace ExamEngine.Application.Commands.AbandonExam;

public record AbandonExamCommand(Guid SessionId, Guid UserId) : IRequest<Result>;
