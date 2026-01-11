using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories
{
    public class ConversationRepository : SqlServerRepository<Conversation>, IConversationRepository
    {
        private readonly AppDbContext _context;

        public ConversationRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int?> GetExistingConversationIdAsync(string userId, string otherUserId)
        {
            return await _context.Conversations
                .Where(c => c.Participants.Any(p => p.UserId == userId)
                         && c.Participants.Any(p => p.UserId == otherUserId))
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateConversationAsync(string userId, string otherUserId)
        {
            var conv = new Conversation
            {
                Participants = new List<ConversationUser>
            {
                new ConversationUser { UserId = userId },
                new ConversationUser { UserId = otherUserId }
            }
            };

            _context.Conversations.Add(conv);
            await _context.SaveChangesAsync();
            return conv.Id;
        }

        public async Task<List<Conversation>> GetConversationsForUserAsync(string userId)
        {
            return await _context.Conversations
                
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                        .ThenInclude(u => u.Cv)

                .Include(c => c.Messages)

                .ToListAsync();
        }

        public async Task<Conversation?> GetConversationWithMessagesAsync(int conversationId)
        {
            return await _context.Conversations
                .Include(c => c.Participants).ThenInclude(p => p.User)
                .Include(c => c.Messages).ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
        }

        public async Task<bool> UserIsParticipantAsync(int conversationId, string userId)
        {
            return await _context.Conversations
                .AnyAsync(c => c.Id == conversationId && c.Participants.Any(p => p.UserId == userId));
        }

      
    }
}

