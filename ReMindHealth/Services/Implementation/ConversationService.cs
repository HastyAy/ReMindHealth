﻿using FFMpegCore;
using Microsoft.EntityFrameworkCore;
using ReMindHealth.Data;
using ReMindHealth.Models;
using ReMindHealth.Services.Interfaces;
namespace ReMindHealth.Services.Implementations;

public class ConversationService : IConversationService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWhisperService _whisperService;
    private readonly IExtractionService _extractionService;
    private readonly ILogger<ConversationService> _logger;
    private readonly string _audioStoragePath;
    private readonly IServiceProvider _serviceProvider;
    public ConversationService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IWhisperService whisperService,
        IExtractionService extractionService,
        ILogger<ConversationService> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _whisperService = whisperService;
        _extractionService = extractionService;
        _logger = logger;
        _audioStoragePath = configuration["AudioStorage:Path"] ?? "wwwroot/audio";

        Directory.CreateDirectory(_audioStoragePath);
        _serviceProvider = serviceProvider;
    }

    public async Task<Conversation> CreateConversationWithAudioAsync(
    string? note,
    byte[] audioData,
    CancellationToken cancellationToken = default)
    {
        var userId = await _currentUserService.GetUserIdAsync();

        var conversationId = Guid.NewGuid();
        var fileName = $"{conversationId}.webm";
        var filePath = Path.Combine(_audioStoragePath, fileName);

        await File.WriteAllBytesAsync(filePath, audioData, cancellationToken);

        var conversation = new Conversation
        {
            ConversationId = conversationId,
            UserId = userId,
            Title = note ?? $"Gespräch vom {DateTime.Now:dd.MM.yyyy HH:mm}",
            AudioFilePath = filePath,
            AudioFormat = "webm",
            AudioDurationSeconds = EstimateAudioDuration(audioData),
            RecordedAt = DateTime.UtcNow,
            ProcessingStatus = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);


        await Task.Run(async () =>
        {
            try
            {
                await TranscribeOnlyAsync(conversationId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Task.Run] ERROR in background task: {ex.Message}");
                Console.WriteLine($"[Task.Run] Stack: {ex.StackTrace}");
            }
        });



        return conversation;
    }
    public async Task ContinueProcessingFromTranscriptionAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        _ = Task.Run(async () => await ExtractFromTranscriptionAsync(conversationId));
    }

    private async Task ExtractFromTranscriptionAsync(Guid conversationId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var extractionService = scope.ServiceProvider.GetRequiredService<IExtractionService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ConversationService>>();

        try
        {
            var transcriptionData = await context.Conversations
                .Where(c => c.ConversationId == conversationId)
                .Select(c => new { c.TranscriptionText })
                .FirstOrDefaultAsync();

            if (transcriptionData == null || string.IsNullOrEmpty(transcriptionData.TranscriptionText))
            {
                logger.LogWarning("Cannot extract - conversation or transcription not found");
                return;
            }

            await context.Conversations
                .Where(c => c.ConversationId == conversationId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.ProcessingStatus, "Analyzing")
                    .SetProperty(c => c.UpdatedAt, DateTime.UtcNow));

            var extraction = await extractionService.ExtractInformationAsync(transcriptionData.TranscriptionText);

            var updateBuilder = context.Conversations
                .Where(c => c.ConversationId == conversationId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Summary, extraction.Summary ?? string.Empty)
                    .SetProperty(c => c.ProcessingStatus, "Completed")
                    .SetProperty(c => c.ProcessedAt, DateTime.UtcNow)
                    .SetProperty(c => c.UpdatedAt, DateTime.UtcNow));

            // If we have corrected transcription, update it too
            if (!string.IsNullOrEmpty(extraction.CorrectedTranscription))
            {
                await context.Conversations
                    .Where(c => c.ConversationId == conversationId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(c => c.TranscriptionText, extraction.CorrectedTranscription)
                        .SetProperty(c => c.UpdatedAt, DateTime.UtcNow));

                logger.LogInformation("Transcription corrected for conversation {ConversationId}", conversationId);
            }
            else
            {
                await updateBuilder;
            }

            foreach (var apt in extraction.Appointments)
            {
                apt.ConversationId = conversationId;
                context.ExtractedAppointments.Add(apt);
            }

            foreach (var task in extraction.Tasks)
            {
                task.ConversationId = conversationId;
                context.ExtractedTasks.Add(task);
            }

            foreach (var note in extraction.Notes)
            {
                note.ConversationId = conversationId;
                context.ExtractedNotes.Add(note);
            }

            await context.SaveChangesAsync();

            logger.LogInformation("Extraction completed for conversation {ConversationId}: {AppointmentCount} appointments, {TaskCount} tasks, {NoteCount} notes",
                conversationId, extraction.Appointments.Count, extraction.Tasks.Count, extraction.Notes.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting from conversation {ConversationId}", conversationId);

            try
            {
                // Update error status using ExecuteUpdate (no tracking)
                await context.Conversations
                    .Where(c => c.ConversationId == conversationId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(c => c.ProcessingStatus, "Failed")
                        .SetProperty(c => c.ProcessingError, ex.Message)
                        .SetProperty(c => c.UpdatedAt, DateTime.UtcNow));
            }
            catch (Exception saveEx)
            {
                logger.LogError(saveEx, "Error saving failure status for conversation {ConversationId}", conversationId);
            }
        }
    }

    private async Task TranscribeOnlyAsync(Guid conversationId)
    {

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var whisperService = scope.ServiceProvider.GetRequiredService<IWhisperService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ConversationService>>();

        try
        {
            var conversation = await context.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                return;
            }

            // Convert to WAV
            conversation.ProcessingStatus = "Converting";
            await context.SaveChangesAsync();

            var wavPath = await ConvertToWavAsync(conversation.AudioFilePath!, default);

            // Transcribe
            conversation.ProcessingStatus = "Transcribing";
            await context.SaveChangesAsync();

            var transcription = await whisperService.TranscribeAsync(wavPath);

            conversation.TranscriptionText = transcription.Text;
            conversation.TranscriptionLanguage = transcription.Language;
            conversation.ProcessingStatus = "Transcribed";
            await context.SaveChangesAsync();


            logger.LogInformation("Transcription completed for conversation {ConversationId}", conversationId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TranscribeOnlyAsync] ERROR: {ex.Message}");
            Console.WriteLine($"[TranscribeOnlyAsync] Stack: {ex.StackTrace}");
            logger.LogError(ex, "Error transcribing conversation {ConversationId}", conversationId);

            var conversation = await context.Conversations.FindAsync(conversationId);
            if (conversation != null)
            {
                conversation.ProcessingStatus = "Failed";
                conversation.ProcessingError = ex.Message;
                await context.SaveChangesAsync();
            }
        }
    }

    public Task<Conversation?> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return _context.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId && !c.IsDeleted, cancellationToken);
    }

    public Task<Conversation?> GetConversationWithDetailsAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return _context.Conversations
            .AsNoTracking()
            .Include(c => c.ExtractedAppointments)
            .Include(c => c.ExtractedTasks)
            .Include(c => c.ExtractedNotes)
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId && !c.IsDeleted, cancellationToken);
    }

    public async Task<List<Conversation>> GetUserConversationsAsync(CancellationToken cancellationToken = default)
    {
        var userId = await _currentUserService.GetUserIdAsync();
        return await _context.Conversations
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Conversation>> GetRecentConversationsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var userId = await _currentUserService.GetUserIdAsync();
        return await _context.Conversations
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.RecordedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<Conversation> CreateConversationAsync(string? title, string? conversationType = null, CancellationToken cancellationToken = default)
    {
        var userId = await _currentUserService.GetUserIdAsync();

        var conversation = new Conversation
        {
            UserId = userId,
            Title = title ?? $"Gespräch vom {DateTime.Now:dd.MM.yyyy HH:mm}",
            ConversationType = conversationType,
            RecordedAt = DateTime.UtcNow,
            ProcessingStatus = "Pending"
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);

        return conversation;
    }

    public async Task UpdateConversationAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        conversation.UpdatedAt = DateTime.UtcNow;

        // Ensure the entity is tracked
        var entry = _context.Entry(conversation);
        if (entry.State == EntityState.Detached)
        {
            _context.Conversations.Attach(conversation);
            entry.State = EntityState.Modified;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _context.Conversations.FindAsync(new object[] { conversationId }, cancellationToken);
        if (conversation != null)
        {
            conversation.IsDeleted = true;
            conversation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }


    private async Task<string> ConvertToWavAsync(string webmPath, CancellationToken cancellationToken)
    {
        try
        {
            var wavPath = Path.ChangeExtension(webmPath, ".wav");

            _logger.LogInformation("Converting {WebmPath} to WAV format", webmPath);

            await FFMpegArguments
                .FromFileInput(webmPath)
                .OutputToFile(wavPath, overwrite: true, options => options
                    .WithAudioCodec("pcm_s16le")
                    .WithAudioSamplingRate(16000)
                    .ForceFormat("wav"))
                .ProcessAsynchronously();

            _logger.LogInformation("Conversion completed: {WavPath}", wavPath);

            return wavPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting audio file");
            throw new Exception($"FFmpeg conversion failed: {ex.Message}. Make sure FFmpeg is installed.", ex);
        }
    }
    private int EstimateAudioDuration(byte[] audioData)
    {
        // Rough estimation: 1 second of WebM audio ≈ 16KB
        return Math.Max(1, audioData.Length / 16000);
    }

    public async Task UpdateTranscriptionTextOnlyAsync(Guid conversationId, string transcriptionText, CancellationToken cancellationToken = default)
    {
        await _context.Conversations
            .Where(c => c.ConversationId == conversationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.TranscriptionText, transcriptionText)
                .SetProperty(c => c.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
    }
}