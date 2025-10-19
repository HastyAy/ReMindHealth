namespace ReMindHealth.Models;

public class ExtractedAppointment
{
    public Guid AppointmentId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid? CalendarEventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public int? DurationMinutes { get; set; }
    public bool IsAllDay { get; set; }
    public string? AttendeeNames { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public bool IsConfirmed { get; set; }
    public bool IsAddedToCalendar { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Conversation Conversation { get; set; } = null!;
}