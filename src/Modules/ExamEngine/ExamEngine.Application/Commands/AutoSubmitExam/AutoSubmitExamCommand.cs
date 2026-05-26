using MediatR;
using Quizora.SharedKernel;

namespace ExamEngine.Application.Commands.AutoSubmitExam;

public record AutoSubmitExamCommand(Guid SessionId) : IRequest<Result>;
