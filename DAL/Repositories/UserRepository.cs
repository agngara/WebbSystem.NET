
using Microsoft.EntityFrameworkCore;
using Models;

namespace DAL.Repositories;

public class UserRepository : SqlServerRepository<User>
{

    public UserRepository(DbContext dbcontext) : base(dbcontext)
    {
    }

    public async Task<List<User>> SearchByNameOrUsername(string query, int limit)
    {
        return await _context.Set<User>()
            .Where(u => u.Name.Contains(query) || u.UserName.Contains(query))
            .Take(limit)
            .ToListAsync();
    }
}
