using DAL.Repositories.DTOs;
using Models;
namespace DAL.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{

    public Task<User> GetUserById(string id);
    public Task<List<User>> SearchUsersAsync(string searchTerm);


    public Task<List<ScoredResult<User>>> SearchByNameOrUsername(string query, int limit, bool isAuthenticated);
    public Task<List<ScoredResult<User>>> FindRelevantUsersForCv(int cvId, List<string> excludeUserIds, bool isUserLoggedIn, int limit = 5);

    public Task<User> GetUserForCV(string id);
}
