using ReMindHealth.Services.Implementations;

namespace ReMindHealth.Services.Interfaces;

public interface IWhisperService
{
    Task<TranscriptionResult> TranscribeAsync(string audioFilePath, CancellationToken cancellationToken = default);
}