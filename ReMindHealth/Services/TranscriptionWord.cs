namespace ReMindHealth.Services
{
    public class TranscriptionWord
    {
        public string Text { get; set; } = string.Empty;
        public double Start { get; set; }
        public double End { get; set; }
        public double Confidence { get; set; }
    }
}
