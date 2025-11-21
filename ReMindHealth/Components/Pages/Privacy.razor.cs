using Microsoft.AspNetCore.Components;

namespace ReMindHealth.Components.Pages
{
    public partial class Privacy
    {
        private bool check1 = false;
        private bool check2 = false;
        private bool check3 = false;

        private bool allChecked => check1 && check2 && check3;

        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private void AgreeClicked()
        {
            if (allChecked)
            {
                NavigationManager.NavigateTo("/dashboard"); // dashboard
            }
        }
    }
}