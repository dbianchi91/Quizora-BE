using MediatR;
using QuizManagement.Application.DTOs;

namespace QuizManagement.Application.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
