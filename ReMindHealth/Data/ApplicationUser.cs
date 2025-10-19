using Microsoft.AspNetCore.Identity;
using ReMindHealth.Models;

namespace ReMindHealth.Data;

public class ApplicationUser : IdentityUser
{
    // Custom fields for your app
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public UserSettings? Settings { get; set; }
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}