namespace ReMindHealth.Services.Interfaces;

public interface ICurrentUserService
{
    Task<string> GetUserIdAsync();
    Task<string?> GetUserEmailAsync();
    Task<string?> GetUserFullNameAsync();
}