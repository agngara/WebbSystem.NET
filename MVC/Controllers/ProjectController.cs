using BL.Services;
using DAL;
using DAL.Repositories;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using MVC.Models;
using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;


namespace MVC.Controllers
{
    public class ProjectController : BaseController
    {
        private IProjectRepository _projectRepository;
        private IUserRepository _userRepository;
        private UserManager<User> _userManager;
        public ProjectController(IProjectRepository projectRepository, UserManager<User> userManager, IUserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _userManager = userManager;
            _userRepository = userRepository;
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
        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelStateErrorResponse("Invalid project creation :("));
            }

            //Here follows code regarding the project image.

            string filePath = "Assets/UI-Images/project_placeholder_image.jpg";

            if (model.ImageFile != null)
            {
                bool isValid = ImageUploadService.ValidateFiletype(new string[] { ".jpg", ".jpeg", ".png", ".gif" }, model.ImageFile.FileName);
                
                if (!isValid) 
                { 
                    return Json(ModelStateErrorResponse("Filetype not allowed.")); 
                }

                if (model.ImageFile.Length > 2000000)
                {
                    return Json(ModelStateErrorResponse("Uploaded file is too large."));
                }

                filePath = ImageUploadService.SaveFile(model.ImageFile, "Assets/UserUploads/ProjectImages");

            }



            Project project = new Project
            {
                CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Creator = await _userRepository.GetUserById(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Titel = model.Name,
                Description = model.Description,
                ProjectUsers = new List<ProjectUser>(),
                ImageUrl = filePath

            };

            bool addResult = _projectRepository.AddProject(project);

            if (!addResult)
            {
                return Json(ModelStateErrorResponse("Something went wrong when saving project picture, please try again :("));
            }

            return RedirectToAction("Catalog");
        }
    }
}
