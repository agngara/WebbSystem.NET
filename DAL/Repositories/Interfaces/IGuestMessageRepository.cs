using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories.Interfaces
{
    public interface IGuestMessageRepository
    {
        Task AddGuestMessageAsync(string toUserId, string guestName, string text);
        Task<List<GuestMessage>> GetInboxForUserAsync(string userId); 
        Task<GuestMessage?> GetByIdForUserAsync(int id, string userId);
        Task MarkAsReadAsync(int id, string userId);

        Task DeleteMessageAsync(int guestMessageId, string currentUserId);
    }
}
