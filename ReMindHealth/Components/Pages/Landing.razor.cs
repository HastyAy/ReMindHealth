using Microsoft.AspNetCore.Components;

namespace ReMindHealth.Components.Pages
{
    public partial class Landing 
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

       
private void NavigateTo(string url)
{
    NavigationManager.NavigateTo(url);
}

    }
}