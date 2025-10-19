﻿using ReMindHealth.Models;

namespace ReMindHealth.Services.Interfaces;

public interface IConversationService
{
    Task<Conversation?> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<Conversation?> GetConversationWithDetailsAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<List<Conversation>> GetUserConversationsAsync(CancellationToken cancellationToken = default);
    Task<List<Conversation>> GetRecentConversationsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<Conversation> CreateConversationAsync(string? title, string? conversationType = null, CancellationToken cancellationToken = default);
    Task UpdateConversationAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task DeleteConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);
}