using MediatR;
using QuizManagement.Application.DTOs;

namespace QuizManagement.Application.Queries.GetQuizById;

public record GetQuizByIdQuery(Guid QuizId) : IRequest<QuizDetailDto?>;
