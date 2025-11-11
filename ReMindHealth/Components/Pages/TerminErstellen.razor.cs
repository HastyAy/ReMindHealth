using Microsoft.AspNetCore.Components;

namespace ReMindHealth.Components.Pages
{
    public partial class TerminErstellen
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        // Korrekte Typen
        private string titel = "";
        private DateTime datum = DateTime.Today; // Standard: Heute
        private TimeSpan uhrzeit = new TimeSpan(14, 30, 0); // Standard: 14:30
        private string notiz = "";

        private async Task Speichern()
{
    DateTime terminDatum = datum.Date + uhrzeit; // Datum + Uhrzeit kombinieren
    Console.WriteLine($"Termin gespeichert: {titel}, {terminDatum}, Notiz: {notiz}");
    await Task.Delay(300);
    NavigationManager.NavigateTo("/kalender");
}

        
private async Task Loeschen()
{
    Console.WriteLine("Termin gelöscht");
    await Task.CompletedTask;
    NavigationManager.NavigateTo("/kalender");
}

    }
}