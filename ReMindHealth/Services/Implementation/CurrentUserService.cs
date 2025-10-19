using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using ReMindHealth.Data;
using ReMindHealth.Services.Interfaces;
using System.Security.Claims;

namespace ReMindHealth.Services.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public CurrentUserService(
        AuthenticationStateProvider authenticationStateProvider,
        UserManager<ApplicationUser> userManager)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userManager = userManager;
    }

    public async Task<string> GetUserIdAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(authState.User);

        if (user == null)
            throw new UnauthorizedAccessException("User not authenticated");

        return user.Id;
    }

    public async Task<string?> GetUserEmailAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return authState.User.FindFirstValue(ClaimTypes.Email);
    }

    public async Task<string?> GetUserFullNameAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(authState.User);

        if (user == null)
            return null;

        return $"{user.FirstName} {user.LastName}".Trim();
    }
}