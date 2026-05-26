using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public sealed class IdentityRepository(
    IdentityDbContext context,
    IPasswordHasher<User> passwordHasher) : IIdentityRepository
{
    public async Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default) =>
        await context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.Value == email.ToLowerInvariant(), ct);

    public async Task<User?> GetByRefreshTokenAsync(string token, CancellationToken ct = default) =>
        await context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == token && !rt.RevokedAt.HasValue), ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        await context.Users.AnyAsync(u => u.Email.Value == email.ToLowerInvariant(), ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await context.Users.AddAsync(user, ct);

    public void Update(User user)
    {
        // Disable auto-detect before checking states: accessing context.Entry() triggers
        // DetectChanges which misclassifies new RefreshTokens (non-default Guid key +
        // ValueGeneratedOnAdd) as Modified instead of Detached, causing UPDATE instead of INSERT.
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            foreach (var rt in user.RefreshTokens
                         .Where(rt => context.Entry(rt).State == EntityState.Detached))
                context.RefreshTokens.Add(rt);

            if (context.Entry(user).State == EntityState.Detached)
                context.Users.Update(user);
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);

    public Task<string> HashPasswordAsync(string password) =>
        Task.FromResult(passwordHasher.HashPassword(null!, password));

    public Task<bool> CheckPasswordAsync(User user, string password)
    {
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return Task.FromResult(result == PasswordVerificationResult.Success ||
                               result == PasswordVerificationResult.SuccessRehashNeeded);
    }
}
