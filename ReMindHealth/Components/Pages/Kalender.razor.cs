using Microsoft.AspNetCore.Components;

namespace ReMindHealth.Components.Pages
{
    public partial class Kalender
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private List<Termin> termine = new()
        {
            new Termin { Id = 1, Titel = "Dr. Schmidt", Datum = new DateTime(2025, 10, 23), Uhrzeit = "14:30" },
            new Termin { Id = 2, Titel = "Laboruntersuchung", Datum = new DateTime(2025, 12, 03), Uhrzeit = "09:00" }
        };

        private void NavigateTo(string url)
        {
            NavigationManager.NavigateTo(url);
        }

        private void NavigateToDetails(int id)
        {
            NavigationManager.NavigateTo($"/termin-details/{id}");
        }

        public class Termin
        {
            public int Id { get; set; }
            public string Titel { get; set; } = "";
            public DateTime Datum { get; set; }
            public string Uhrzeit { get; set; } = "";
        }
    }
}