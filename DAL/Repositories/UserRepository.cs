using DAL.Repositories.Models;
using DAL.Repositories.DTOs;
using Microsoft.EntityFrameworkCore;
using Models;
using DAL.Repositories.Interfaces;

namespace DAL.Repositories;

public class UserRepository : SqlServerRepository<User>, IUserRepository
{
    private readonly SearchScoringConfig _scoringConfig;
    private readonly AppDbContext _context;


    public UserRepository(AppDbContext dbContext, SearchScoringConfig _config) : base(dbContext)
    {
        _context = dbContext;
        _scoringConfig = _config;
    }


    public async Task<User> GetUserById(string id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> GetUserForCV(string id)
    {
        return await _context.Users
            .Include(u => u.Cv)
                .ThenInclude(cv => cv!.Competences)
            .Include(u => u.Cv)
                .ThenInclude(cv => cv!.Educations)
            .Include(u => u.Cv)
                .ThenInclude(cv => cv!.WorkExperiences)
                .ThenInclude(we => we.WorkPlace)
            .Include(u => u.Cv)
                .ThenInclude(cv => cv!.Educations)
                .ThenInclude(e => e.Institution)
            .Include(u => u.ProjectUsers)
                .ThenInclude(pu => pu.Project)
            .FirstOrDefaultAsync(u => u.Id == id);
    }


    public async Task<List<ScoredResult<User>>> SearchByNameOrUsername(string query, int limit, bool isAuthenticated)

    {
        string normQuery = query.Trim().ToLower();

        var Dbquery = _context.Set<User>().AsQueryable();

        if (!isAuthenticated)
        {
            Dbquery = Dbquery.Where(u => u.IsPrivate != true);
        }

        var candidates = await Dbquery
            .Where(u => u.IsActive == true)
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


    public async Task<List<ScoredResult<User>>> FindRelevantUsersForCv(int cvId, List<string> excludeUserIds, bool isUserLoggedIn, int limit = 5)
    {
        Console.WriteLine($"=== FindRelevantUsersForCv ===");
        Console.WriteLine($"CV ID: {cvId}");
        Console.WriteLine($"Excluding users: {string.Join(", ", excludeUserIds)}");

        // 1. Hämta CV:ets data
        var cvData = await GetCvData(cvId);
        if (cvData == null || !HasRelevantData(cvData))
        {
            Console.WriteLine($"CV has no relevant data or not found");
            return new List<ScoredResult<User>>();
        }

        Console.WriteLine($"CV belongs to user: {cvData.UserId}");
        Console.WriteLine($"CV has - WorkPlaces: {cvData.WorkPlaceIds.Count}, " +
                         $"Institutions: {cvData.InstitutionIds.Count}, " +
                         $"Competences: {cvData.CompetenceNames.Count}");

        // 2. Hämta kandidatanvändare (exkludera alla användare i listan)
        var candidateUsers = await GetCandidateUsers(cvData.UserId, excludeUserIds, limit * 5, isUserLoggedIn);
        Console.WriteLine($"Found {candidateUsers.Count} candidate users");

        // 3. Beräkna poäng för varje användare
        var scoredUsers = candidateUsers
            .Select(user => ScoreUser(user, cvData))
            .Where(scored => scored.Score > 0)
            .OrderByDescending(scored => scored.Score)
            .Take(limit)
            .ToList();

        Console.WriteLine($"Returning {scoredUsers.Count} relevant users");
        return scoredUsers;
    }


    private async Task<CvDataDto?> GetCvData(int cvId)
    {
        // Hämta CV:et med dess relationer
        var cv = await _context.CVs
            .Include(c => c.WorkExperiences).ThenInclude(we => we.WorkPlace)
            .Include(c => c.Educations).ThenInclude(e => e.Institution)
            .Include(c => c.Competences)
            .FirstOrDefaultAsync(c => c.Id == cvId);

        if (cv == null)
            return null;

        // Bearbeta data i minnet istället för i databasen
        return new CvDataDto
        {
            UserId = cv.UserId,
            WorkPlaceIds = cv.WorkExperiences
                .Where(we => we.WorkPlaceId.HasValue)
                .Select(we => we.WorkPlaceId.Value)
                .Distinct()
                .ToList(),
            InstitutionIds = cv.Educations
                .Where(e => e.InstitutionId.HasValue)
                .Select(e => e.InstitutionId.Value)
                .Distinct()
                .ToList(),
            CompetenceNames = cv.Competences
                .Where(co => !string.IsNullOrEmpty(co.Name))
                .Select(co => co.Name.ToLower().Trim())
                .Distinct()
                .ToList()
        };
    }

    private bool HasRelevantData(CvDataDto cvData)
    {
        return cvData.WorkPlaceIds.Any() ||
               cvData.InstitutionIds.Any() ||
               cvData.CompetenceNames.Any();
    }

    private async Task<List<User>> GetCandidateUsers(string cvOwnerUserId, List<string> excludeUserIds, int takeCount, bool isUserLoggedIn)
    {
        // Lägg till CV-ägaren i exkluderingslistan också
        var allExcludedIds = excludeUserIds
            .Append(cvOwnerUserId)
            .Distinct()
            .ToList();

        Console.WriteLine($"Excluding total {allExcludedIds.Count} users: {string.Join(", ", allExcludedIds)}");

        return await _context.Users
            .Include(u => u.Cv)
                .ThenInclude(c => c.WorkExperiences).ThenInclude(we => we.WorkPlace)
            .Include(u => u.Cv)
                .ThenInclude(c => c.Educations).ThenInclude(e => e.Institution)
            .Include(u => u.Cv)
                .ThenInclude(c => c.Competences)
            .Where(u => !allExcludedIds.Contains(u.Id) &&
                        u.IsActive &&
                        u.Cv != null &&
                        // Om användaren inte är inloggad, filtrera bort privata användare
                        (isUserLoggedIn || u.IsPrivate == false))
            .Take(takeCount)
            .ToListAsync();
    }

    private ScoredResult<User> ScoreUser(User user, CvDataDto cvData)
    {
        int score = 0;
        var matchTypes = new List<SearchType>();

        // 1. Poäng för WorkPlaces
        int workPlaceScore = CalculateWorkPlaceScore(user, cvData);
        if (workPlaceScore > 0)
        {
            score += workPlaceScore;
            matchTypes.Add(SearchType.WorkPlace);
        }

        // 2. Poäng för Institutions
        int institutionScore = CalculateInstitutionScore(user, cvData);
        if (institutionScore > 0)
        {
            score += institutionScore;
            matchTypes.Add(SearchType.Institution);
        }

        // 3. Poäng för Competences
        int competenceScore = CalculateCompetenceScore(user, cvData);
        if (competenceScore > 0)
        {
            score += competenceScore;
            matchTypes.Add(SearchType.Competence);
        }

        return new ScoredResult<User>
        {
            Entity = user,
            Score = score,
            MatchedOn = GetPrimaryMatchType(matchTypes)
        };
    }

    private int CalculateWorkPlaceScore(User user, CvDataDto cvData)
    {
        if (!cvData.WorkPlaceIds.Any() || !user.Cv.WorkExperiences.Any())
            return 0;

        var userWorkPlaceIds = user.Cv.WorkExperiences
            .Where(we => we.WorkPlaceId.HasValue)
            .Select(we => we.WorkPlaceId.Value)
            .Distinct()
            .ToList();

        var matches = cvData.WorkPlaceIds
            .Intersect(userWorkPlaceIds)
            .Count();

        return matches * _scoringConfig.WorkPlaceMatchScore;
    }

    private int CalculateInstitutionScore(User user, CvDataDto cvData)
    {
        if (!cvData.InstitutionIds.Any() || !user.Cv.Educations.Any())
            return 0;

        var userInstitutionIds = user.Cv.Educations
            .Where(e => e.InstitutionId.HasValue)
            .Select(e => e.InstitutionId.Value)
            .Distinct()
            .ToList();

        var matches = cvData.InstitutionIds
            .Intersect(userInstitutionIds)
            .Count();

        return matches * _scoringConfig.InstitutionMatchScore;
    }

    private int CalculateCompetenceScore(User user, CvDataDto cvData)
    {
        if (!cvData.CompetenceNames.Any() || !user.Cv.Competences.Any())
            return 0;

        var userCompetenceNames = user.Cv.Competences
            .Where(co => !string.IsNullOrEmpty(co.Name))
            .Select(co => co.Name.ToLower().Trim())
            .Distinct()
            .ToList();

        var matches = cvData.CompetenceNames
            .Intersect(userCompetenceNames)
            .Count();

        return matches * _scoringConfig.CompetenceMatchScore;
    }

    private SearchType GetPrimaryMatchType(List<SearchType> matchTypes)
    {
        // Prioritera WorkPlace > Institution > Competence
        if (matchTypes.Contains(SearchType.WorkPlace))
            return SearchType.WorkPlace;

        if (matchTypes.Contains(SearchType.Institution))
            return SearchType.Institution;

        if (matchTypes.Contains(SearchType.Competence))
            return SearchType.Competence;

        return SearchType.Relevance;
    }

    public async Task<List<User>> SearchUsersAsync(string searchTerm)
    {
        searchTerm = searchTerm.Trim();

        return await _context.Users
            .Where(u => u.IsActive &&
            (
                (u.Email != null && u.Email.Contains(searchTerm)) ||
                (u.UserName != null && u.UserName.Contains(searchTerm)) ||
                (u.Name != null && u.Name.Contains(searchTerm))
             ))
            .OrderBy(u => u.Name ?? u.Email ?? u.UserName)

            .ToListAsync();
    }

    // Helper DTO
    private class CvDataDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<int> WorkPlaceIds { get; set; } = new();
        public List<int> InstitutionIds { get; set; } = new();
        public List<string> CompetenceNames { get; set; } = new();
    }
}
