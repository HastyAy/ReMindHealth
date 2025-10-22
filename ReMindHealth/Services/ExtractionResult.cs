using ReMindHealth.Models;

namespace ReMindHealth.Services
{
    public class ExtractionResult
    {
        public string Summary { get; set; } = string.Empty;
        public string? CorrectedTranscription { get; set; }
        public List<ExtractedAppointment> Appointments { get; set; } = new();
        public List<ExtractedTask> Tasks { get; set; } = new();
        public List<ExtractedNote> Notes { get; set; } = new();
    }
}
