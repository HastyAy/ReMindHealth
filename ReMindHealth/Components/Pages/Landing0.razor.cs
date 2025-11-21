using Microsoft.AspNetCore.Components;

namespace ReMindHealth.Components.Pages
{
    public partial class Landing0
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await Task.Delay(3000); // 3 Sekunden warten
        NavigationManager.NavigateTo("/landing", forceLoad: true);
    }
}

    }
}