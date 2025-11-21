using Microsoft.AspNetCore.Components;

namespace ReMindHealth.Components.Pages
{
    public partial class Dashboard
    {
        private string greeting = "";

        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        protected override void OnInitialized()
        {
            // Deutsche Zeitzone erzwingen
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
        }

        private void NavigateTo(string url)
        {
            NavigationManager.NavigateTo(url);
        }
    }
}