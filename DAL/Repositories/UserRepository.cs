
using DAL.Repositories.Models;
using DAL.Repositories.DTOs;
using Microsoft.EntityFrameworkCore;
using Models;
using DAL.Repositories.Interfaces;

namespace DAL.Repositories;

public class UserRepository : SqlServerRepository<User>, IUserRepository
{
    private readonly SearchScoringConfig _scoringConfig;


    public UserRepository(AppDbContext dbContext, SearchScoringConfig _config) : base(dbContext)
    {
        _scoringConfig = _config;
    }

    public async Task<List<ScoredResult<User>>> SearchByNameOrUsername(string query, int limit)
    {
        string normQuery = query.Trim().ToLower();

        var candidates = await _context.Set<User>()
            .Where(u => u.Name.ToLower().Contains(normQuery)
                     || u.UserName!.ToLower().Contains(normQuery))
            .OrderBy(u => u.Name)
            .Take(100)
            .ToListAsync();

        int exactScore = _scoringConfig.ExactMatchScore;
        int startsScore = _scoringConfig.StartsWithScore;
        int containsScore = _scoringConfig.ContainsScore;

        var scoredResults = candidates
            .Select(u =>
            {
                int score = 0;
                score += u.Name.ToLower() == normQuery ? exactScore : 0;
                score += u.Name.ToLower().StartsWith(normQuery) ? startsScore : 0;
                score += u.Name.ToLower().Contains(normQuery) ? containsScore : 0;
                score += u.UserName!.ToLower() == normQuery ? exactScore : 0;
                score += u.UserName!.ToLower().StartsWith(normQuery) ? startsScore : 0;
                score += u.UserName!.ToLower().Contains(normQuery) ? containsScore : 0;

                bool matchedOnName = u.Name.ToLower() == normQuery ||
                                     u.Name.ToLower().StartsWith(normQuery) ||
                                     u.Name.ToLower().Contains(normQuery);

                return new ScoredResult<User>
                {
                    Entity = u,
                    Score = score,
                    MatchedOn = matchedOnName ? SearchType.Name : SearchType.Username
                };
            })
            .OrderByDescending(r => r.Score)
            .Take(limit)
            .ToList();

        Console.WriteLine($"results: {scoredResults.Count()}");

        return scoredResults;
    }


}
