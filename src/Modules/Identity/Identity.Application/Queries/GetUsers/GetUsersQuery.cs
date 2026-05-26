using Dapper;
using Identity.Application.DTOs;
using MediatR;
using Quizora.SharedKernel;

namespace Identity.Application.Queries.GetUsers;

public record AdminUserDto(
    Guid Id, string Email, string UserName,
    bool IsEmailConfirmed, bool IsCreator, bool IsAdmin,
    DateTime CreatedAt, DateTime? LastLoginAt);

public record GetUsersQuery(int Page = 1, int PageSize = 20) : IRequest<IReadOnlyList<AdminUserDto>>;

public sealed class GetUsersQueryHandler(IDbConnectionFactory db)
    : IRequestHandler<GetUsersQuery, IReadOnlyList<AdminUserDto>>
{
    public async Task<IReadOnlyList<AdminUserDto>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var offset = (request.Page - 1) * request.PageSize;

        var rows = await conn.QueryAsync<AdminUserDto>("""
            SELECT Id, Email, UserName, IsEmailConfirmed, IsCreator, IsAdmin, CreatedAt, LastLoginAt
            FROM [identity].Users
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """, new { Offset = offset, PageSize = request.PageSize });

        return rows.AsList();
    }
}
