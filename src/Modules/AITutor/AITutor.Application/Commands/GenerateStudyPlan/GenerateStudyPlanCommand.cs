using MediatR;
using Quizora.SharedKernel;

namespace AITutor.Application.Commands.GenerateStudyPlan;

public record GenerateStudyPlanCommand(Guid UserId, bool Automatic) : IRequest<Result>;
