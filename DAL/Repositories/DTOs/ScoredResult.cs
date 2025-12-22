
using DAL.Repositories.Models;

namespace DAL.Repositories.DTOs;

public class ScoredResult<T>
{
    public required T Entity { get; set; }
    public int Score { get; set; }
    public SearchType MatchedOn { get; set; }
}
