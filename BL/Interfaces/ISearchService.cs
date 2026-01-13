using BL.DTOs;

namespace BL.Interfaces;

public interface ISearchService
{
    public Task<List<SearchResultDto>> Search(string query, bool isAuthenticated);
    public Task<List<RelevantProfileDto>> SearchRelevantProfiles(string currentUserId, string profileUserId, bool isLoggedIn);
}
