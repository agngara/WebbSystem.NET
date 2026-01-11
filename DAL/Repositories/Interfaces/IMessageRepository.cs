using Models;
namespace DAL.Repositories.Interfaces;

public interface IMessageRepository : IRepository<Message>
{

    Task AddMessageAsync(int conversationId, string senderId, string text);
    Task MarkAsReadAsync(int conversationId, string currentUserId);
    //public Task<int> GetUnreadCountAsync(string userId);
}

