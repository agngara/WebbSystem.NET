using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories
{
    public class GuestMessageRepository : SqlServerRepository<GuestMessage>, IGuestMessageRepository
    {
        private readonly AppDbContext _context;

    public GuestMessageRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task AddGuestMessageAsync(string toUserId, string guestName, string text)
    {


             _context.GuestMessages.Add(new GuestMessage
                {
                    ToUserId = toUserId,
                    GuestName = guestName.Trim(),
                    Text = text.Trim(),
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                });
                await _context.SaveChangesAsync();
                
            

          

         
    }

    public async Task<List<GuestMessage>> GetInboxForUserAsync(string userId)
    {
        return await _context.GuestMessages
            .Where(g => g.ToUserId == userId && !g.IsDeleteed)
            .OrderByDescending(g => g.SentAt)
            .ToListAsync();
    }

    public async Task<GuestMessage?> GetByIdForUserAsync(int id, string userId)
    {
        return await _context.GuestMessages
            .FirstOrDefaultAsync(g => g.Id == id && g.ToUserId == userId);
    }

    public async Task MarkAsReadAsync(int id, string userId)
    {
        var g = await GetByIdForUserAsync(id, userId);
        if (g == null) return;

        if (!g.IsRead)
        {
            g.IsRead = true;
            g.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

        public async Task DeleteMessageAsync(int guestMessageId, string currentId)
        {
            var guestMessage = await _context.GuestMessages
                 .FirstOrDefaultAsync(g => g.Id == guestMessageId && g.ToUserId == currentId);

            if (guestMessage == null) return; 

            guestMessage.IsDeleteed = true;

            await _context.SaveChangesAsync();

        }
    }
}
