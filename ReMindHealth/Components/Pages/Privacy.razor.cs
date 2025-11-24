using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using ReMindHealth.Data;

namespace ReMindHealth.Components.Pages
{
    public partial class Privacy
    {
        private bool check1 = false;
        private bool check2 = false;
        private bool check3 = false;
        private bool isLoading = false;
        private bool hasCheckedPrivacy = false;

        private bool allChecked => check1 && check2 && check3;

        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = default!;

        // ✅ Use OnAfterRenderAsync instead of OnInitializedAsync
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !hasCheckedPrivacy)
            {
                hasCheckedPrivacy = true;

                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated == true)
                {
                    var appUser = await UserManager.GetUserAsync(user);

                    if (appUser?.HasAcceptedPrivacy == true)
                    {
                        NavigationManager.NavigateTo("/dashboard");
                    }
                }
            }
        }

        private async Task AgreeClicked()
        {
            if (!allChecked) return;

            isLoading = true;

            try
            {
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated == true)
                {
                    var appUser = await UserManager.GetUserAsync(user);

                    if (appUser != null)
                    {
                        appUser.HasAcceptedPrivacy = true;
                        appUser.PrivacyAcceptedAt = DateTime.UtcNow;

                        var result = await UserManager.UpdateAsync(appUser);

                        if (result.Succeeded)
                        {
                            // ✅ This navigation is fine (it's in a button click handler)
                            NavigationManager.NavigateTo("/dashboard");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting privacy: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }
    }
}