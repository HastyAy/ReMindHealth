namespace ReMindHealth.Models;

public class UserSettings
{
    public Guid UserSettingsId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "de-DE";
    public string TimeZone { get; set; } = "Europe/Zurich";
    public int ReminderLeadTimeMinutes { get; set; } = 15;
    public bool EnableEmailNotifications { get; set; } = true;
    public bool EnablePushNotifications { get; set; } = true;
    public bool AutoCreateCalendarEvents { get; set; } = true;
    public string AudioQuality { get; set; } = "medium";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Data.ApplicationUser User { get; set; } = null!;
}