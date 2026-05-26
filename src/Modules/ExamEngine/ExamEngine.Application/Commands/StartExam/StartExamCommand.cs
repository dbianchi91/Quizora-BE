using MediatR;
using Quizora.SharedKernel;

namespace ExamEngine.Application.Commands.StartExam;

public record StartExamCommand(
    Guid QuizId, Guid UserId, string SessionType,
    string IdempotencyKey) : IRequest<Result<Guid>>;
