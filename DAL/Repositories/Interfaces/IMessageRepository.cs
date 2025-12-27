using Models;
namespace DAL.Repositories.Interfaces;

public interface IMessageRepository : IRepository<Message>
{
    public Task<int> GetUnreadCountAsync(string userId);
}

