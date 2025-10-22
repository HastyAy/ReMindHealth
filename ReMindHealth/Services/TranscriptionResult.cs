namespace ReMindHealth.Services
{
    public class TranscriptionResult
    {
        public string Text { get; set; } = string.Empty;
        public string Language { get; set; } = "de";
        public decimal Confidence { get; set; }
    }
}
