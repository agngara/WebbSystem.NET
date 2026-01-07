using DAL.Repositories.DTOs;
using Models;

namespace DAL.Repositories.Interfaces;

public interface ICompetenceRepository : IRepository<Competence>
{
    Task<List<Competence>> SearchByCompetence(string query, int limit);
    Task<List<ScoredResult<User>>> SearchByCompetenceForUsers(string query, int limit, bool isAuthenticated);
}
