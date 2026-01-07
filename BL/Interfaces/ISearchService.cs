using BL.DTOs;

namespace BL.Interfaces;

public interface ISearchService
{
    public Task<List<SearchResultDto>> Search(string query, bool isAuthenticated);
    Task<List<RelevantProfileDto>> SearchRelevantProfiles(string currentUserId, string profileUserId);
}
