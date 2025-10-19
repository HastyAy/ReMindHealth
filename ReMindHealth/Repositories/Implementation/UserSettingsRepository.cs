using Microsoft.EntityFrameworkCore;
using ReMindHealth.Data;
using ReMindHealth.Models;
using ReMindHealth.Repositories.Interfaces;

namespace ReMindHealth.Repositories.Implementations;

public class UserSettingsRepository : IUserSettingsRepository
{
    private readonly ApplicationDbContext _context;

    public UserSettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserSettings?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task<UserSettings> CreateAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        settings.UserSettingsId = Guid.NewGuid();
        settings.CreatedAt = DateTime.UtcNow;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.UserSettings.AddAsync(settings, cancellationToken);
        return settings;
    }

    public Task UpdateAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        _context.UserSettings.Update(settings);
        return Task.CompletedTask;
    }
}