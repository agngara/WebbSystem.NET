using DAL.Repositories.DTOs;
using DAL.Repositories.Interfaces;
using DAL.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DAL.Repositories;

public class CompetenceRepository : SqlServerRepository<Competence>, ICompetenceRepository
{
    private readonly SearchScoringConfig _scoringConfig;

    public CompetenceRepository(AppDbContext dbContext, SearchScoringConfig scoringConfig)
        : base(dbContext)
    {
        _scoringConfig = scoringConfig;
    }

    public async Task<List<Competence>> SearchByCompetence(string query, int limit)
    {
        return await _context.Set<Competence>()
            .Where(c => c.Name.Contains(query))
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<ScoredResult<User>>> SearchByCompetenceForUsers(string query, int limit, bool isAuthenticated)
    {
        string normQuery = query.Trim().ToLower();

        var Dbquery = _context.Set<Competence>()
            .Include(c => c.Cv)
                .ThenInclude(cv => cv.User)
            .AsQueryable();

        if (!isAuthenticated)
        {
            Dbquery = Dbquery.Where(c => c.Cv.User.IsPrivate != true);
        }

        var competences = await Dbquery
            .Where(c => c.Name.ToLower().Contains(normQuery))
            .Take(100)
            .ToListAsync();

        int exactScore = _scoringConfig.ExactMatchScore;
        int startsScore = _scoringConfig.StartsWithScore;
        int containsScore = _scoringConfig.ContainsScore;

        var scoredResults = competences
            .GroupBy(c => c.Cv.User)
            .Select(g =>
            {
                int score = 0;
                var matchedCompetences = new List<string>();

                foreach (var competence in g)
                {
                    if (competence.Name.ToLower() == normQuery)
                    {
                        score += exactScore;
                        matchedCompetences.Add(competence.Name);
                    }
                    else if (competence.Name.ToLower().StartsWith(normQuery))
                    {
                        score += startsScore;
                        matchedCompetences.Add(competence.Name);
                    }
                    else if (competence.Name.ToLower().Contains(normQuery))
                    {
                        score += containsScore;
                        matchedCompetences.Add(competence.Name);
                    }
                }

                return new ScoredResult<User>
                {
                    Entity = g.Key,
                    Score = score,
                    MatchedOn = SearchType.Competence,
                    MatchedCompetences = matchedCompetences
                };
            })
            .OrderByDescending(r => r.Score)
            .Take(limit)
            .ToList();

        Console.WriteLine($"competence results: {scoredResults.Count}");
        return scoredResults;
    }
}
