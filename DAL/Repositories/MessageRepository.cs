

using DAL.Repositories.Models;
using DAL.Repositories.DTOs;
using Microsoft.EntityFrameworkCore;
using Models;
using DAL.Repositories.Interfaces;

namespace DAL.Repositories;

public class MessageRepository : SqlServerRepository<Message>, IMessageRepository
{
    public MessageRepository(AppDbContext dbContext) : base(dbContext)
    {
    }


    public async Task AddMessageAsync(int conversationId, string senderId, string text)
    {
        _context.Messages.Add(new Message
        {
            ConversationId = conversationId,
            SenderId = senderId,
            Text = text.Trim(),
            SendDateTime = DateTime.UtcNow,
            IsRead = false
        });

        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(int conversationId, string currentUserId)
    {
        var unread = await _context.Messages
            .Where(m => m.ConversationId == conversationId
                     && m.SenderId != currentUserId
                     && !m.IsRead)
            .ToListAsync();

        if (unread.Count == 0) return;

        foreach (var m in unread)
        {
            m.IsRead = true;
            m.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var messages = await _context.Messages
            .Where(m => m.Conversation.Participants.Any(p => p.UserId == userId)
                        && m.SenderId != userId)
            .Select(m => m.IsRead)
            .ToListAsync();

        var unreadCount = messages.Count(m => !m);
        var totalMessages = messages.Count;

        return unreadCount;
    }
}
