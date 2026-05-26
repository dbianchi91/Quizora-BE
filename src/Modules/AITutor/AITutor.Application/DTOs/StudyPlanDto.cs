namespace AITutor.Application.DTOs;

public sealed record StudyPlanDto(
    string ContentJson,
    DateTime GeneratedAt,
    bool UpdatedAutomatically);

public sealed record ChatMessageResponseDto(string Role, string Content, DateTime CreatedAt);
