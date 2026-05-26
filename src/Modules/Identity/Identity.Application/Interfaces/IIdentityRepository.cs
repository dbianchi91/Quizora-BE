using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;

namespace Identity.Application.Interfaces;

public interface IIdentityRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenAsync(string token, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<string> HashPasswordAsync(string password);
    Task<bool> CheckPasswordAsync(User user, string password);
}
