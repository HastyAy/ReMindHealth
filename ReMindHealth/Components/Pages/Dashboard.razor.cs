using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using ReMindHealth.Data;
using ReMindHealth.Services.Interfaces;

namespace ReMindHealth.Components.Pages
{
    public partial class Dashboard
    {
        private string greeting = "";
        private bool isLoading = true;

        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var appUser = await UserManager.GetUserAsync(user);

                if (appUser != null && !appUser.HasAcceptedPrivacy)
                {
                    NavigationManager.NavigateTo("/privacy");
                    return; 
                }
            }

            await LoadDashboardAsync();
        }

        private async Task LoadDashboardAsync()
        {
            try
            {
                // Set greeting
                var berlinTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
                DateTime berlinTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, berlinTimeZone);
                int hour = berlinTime.Hour;

                if (hour >= 6 && hour < 12)
                {
                    greeting = "Guten Morgen 👋";
                }
                else if (hour >= 12 && hour < 18)
                {
                    greeting = "Guten Mittag ☀️";
                }
                else
                {
                    greeting = "Guten Abend 🌙";
                }

                // Load your dashboard data here...
            }
            finally
            {
                isLoading = false;
            }
        }

        private void NavigateTo(string url)
        {
            NavigationManager.NavigateTo(url);
        }
    }
}