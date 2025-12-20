namespace DAL.Repositories.Models;

public class SearchScoringConfig
{
    public int ExactMatchScore { get; set; } = 10;
    public int StartsWithScore { get; set; } = 5;
    public int ContainsScore { get; set; } = 3;
}
