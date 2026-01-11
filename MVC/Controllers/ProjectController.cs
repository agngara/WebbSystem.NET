using BL.Services;
using DAL;
using DAL.Repositories;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Models;
using MVC.Models.Projects;
using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;


namespace MVC.Controllers
{
    public class ProjectController : BaseController
    {
        private IProjectRepository _projectRepository;
        private IUserRepository _userRepository;
        private IProjectUserRepository _projectUserRepository;
        private UserManager<User> _userManager;

        public ProjectController(IProjectRepository projectRepository, IProjectUserRepository projectUserRepository, UserManager<User> userManager, IUserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _projectUserRepository = projectUserRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }
        public async Task<IActionResult> Catalog()
        {
            try
            {

                var user = await _userManager.GetUserAsync(User);
                string userId;
                List<Project> userProjects;
                if (user != null)
                {
                    userId = user.Id;
                    userProjects = await _projectRepository.GetProjectsByCreator(userId);

                }
                else
                {
                    userProjects = new List<Project>();
                }

                List<Project> allProjects = await _projectRepository.GetAllAsync();

                CatalogViewModel catalogViewModel = new CatalogViewModel
                {
                    AllProjects = allProjects,
                    UserProjects = userProjects,

                };

                return View(catalogViewModel);
            }
            catch (Exception ex) 
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return StatusCode(500);
            }

        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            try 
            {
                if (!ModelState.IsValid)
                {
                    return Json(ModelStateErrorResponse("Invalid project creation :("));
                }

                //Here follows code regarding the project image.

                string filePath = "/Assets/UI-Images/project_placeholder_image.jpg";

                if (model.ImageFile != null)
                {
                    bool isValid = FileService.ValidateFiletype(new string[] { ".jpg", ".jpeg", ".png", ".gif" }, model.ImageFile.FileName);

                    if (!isValid)
                    {
                        return Json(ModelStateErrorResponse("Filetype not allowed."));
                    }

                    if (model.ImageFile.Length > 2000000)
                    {
                        return Json(ModelStateErrorResponse("Uploaded file is too large."));
                    }

                    filePath = FileService.SaveFile(model.ImageFile, "Assets/UserUploads/ProjectImages");

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
                    return Json(ModelStateErrorResponse("Something went wrong when saving project image, please try again :("));
                }

                return RedirectToAction("Catalog");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {

                Project project = await _projectRepository.GetProjectAsync(id);

                if (project == null)
                {
                    return Json(ModelStateErrorResponse("No project found, Yo :("));
                }


                if (project.CreatorId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                {

                    EditProjectViewModel viewModel = new EditProjectViewModel()
                    {
                        ProjectId = project.Id,
                        Name = project.Titel,
                        Description = project.Description,
                        ImageUrl = project.ImageUrl,
                        ImageFile = null
                    };
                    return View(viewModel);
                }
                else
                {
                    return StatusCode(401);
                }
            }
            catch (Exception ex) 
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return StatusCode(500);
            }
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditProjectViewModel viewModel)
        {
                try
                {

                    Project dbProject = await _projectRepository.GetProjectAsync(viewModel.ProjectId);
                    string userId = dbProject.CreatorId;

                    // Checks if logged in user has the same ID as the one listed as creator in DB.
                    if (userId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                    {

                        if (!ModelState.IsValid)
                        {
                            return Json(ModelStateErrorResponse("Invalid changes, computer says no :("));
                        }
                        Project p = await _projectRepository.GetProjectAsync(id);

                        string? imgUrl = viewModel.ImageUrl;

                        if (viewModel.ImageFile != null)
                        {
                            bool isValid = FileService.ValidateFiletype(new string[] { ".jpg", ".jpeg", ".png", ".gif" }, viewModel.ImageFile.FileName);

                            if (!isValid)
                            {
                                return Json(ModelStateErrorResponse("Filetype not allowed."));
                            }

                            // Checks filesize in bytes.
                            if (viewModel.ImageFile.Length > 4000000)
                            {
                                return Json(ModelStateErrorResponse("Uploaded file is too large."));
                            }

                            imgUrl = FileService.SaveFile(viewModel.ImageFile, "Assets/UserUploads/ProjectImages");

                        }
                        else
                        {
                            imgUrl = p.ImageUrl;
                        }

                        p.Id = id;
                        p.CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        p.Creator = await _userRepository.GetUserById(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        p.Titel = viewModel.Name;
                        p.Description = viewModel.Description;
                        p.ImageUrl = imgUrl;


                        _projectRepository.UpdateProject(p);
                        return RedirectToAction("Catalog");
                    }
                    // Or else, a forbidden status code is sent. 
                    else
                    {
                        return StatusCode(401);
                    }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return StatusCode(500);
            }
            
        }
        
        public IActionResult Delete(int id)
        {
            try
            {
                _projectRepository.DeleteProject(id);

                return RedirectToAction("Catalog");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return StatusCode(500);
            }
        }

        public IActionResult Join(int projectId)
        {
            try
            {
                if (User.Identity?.IsAuthenticated == false)
                {
                    return RedirectToAction("Login", "Account");
                }

                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!_projectUserRepository.AddUserToProject(userId, projectId))
                {
                    return Json(ModelStateErrorResponse("Could not join project, because there is already a user assigned to the project with id: " + projectId));
                }
                else
                {
                    var referer = Request.Headers["Referer"].ToString();

                    if (!string.IsNullOrEmpty(referer))
                    {
                        return Redirect(referer);
                    }

                    return RedirectToAction("Catalog");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return StatusCode(500);
            }
        }
        public IActionResult Leave(int projectId)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!_projectUserRepository.RemoveUserFromProject(userId, projectId))
                {
                    return Json(ModelStateErrorResponse("Could not leave project, because there were no user with Id: " + userId + " to remove."));
                }
                else
                {
                    var referer = Request.Headers["Referer"].ToString();

                    if (!string.IsNullOrEmpty(referer))
                    {
                        return Redirect(referer);
                    }
                    return RedirectToAction("Catalog");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return StatusCode(500);
            }
        }

        public async Task<IActionResult> Details(int projectId)
        {
            try
            {
               
                List<User> filteredUsers = new List<User>();

                List<User> rawUsers = _projectUserRepository.GetAllUsersByProject(projectId);

                // This statement filters out private users if the browsing user is anonymous. 
                if (User.Identity?.IsAuthenticated == false)
                {

                   filteredUsers  = rawUsers.Where(u => u.IsPrivate == false).ToList();
                }
                else
                {
                    filteredUsers = rawUsers;
                }

                    Project project = await _projectRepository.GetProjectAsync(projectId);

                User creator = await _userRepository.GetUserById(project.CreatorId);

                ProjectViewModel viewModel = new ProjectViewModel
                {
                    ProjectId = project.Id,
                    CreatorId = project.CreatorId,
                    Creator = creator,
                    Titel = project.Titel,
                    Description = project.Description,
                    ImageUrl = project.ImageUrl,
                    Users = filteredUsers
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return StatusCode(500);
            }
        }
    }
}
