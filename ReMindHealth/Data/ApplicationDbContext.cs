using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReMindHealth.Models;

namespace ReMindHealth.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Add DbSets for your custom tables
    public DbSet<UserSettings> UserSettings { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ExtractedAppointment> ExtractedAppointments { get; set; }
    public DbSet<ExtractedTask> ExtractedTasks { get; set; }
    public DbSet<ExtractedNote> ExtractedNotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===================================
        // ApplicationUser Configuration
        // ===================================
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        // ===================================
        // UserSettings Configuration
        // ===================================
        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.HasKey(e => e.UserSettingsId);

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Settings)
                  .HasForeignKey<UserSettings>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.PreferredLanguage).HasMaxLength(10).IsRequired();
            entity.Property(e => e.TimeZone).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AudioQuality).HasMaxLength(20).IsRequired();
        });

        // ===================================
        // Conversation Configuration
        // ===================================
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Conversations)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.RecordedAt);
            entity.HasIndex(e => e.ProcessingStatus);
            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.ConversationType).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.ParticipantNames).HasMaxLength(500);
            entity.Property(e => e.AudioFilePath).HasMaxLength(500);
            entity.Property(e => e.AudioFormat).HasMaxLength(10);
            entity.Property(e => e.TranscriptionLanguage).HasMaxLength(10);
            entity.Property(e => e.ProcessingStatus).HasMaxLength(50).IsRequired();
        });

        // ===================================
        // ExtractedAppointment Configuration
        // ===================================
        modelBuilder.Entity<ExtractedAppointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId);

            entity.HasOne(e => e.Conversation)
                  .WithMany(c => c.ExtractedAppointments)
                  .HasForeignKey(e => e.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.AppointmentDateTime);
            entity.HasIndex(e => e.ConversationId);

            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(300);
            entity.Property(e => e.AttendeeNames).HasMaxLength(500);
            entity.Property(e => e.ConfidenceScore).HasPrecision(5, 4);
        });

        // ===================================
        // ExtractedTask Configuration
        // ===================================
        modelBuilder.Entity<ExtractedTask>(entity =>
        {
            entity.HasKey(e => e.TaskId);

            entity.HasOne(e => e.Conversation)
                  .WithMany(c => c.ExtractedTasks)
                  .HasForeignKey(e => e.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => e.ConversationId);

            entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Priority).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.ConfidenceScore).HasPrecision(5, 4);
        });

        // ===================================
        // ExtractedNote Configuration
        // ===================================
        modelBuilder.Entity<ExtractedNote>(entity =>
        {
            entity.HasKey(e => e.NoteId);

            entity.HasOne(e => e.Conversation)
                  .WithMany(c => c.ExtractedNotes)
                  .HasForeignKey(e => e.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.NoteType);
            entity.HasIndex(e => e.IsPinned);
            entity.HasIndex(e => e.ConversationId);

            entity.Property(e => e.NoteType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.ConfidenceScore).HasPrecision(5, 4);
        });
    }
}