using ReMindHealth.Models;

namespace ReMindHealth.Repositories.Interfaces;

public interface IUserSettingsRepository
{
    Task<UserSettings?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserSettings> CreateAsync(UserSettings settings, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserSettings settings, CancellationToken cancellationToken = default);
}