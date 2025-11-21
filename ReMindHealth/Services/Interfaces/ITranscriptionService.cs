using ReMindHealth.Services.Implementations;

namespace ReMindHealth.Services.Interfaces;

public interface ITranscriptionService
{
    Task<TranscriptionResult> TranscribeAsync(string audioFilePath, CancellationToken cancellationToken = default);
}