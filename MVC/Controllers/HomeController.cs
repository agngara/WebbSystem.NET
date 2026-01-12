using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using DAL;
using Models;
using DAL.Repositories.Interfaces;

namespace MVC;

public class HomeController : Controller
{
    private ICvRepository CVrepo;
    private IProjectRepository ProjectRepo;

    public HomeController(ICvRepository cvRepository, IProjectRepository projectRepository)
    {
        CVrepo = cvRepository;
        ProjectRepo = projectRepository;
    }
    public async Task<IActionResult> Index()
    {

        List<Project> projects = await ProjectRepo.GetAmount(20);
        projects.Reverse();

        HomepageViewModel viewModel = new HomepageViewModel
        {
            TopCVs = await CVrepo.GetTop20CVs(),
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
