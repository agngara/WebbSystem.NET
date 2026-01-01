using DAL;
using DAL.Repositories;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using MVC.Models;

namespace MVC.Controllers
{
    public class ProjectController : Controller
    {
        private IProjectRepository _projectRepository;
        private UserManager<User> _userManager;
        public ProjectController(IProjectRepository projectRepository, UserManager<User> userManager)
        {
            _projectRepository = projectRepository;
            _userManager = userManager;
        }
        public async Task<IActionResult> Catalog()
        {
            var user = await _userManager.GetUserAsync(User);
            string userId;
            List<Project> userProjects;
            if (user != null)
            {
                userId = user.Id;
                userProjects = await _projectRepository.GetUserProjectsAsync(userId);

            }
            else
            {
                userProjects = new List<Project>();
            }

            List<Project> allProjects = await _projectRepository.GetAllAsync();

            CatalogViewModel catalogViewModel = new CatalogViewModel
            {
                allProjects = allProjects,
                userProjects = userProjects
            };

            return View(catalogViewModel);

        }
    }
}
