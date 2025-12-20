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

        var results = await _serachService.Search(request.Query);
        return Ok(results);
    }
}
