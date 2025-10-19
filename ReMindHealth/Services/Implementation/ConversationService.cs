using ReMindHealth.Data;
using ReMindHealth.Models;
using ReMindHealth.Services.Interfaces;

namespace ReMindHealth.Services.Implementations;

public class ConversationService : IConversationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ConversationService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public Task<Conversation?> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ConversationRepository.GetByIdAsync(conversationId, cancellationToken);
    }

    public Task<Conversation?> GetConversationWithDetailsAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ConversationRepository.GetWithDetailsAsync(conversationId, cancellationToken);
    }

    public async Task<List<Conversation>> GetUserConversationsAsync(CancellationToken cancellationToken = default)
    {
        var userId = await _currentUserService.GetUserIdAsync();
        return await _unitOfWork.ConversationRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<List<Conversation>> GetRecentConversationsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var userId = await _currentUserService.GetUserIdAsync();
        return await _unitOfWork.ConversationRepository.GetRecentByUserIdAsync(userId, count, cancellationToken);
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

        await _unitOfWork.ConversationRepository.AddAsync(conversation, cancellationToken);

        return conversation;
    }

    public async Task UpdateConversationAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ConversationRepository.UpdateAsync(conversation, cancellationToken);
    }

    public async Task DeleteConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ConversationRepository.DeleteAsync(conversationId, cancellationToken);
    }
}