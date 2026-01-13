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
        try
        {
            CVRepository CVrepo = new CVRepository(_context);
            ProjectRepository ProjectRepo = new ProjectRepository(_context);

            List<Project> projects = await ProjectRepo.GetAmount(20);
            projects.Reverse();

            foreach (Project project in projects)
            {
                if (project.Creator.IsActive == false)
                {
                    project.Creator = new User
                    {
                        Name = "Deactivated User"
                    };
                }
            }

            HomepageViewModel viewModel = new HomepageViewModel
            {
                TopCVs = await CVrepo.GetTop20PublicCVs(),
                LatestProjects = projects
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("An error occurd when fetching the homepage: " + ex);
            return StatusCode(500);
        }
    }

    public IActionResult Privacy()
    {
        try
        {
            return View();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("An error occured when fetching the privacy page: " + ex);
            return StatusCode(500);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        try
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("An error occured when displaying the error, good luck with that one. : " + ex);
            return StatusCode(500); 
        }
    }
}
