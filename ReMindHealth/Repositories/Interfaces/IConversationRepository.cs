using ReMindHealth.Models;

namespace ReMindHealth.Repositories.Interfaces;

public interface IConversationRepository : IRepository<Conversation>
{
    Task<List<Conversation>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Conversation>> GetRecentByUserIdAsync(string userId, int count = 10, CancellationToken cancellationToken = default);
    Task<Conversation?> GetWithDetailsAsync(Guid conversationId, CancellationToken cancellationToken = default);
}