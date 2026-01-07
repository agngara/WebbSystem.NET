using BL.DTOs;
using BL.Interfaces;
using DAL.Repositories.DTOs;
using DAL.Repositories.Interfaces;
using DAL.Repositories.Models;
using Models;

namespace BL.Services;

public class SearchService : ISearchService
{
    private readonly int limit = 5;
    private readonly IUserRepository _userRepo;
    private readonly ICompetenceRepository _competenceRepo;

    public SearchService(IUserRepository userRepository, ICompetenceRepository competenceRepository)
    {
        _userRepo = userRepository;
        _competenceRepo = competenceRepository;
    }

    public async Task<List<SearchResultDto>> Search(string query, bool isAuthenticated)
    {
        var searchTerms = ParseSearchQuery(query);

        // Dictionary för att hålla koll på vilka termer varje användare matchar
        var userMatchCount = new Dictionary<string, int>();
        var allResults = new Dictionary<string, ScoredResult<User>>();

        // Sök på namn/användarnamn för varje term
        foreach (var term in searchTerms)
        {
            var userResults = await _userRepo.SearchByNameOrUsername(term, limit * 2, isAuthenticated);

            foreach (var result in userResults)
            {
                if (allResults.ContainsKey(result.Entity.Id))
                {
                    // Användaren finns redan - öka poängen
                    allResults[result.Entity.Id].Score += result.Score;
                    userMatchCount[result.Entity.Id]++;
                }
                else
                {
                    allResults[result.Entity.Id] = result;
                    userMatchCount[result.Entity.Id] = 1;
                }
            }
        }

        // Sök på kompetenser för varje term
        foreach (var term in searchTerms)
        {
            var competenceResults = await _competenceRepo.SearchByCompetenceForUsers(term, limit * 2, isAuthenticated);

            foreach (var result in competenceResults)
            {
                if (allResults.ContainsKey(result.Entity.Id))
                {
                    // Användaren finns redan - öka poängen
                    allResults[result.Entity.Id].Score += result.Score;
                    userMatchCount[result.Entity.Id]++;
                    // Uppdatera MatchedOn om kompetens har högre prioritet
                    if (result.MatchedOn == SearchType.Competence)
                    {
                        allResults[result.Entity.Id].MatchedOn = SearchType.Competence;
                    }
                }
                else
                {
                    allResults[result.Entity.Id] = result;
                    userMatchCount[result.Entity.Id] = 1;
                }
            }
        }

        // Filtrera bort användare som inte matchar ALLA termer
        var requiredMatches = searchTerms.Count;
        var completeMatches = allResults
            .Where(kvp => userMatchCount[kvp.Key] >= requiredMatches)
            .Select(kvp => kvp.Value)
            .OrderByDescending(r => r.Score)
            .Take(limit)
            .ToList();

        Console.WriteLine($"startof service: {completeMatches.Count}");

        List<SearchResultDto> returnedResults = new();
        foreach (var res in completeMatches)
        {
            var dto = new SearchResultDto()
            {
                Id = res.Entity.Id,
                DisplayName = res.MatchedOn == SearchType.Name
                    ? res.Entity.Name
                    : res.Entity.UserName ?? res.Entity.Name,
                MatchedOn = res.MatchedOn.ToString()
            };
            returnedResults.Add(dto);
        }

        Console.WriteLine($"service: {returnedResults.Count}");
        return returnedResults;
    }

    private List<string> ParseSearchQuery(string query)
    {
        // Stödjer kommatecken som separator
        if (query.Contains(','))
        {
            return query.Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();
        }

        // Annars splitta på mellanslag
        return query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToList();
    }
}
