namespace AITutor.Application.Services;

public interface IUserContextBuilder
{
    Task<string> BuildSystemPromptAsync(Guid userId, string? pageContext, CancellationToken ct);
}
