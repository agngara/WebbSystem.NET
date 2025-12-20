
using Microsoft.EntityFrameworkCore;
using Models;

namespace DAL.Repositories;

public class CompetenceRepository(AppDbContext dbcontext) : SqlServerRepository<Competence>(dbcontext)
{
    public async Task<List<Competence>> SearchByCompetence(string query, int limit)
    {
        return await _context.Set<Competence>()
            .Where(c => c.Name.Contains(query))
            .Take(limit)
            .ToListAsync();
    }
}

