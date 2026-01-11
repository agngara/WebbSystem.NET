using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories.Interfaces
{
    public interface IConversationRepository
    {
        Task<int?> GetExistingConversationIdAsync(string userId, string otherUserId);
        Task<int> CreateConversationAsync(string userId, string otherUserId);

        Task<List<Conversation>> GetConversationsForUserAsync(string userId);
        Task<Conversation?> GetConversationWithMessagesAsync(int conversationId);
        Task<bool> UserIsParticipantAsync(int conversationId, string userId);

       
    }
}
