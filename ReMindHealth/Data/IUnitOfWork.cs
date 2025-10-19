using ReMindHealth.Repositories.Interfaces;

namespace ReMindHealth.Data;

public interface IUnitOfWork : IDisposable
{
    IConversationRepository ConversationRepository { get; }
    IAppointmentRepository AppointmentRepository { get; }
    ITaskRepository TaskRepository { get; }
    INoteRepository NoteRepository { get; }
    IUserSettingsRepository UserSettingsRepository { get; }

    Task SaveAsync(CancellationToken cancellationToken = default);
}