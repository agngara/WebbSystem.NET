
using BL.DTOs;
using BL.Interfaces;
using DAL.Repositories.Interfaces;
using DAL.Repositories.Models;

namespace BL.Services;

public class SearchService : ISearchService
{
    private readonly int limit = 5;
    private readonly IUserRepository _userRepo;

    public SearchService(IUserRepository userRepository)
    {
        _userRepo = userRepository;
    }

    public async Task<List<SearchResultDto>> Search(string query)
    {
        var queryResults = await _userRepo.SearchByNameOrUsername(query, limit);
        Console.WriteLine($"startof service: {queryResults.Count()}");

        List<SearchResultDto> returnedResults = new();
        foreach (var res in queryResults)
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

        Console.WriteLine($"service: {returnedResults.Count()}");
        return returnedResults;
    }
}
