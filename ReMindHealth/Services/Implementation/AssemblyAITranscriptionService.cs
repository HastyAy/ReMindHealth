using AssemblyAI;
using AssemblyAI.Transcripts;
using ReMindHealth.Services.Interfaces;

namespace ReMindHealth.Services.Implementations;

public class AssemblyAITranscriptionService : ITranscriptionService
{
    private readonly AssemblyAIClient _client;
    private readonly ILogger<AssemblyAITranscriptionService> _logger;

    public AssemblyAITranscriptionService(
        IConfiguration configuration,
        ILogger<AssemblyAITranscriptionService> logger)
    {
        var apiKey = configuration["AssemblyAI:ApiKey"]
            ?? throw new InvalidOperationException("AssemblyAI API key not configured");

        _client = new AssemblyAIClient(apiKey);
        _logger = logger;
    }

    public async Task<TranscriptionResult> TranscribeAsync(
     string audioFilePath,
     CancellationToken cancellationToken = default)
    {
        try
        {
            var uploadedFile = await _client.Files.UploadAsync(new FileInfo(audioFilePath));

            var transcriptParams = new TranscriptParams
            {
                AudioUrl = uploadedFile.UploadUrl,
                LanguageCode = TranscriptLanguageCode.De,
                Punctuate = true,
                FormatText = true
            };

            var transcript = await _client.Transcripts.TranscribeAsync(transcriptParams);

            if (transcript.Status == TranscriptStatus.Error)
            {
                throw new Exception($"Transcription failed: {transcript.Error}");
            }

            return new TranscriptionResult
            {
                Text = transcript.Text ?? string.Empty,
                Language = "de",
                Duration = transcript.AudioDuration / 1000.0,
                Confidence = transcript.Confidence ?? 0.0,
                Words = transcript.Words?.Select(w => new TranscriptionWord
                {
                    Text = w.Text,
                    Start = w.Start / 1000.0,
                    End = w.End / 1000.0,
                    Confidence = w.Confidence
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during transcription");
            throw;
        }
    }
}