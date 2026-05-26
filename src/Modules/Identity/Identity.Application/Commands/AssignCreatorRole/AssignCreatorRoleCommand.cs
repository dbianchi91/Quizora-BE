using Identity.Application.Interfaces;
using Identity.Domain.ValueObjects;
using MediatR;
using Quizora.SharedKernel;

namespace Identity.Application.Commands.AssignCreatorRole;

public record AssignCreatorRoleCommand(Guid TargetUserId, bool Grant) : IRequest<Result>;

public sealed class AssignCreatorRoleCommandHandler(IIdentityRepository repository)
    : IRequestHandler<AssignCreatorRoleCommand, Result>
{
    public async Task<Result> Handle(AssignCreatorRoleCommand request, CancellationToken ct)
    {
        var user = await repository.GetByIdAsync(UserId.From(request.TargetUserId), ct);
        if (user is null) return Result.Failure(Error.NotFound("User"));

        if (request.Grant) user.AssignCreatorRole();
        else user.RevokeCreatorRole();

        repository.Update(user);
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
