namespace Identity.Application.DTOs;

public record UserDto(Guid Id, string Email, string UserName, bool IsEmailConfirmed, bool IsCreator, bool IsAdmin, DateTime CreatedAt, DateTime? LastLoginAt);
