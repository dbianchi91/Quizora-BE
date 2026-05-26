using ExamEngine.Application.DTOs;
using MediatR;
using Quizora.SharedKernel;

namespace ExamEngine.Application.Commands.AnswerQuestion;

public record AnswerQuestionCommand(
    Guid SessionId, Guid QuestionId,
    Guid? SelectedOptionId, int TimeSpentSeconds) : IRequest<Result<AnswerFeedbackDto>>;
