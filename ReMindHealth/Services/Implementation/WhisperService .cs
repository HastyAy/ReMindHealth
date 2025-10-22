using Whisper.net;
using Whisper.net.Ggml;
using ReMindHealth.Services.Interfaces;

namespace ReMindHealth.Services.Implementations;

public class WhisperService : IWhisperService
{
    private readonly ILogger<WhisperService> _logger;
    private readonly HttpClient _httpClient;
    private static WhisperFactory? _whisperFactory;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly string _modelPath;

    public WhisperService(
        ILogger<WhisperService> logger,
        IConfiguration configuration,
        HttpClient httpClient   )
    {
        _logger = logger;

        // Set model path to user's home directory
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _modelPath = Path.Combine(userProfile, ".whisper", "ggml-base.bin");
        _httpClient = httpClient;
    }

    public async Task<TranscriptionResult> TranscribeAsync(
        string audioFilePath,
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {

            if (!File.Exists(audioFilePath))
            {
                throw new FileNotFoundException($"Audio file not found: {audioFilePath}");
            }

            if (_whisperFactory == null)
            {

                var modelDir = Path.GetDirectoryName(_modelPath)!;
                Directory.CreateDirectory(modelDir);

                if (!File.Exists(_modelPath))
                {
                    _logger.LogInformation("Downloading Whisper model to: {ModelPath}", _modelPath);

                    try
                    {
                        var downloader = new WhisperGgmlDownloader(_httpClient);
                        using var modelStream = await downloader.GetGgmlModelAsync(GgmlType.Base);
                        using var outputStream = File.Create(_modelPath);
                        await modelStream.CopyToAsync(outputStream, cancellationToken);

                        _logger.LogInformation("Whisper model downloaded successfully");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to download Whisper model: {ex.Message}", ex);
                    }
                }
                else
                {
                    Console.WriteLine($" Using cached model: {_modelPath}");
                }

                _whisperFactory = WhisperFactory.FromPath(_modelPath);
                _logger.LogInformation("Whisper model loaded successfully");
            }

            using var processor = _whisperFactory.CreateBuilder()
                .WithLanguage("de")
                .Build();

            var allText = new System.Text.StringBuilder();


            // Open file as stream
            using var fileStream = File.OpenRead(audioFilePath);

            int segmentCount = 0;
            await foreach (var segment in processor.ProcessAsync(fileStream, cancellationToken))
            {
                segmentCount++;
                allText.Append(segment.Text);
                Console.WriteLine($"   Segment {segmentCount}: {segment.Text}");
                _logger.LogDebug("Transcribed segment {Count}: {Text}", segmentCount, segment.Text);
            }

            var fullText = allText.ToString().Trim();

            _logger.LogInformation(
                "Transcription completed. {SegmentCount} segments, {Length} characters",
                segmentCount,
                fullText.Length
            );

            return new TranscriptionResult
            {
                Text = fullText,
                Language = "de",
                Confidence = 0.85m
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Transcription error: {ex.Message}");
            _logger.LogError(ex, "Error transcribing audio file: {AudioFilePath}", audioFilePath);
            throw new Exception($"Transkriptionsfehler: {ex.Message}", ex);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}