using BL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;

namespace MVC.Controllers;

public class SearchController : Controller
{

    private readonly ISearchService _serachService;

    public SearchController(ISearchService searchService)
    {
        _serachService = searchService;
    }

    [HttpPost]
    public async Task<IActionResult> Search([FromBody] SearchRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var results = await _serachService.Search(request.Query, isAuthenticated);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your search. Please try again later." });
        }
    }
}
