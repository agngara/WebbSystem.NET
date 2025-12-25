

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


    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Messages
            .Where(m => m.Receiver.Id == userId && !m.IsRead)
            .CountAsync();
    }
}
