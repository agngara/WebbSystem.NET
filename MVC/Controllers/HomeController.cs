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

        List<Project> projects = await ProjectRepo.GetAmount(20);
        projects.Reverse();

        HomepageViewModel viewModel = new HomepageViewModel
        {
            TopCVs = await CVrepo.GetTop20PublicCVs(),
            LatestProjects = projects
        };

        return View(viewModel);
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
