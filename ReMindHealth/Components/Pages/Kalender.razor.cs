using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace ReMindHealth.Components.Pages
{
    public partial class Kalender
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private List<Termin> termine = new()
        {
            new Termin { Id = 1, Titel = "Dr. Schmidt", Datum = new DateTime(2025, 10, 20), Uhrzeit = "14:30", Adresse = "Blumenstraße 3, 92120 Nürnberg" },
            new Termin { Id = 2, Titel = "", Datum = new DateTime(2025, 10, 23), Uhrzeit = "09:00", Adresse = "" },
            new Termin { Id = 3, Titel = "Laboruntersuchung", Datum = new DateTime(2025, 12, 03), Uhrzeit = "08:15", Adresse = "Krankenhausstraße 7, 97070 Würzburg" }
        };

        private Termin? selectedTermin;

        private void SelectTermin(Termin termin)
        {
            selectedTermin = termin;
        }

        private void NavigateTo(string url)
        {
            NavigationManager.NavigateTo(url);
        }

        public class Termin
        {
            public int Id { get; set; }
            public string Titel { get; set; } = "";
            public DateTime Datum { get; set; }
            public string Uhrzeit { get; set; } = "";
            public string Adresse { get; set; } = "";
        }
    }
}

