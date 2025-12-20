
using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public class SearchRequest
{
    [Required]
    public string Query { get; set; } = string.Empty;
}
