namespace DAL.Repositories.Models;

public class SearchScoringConfig
{
    public int ExactMatchScore { get; set; } = 10;
    public int StartsWithScore { get; set; } = 5;
    public int ContainsScore { get; set; } = 3;

    //Cv related scoring
    public int WorkPlaceMatchScore { get; set; } = 30;
    public int InstitutionMatchScore { get; set; } = 25;
    public int CompetenceMatchScore { get; set; } = 20;
}
