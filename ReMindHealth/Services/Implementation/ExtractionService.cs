using System.Text;
using System.Text.Json;
using ReMindHealth.Models;
using ReMindHealth.Services.Interfaces;

namespace ReMindHealth.Services.Implementations;

public class ExtractionService : IExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExtractionService> _logger;
    private readonly string _ollamaApiUrl;
    private readonly string _modelName;

    public ExtractionService(
        HttpClient httpClient,
        ILogger<ExtractionService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _ollamaApiUrl = configuration["Ollama:ApiUrl"] ?? "http://localhost:11434";
        _modelName = configuration["Ollama:ModelName"] ?? "llama3.2";

        // Set timeout for AI processing
        _httpClient.Timeout = TimeSpan.FromMinutes(3);
    }

    public async Task<ExtractionResult> ExtractInformationAsync(string transcriptionText, CancellationToken cancellationToken = default)
    {
        try
        {

            var correctedText = await CorrectTranscriptionAsync(transcriptionText, cancellationToken);
            _logger.LogInformation("Corrected text: {CorrectedText}", correctedText);

            var prompt = BuildExtractionPrompt(correctedText);
            var ollamaResponse = await CallOllamaAsync(prompt, cancellationToken);
            var extractionResult = ParseExtractionResponse(ollamaResponse);

            // Add the corrected text to the summary
            extractionResult.CorrectedTranscription = correctedText;

            _logger.LogInformation(
                "Extraction completed. Found: {AppointmentCount} appointments, {TaskCount} tasks, {NoteCount} notes",
                extractionResult.Appointments.Count,
                extractionResult.Tasks.Count,
                extractionResult.Notes.Count);

            return extractionResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during AI extraction");
            throw;
        }
    }

    private async Task<string> CorrectTranscriptionAsync(string transcriptionText, CancellationToken cancellationToken)
    {
        var prompt = $@"Du bist ein KI-Assistent zur Korrektur von Transkriptionsfehlern. 

Die folgende Transkription wurde automatisch erstellt und enthält Fehler aufgrund von Spracherkennung (ca. 80% Genauigkeit). 
Deine Aufgabe ist es, den Text zu korrigieren und verständlich zu machen, OHNE den Inhalt oder die Bedeutung zu ändern.

WICHTIGE REGELN:
1. Korrigiere nur offensichtliche Fehler (z.B. ""Lapur verteber"" → ""Laborwerte"", ""Zember"" → ""Dezember"")
2. Verbessere die Grammatik und Rechtschreibung
3. Füge KEINE neuen Informationen hinzu
4. Lösche KEINE Informationen
5. Behalte alle Zahlen, Daten, Namen und spezifischen Details bei
6. Achte besonders auf:
   - Medizinische Begriffe (z.B. ""Blutdruck"", ""Laborwerte"", ""Medikamente"")
   - Zeitangaben und Daten
   - Namen von Personen und Orten
   - Zahlen und Messungen

ORIGINAL TRANSKRIPTION:
{transcriptionText}

Antworte NUR mit dem korrigierten Text, ohne zusätzliche Kommentare oder Erklärungen.
Der korrigierte Text sollte natürlich klingen und alle wichtigen Informationen aus dem Original enthalten.

KORRIGIERTER TEXT:";

        var correctedText = await CallOllamaAsync(prompt, cancellationToken);

        correctedText = correctedText.Trim();

        return correctedText;
    }

    private string BuildExtractionPrompt(string transcription)
    {
        return $@"Du bist ein KI-Assistent für die Extraktion strukturierter Informationen aus Gesprächstranskripten.

Der folgende Text wurde bereits korrigiert und bereinigt. Analysiere ihn sorgfältig und extrahiere:
1. **Termine/Appointments**: Zeitpunkte für Treffen, Arztbesuche, Meetings etc.
2. **Aufgaben/Tasks**: To-dos, Erinnerungen, Dinge die erledigt werden müssen
3. **Wichtige Notizen**: Medizinische Informationen, Entscheidungen, wichtige Fakten
4. **Zusammenfassung**: Eine kurze Zusammenfassung des Gesprächs

TRANSKRIPT:
{transcription}

Antworte NUR mit einem gültigen JSON-Objekt in folgendem Format (keine zusätzlichen Texte):

{{
  ""summary"": ""Kurze Zusammenfassung des Gesprächs"",
  ""appointments"": [
    {{
      ""title"": ""Titel des Termins"",
      ""description"": ""Beschreibung (optional)"",
      ""location"": ""Ort (optional)"",
      ""dateTime"": ""2025-11-20T10:00:00"",
      ""durationMinutes"": 30,
      ""isAllDay"": false,
      ""attendeeNames"": ""Namen der Teilnehmer (optional)"",
      ""confidenceScore"": 0.95
    }}
  ],
  ""tasks"": [
    {{
      ""title"": ""Aufgabe"",
      ""description"": ""Details (optional)"",
      ""dueDate"": ""2025-11-22T00:00:00"",
      ""priority"": ""Medium"",
      ""category"": ""Gesundheit"",
      ""confidenceScore"": 0.90
    }}
  ],
  ""notes"": [
    {{
      ""noteType"": ""Medical"",
      ""title"": ""Titel der Notiz"",
      ""content"": ""Inhalt der Notiz"",
      ""category"": ""Gesundheit"",
      ""tags"": ""blutdruck,medikation"",
      ""confidenceScore"": 0.95
    }}
  ]
}}

WICHTIG:
- Verwende ISO 8601 Format für Datumswerte
- Priority kann sein: Low, Medium, High
- NoteType kann sein: General, Medical, Financial, Personal
- Wenn keine Informationen gefunden werden, gib leere Arrays zurück
- ConfidenceScore ist eine Zahl zwischen 0 und 1
- Achte auf deutsche Zeitangaben wie ""in 6 Wochen"", ""nächste Woche"", ""morgen"" etc.
- Berechne relative Daten basierend auf dem heutigen Datum: {DateTime.Now:yyyy-MM-dd}
- Extrahiere ALLE genannten Termine, Aufgaben und wichtigen Informationen
- Sei präzise bei Datums- und Zeitangaben";
    }

    private async Task<string> CallOllamaAsync(string prompt, CancellationToken cancellationToken)
    {
        var request = new
        {
            model = _modelName,
            prompt = prompt,
            stream = false,
            options = new
            {
                temperature = 0.2,  // Lower temperature for more consistent results
                top_p = 0.9,
                num_predict = 2000  // Allow longer responses
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogDebug("Calling Ollama API at: {Url}", $"{_ollamaApiUrl}/api/generate");

        var response = await _httpClient.PostAsync($"{_ollamaApiUrl}/api/generate", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Ollama API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
            throw new Exception($"Ollama API request failed: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (ollamaResponse == null || string.IsNullOrEmpty(ollamaResponse.Response))
        {
            throw new Exception("Empty response from Ollama");
        }

        return ollamaResponse.Response;
    }

    private ExtractionResult ParseExtractionResponse(string jsonResponse)
    {
        try
        {
            jsonResponse = jsonResponse.Trim();
            if (jsonResponse.StartsWith("```json"))
            {
                jsonResponse = jsonResponse.Substring(7);
            }
            if (jsonResponse.StartsWith("```"))
            {
                jsonResponse = jsonResponse.Substring(3);
            }
            if (jsonResponse.EndsWith("```"))
            {
                jsonResponse = jsonResponse.Substring(0, jsonResponse.Length - 3);
            }
            jsonResponse = jsonResponse.Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var aiResponse = JsonSerializer.Deserialize<AIExtractionResponse>(jsonResponse, options);

            if (aiResponse == null)
            {
                throw new Exception("Failed to deserialize AI response");
            }

            // Convert AI response to domain models
            var result = new ExtractionResult
            {
                Summary = aiResponse.Summary ?? string.Empty,
                Appointments = aiResponse.Appointments?.Select(a => new ExtractedAppointment
                {
                    AppointmentId = Guid.NewGuid(),
                    Title = a.Title,
                    Description = a.Description,
                    Location = a.Location,
                    AppointmentDateTime = DateTime.Parse(a.DateTime),
                    DurationMinutes = a.DurationMinutes,
                    IsAllDay = a.IsAllDay,
                    AttendeeNames = a.AttendeeNames,
                    ConfidenceScore = a.ConfidenceScore,
                    IsConfirmed = false,
                    IsAddedToCalendar = false,
                    CreatedAt = DateTime.UtcNow
                }).ToList() ?? new List<ExtractedAppointment>(),

                Tasks = aiResponse.Tasks?.Select(t => new ExtractedTask
                {
                    TaskId = Guid.NewGuid(),
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = string.IsNullOrEmpty(t.DueDate) ? null : DateTime.Parse(t.DueDate),
                    Priority = t.Priority ?? "Medium",
                    Category = t.Category,
                    IsCompleted = false,
                    ConfidenceScore = t.ConfidenceScore,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList() ?? new List<ExtractedTask>(),

                Notes = aiResponse.Notes?.Select(n => new ExtractedNote
                {
                    NoteId = Guid.NewGuid(),
                    NoteType = n.NoteType ?? "General",
                    Title = n.Title,
                    Content = n.Content,
                    Category = n.Category,
                    Tags = n.Tags,
                    ConfidenceScore = n.ConfidenceScore,
                    IsPinned = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList() ?? new List<ExtractedNote>()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing extraction response: {JsonResponse}", jsonResponse);

            // Return empty result if parsing fails
            return new ExtractionResult
            {
                Summary = "Fehler bei der Extraktion. Bitte überprüfen Sie die Aufnahme manuell.",
                Appointments = new List<ExtractedAppointment>(),
                Tasks = new List<ExtractedTask>(),
                Notes = new List<ExtractedNote>()
            };
        }
    }

    private class OllamaResponse
    {
        public string Model { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public bool Done { get; set; }
    }

    private class AIExtractionResponse
    {
        public string? Summary { get; set; }
        public List<AIAppointment>? Appointments { get; set; }
        public List<AITask>? Tasks { get; set; }
        public List<AINote>? Notes { get; set; }
    }

    private class AIAppointment
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string DateTime { get; set; } = string.Empty;
        public int DurationMinutes { get; set; } = 30;
        public bool IsAllDay { get; set; }
        public string? AttendeeNames { get; set; }
        public decimal ConfidenceScore { get; set; }
    }

    private class AITask
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? DueDate { get; set; }
        public string? Priority { get; set; }
        public string? Category { get; set; }
        public decimal ConfidenceScore { get; set; }
    }

    private class AINote
    {
        public string? NoteType { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Tags { get; set; }
        public decimal ConfidenceScore { get; set; }
    }
}