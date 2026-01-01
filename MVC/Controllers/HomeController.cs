using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using DAL;
using Models;
using DAL.Repositories;

namespace MVC;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index()
    {
        CVRepository CVrepo = new CVRepository(_context);
        ProjectRepository ProjectRepo = new ProjectRepository(_context);

        HomepageViewModel viewModel = new HomepageViewModel
        {
            TopCVs = await CVrepo.GetAllExtendedCVs(),
            LatestProjects = await ProjectRepo.GetAllAsync()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Indexold()
    {
        var canConnect = await _context.Database.CanConnectAsync();

        if (canConnect)
        {
            ViewBag.Message = "✅ Databas-anslutning fungerar!";
            ViewBag.DatabaseName = _context.Database.GetDbConnection().Database;
        }
        else
        {
            ViewBag.Message = "❌ Kunde inte ansluta till databasen";
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
