using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Security.Claims;
using DAL;
using System.IO;
using System.Xml.Serialization;
using DAL.Repositories.Interfaces;
using MVC.Models.CVs;
using BL.Interfaces;
using BL.DTOs;


namespace MVC.Controllers
{
    [Authorize]
    public class CVController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ISearchService _searchService;


        private readonly IUserRepository _userRepository;
        private readonly IWorkPlaceRepository _workplaceRepository;
        private readonly ICvRepository _cvRepository;

        public CVController(AppDbContext context, UserManager<User> userManager, IWebHostEnvironment environment, IProjectRepository projectRepository, 
            IUserRepository userRepository, IWorkPlaceRepository workPlaceRepository, ISearchService searchService, ICvRepository cvRepository)

        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _userRepository = userRepository;
            _workplaceRepository = workPlaceRepository;
            _searchService = searchService;
            _cvRepository = cvRepository;

        }

        [AllowAnonymous]
        public async Task<IActionResult> View(string id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Cv)
                        .ThenInclude(cv => cv!.Competences)
                    .Include(u => u.Cv)
                        .ThenInclude(cv => cv!.Educations)
                    .Include(u => u.Cv)
                        .ThenInclude(cv => cv!.WorkExperiences)
                        .ThenInclude(we => we.WorkPlace)
                    .Include(u => u.Cv)
                        .ThenInclude(cv => cv!.Educations)
                        .ThenInclude(e => e.Institution)
                    .Include(u => u.ProjectUsers)
                        .ThenInclude(pu => pu.Project)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound("Användaren hittades inte");
                }

                if (user.IsActive == false)
                {
                    return Unauthorized("This account is deactivated");
                }

                if (user.IsPrivate == true)
                {
                    if (User.Identity.IsAuthenticated == false)
                    {
                        return Unauthorized("Den här profilen är privat. Logga in för att se den.");
                    }
                }

                if (User.Identity!.IsAuthenticated)
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (currentUserId != user.Id && user.Cv != null)
                    {
                        user.Cv.ProfileViews++;
                        await _context.SaveChangesAsync();
                    }
                }

                List<RelevantProfileDto> relevantProfiles = new List<RelevantProfileDto>();

                if (user.Cv != null)
                {
                    try
                    {
                        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        currentUserId = currentUserId ?? "";

                        relevantProfiles = await _searchService.SearchRelevantProfiles(
                            currentUserId: currentUserId,
                            profileUserId: id
                        );

                        relevantProfiles = relevantProfiles
                            .Where(p => p.UserId != id && p.UserId != currentUserId)
                            .ToList();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting relevant profiles: {ex.Message}");
                    }
                }

                ViewBag.RelevantProfiles = relevantProfiles;
                ViewBag.HasRelevantProfiles = relevantProfiles.Any();

                return View("~/Views/CvView/View.cshtml", user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while loading the CV. Please try again later." });
            }
        }

        public async Task<IActionResult> MyCV()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Users
                    .Include(u => u.Cv)
                        .ThenInclude(cv => cv!.Competences)
                    .Include(u => u.Cv)
                        .ThenInclude(cv => cv!.Educations).ThenInclude(e => e.Institution)
                    .Include(u => u.Cv)
                        .ThenInclude(cv => cv!.WorkExperiences).ThenInclude(W => W.WorkPlace)
                    .Include(u => u.ProjectUsers)
                        .ThenInclude(pu => pu.Project)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound();
                }

                var allProjects = await _context.Projects.ToListAsync();

                var viewModel = new CvEditModel
                {
                    Id = user.Cv?.Id,
                    Description = user.Cv?.Description ?? "",
                    ExistingProfileImage = user.Cv?.ProfileImage,
                    Competences = user.Cv?.Competences.Select(c => new CompetenceViewModel
                    {
                        Id = c.Id,
                        Name = c.Name
                    }).ToList() ?? new(),
                    Educations = user.Cv?.Educations.Select(e => new EducationViewModel
                    {
                        Id = e.Id,
                        ProgramOrCourseName = e.ProgramOrCourseName ?? "",
                        InstitutionName = e.Institution?.Name,
                        Description = e.Description,
                        StartDate = e.StartDate,
                        EndDate = e.EndDate
                    }).ToList() ?? new(),
                    WorkExperiences = user.Cv?.WorkExperiences.Select(w => new WorkExperienceViewModel
                    {
                        Id = w.Id,
                        Title = w.Title,
                        CompanyName = w.WorkPlace?.Name,
                        Description = w.Description,
                        StartDate = w.StartDate,
                        EndDate = w.EndDate
                    }).ToList() ?? new(),
                    AvailableProjects = allProjects.Select(p => new ProjectOptionViewModel
                    {
                        Id = p.Id,
                        Title = p.Titel,
                        IsSelected = user.ProjectUsers.Any(pu => pu.ProjectId == p.Id)
                    }).ToList()
                };

                ViewBag.ProfileViews = user.Cv?.ProfileViews ?? 0;
                return View("~/Views/CvView/Edit.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while loading your CV. Please try again later." });
            }
        }


        public async Task<IActionResult> Export(int cvId)
        {
            try
            {
                CV? cv = _context.CVs
                                .Include(c => c.Competences)
                                .Include(c => c.WorkExperiences)
                                .ThenInclude(w => w.WorkPlace)
                                .Include(c => c.Educations)
                                .Include(c => c.User)
                                .FirstOrDefault(c => c.Id == cvId);

                User? user = await _userRepository.GetUserById(cv.UserId);

                if (user == null || cv == null)
                {
                    return BadRequest();
                }

                List<WorkExperienceFileModel> viewWorkExperiences = new List<WorkExperienceFileModel>();

                foreach (WorkExperience w in cv.WorkExperiences)
                {
                    WorkPlace? workplace = w.WorkPlace;
                    if (workplace == null)
                    {
                        break;
                    }

                    WorkExperienceFileModel workExperienceFileModel = new WorkExperienceFileModel()
                    {
                        Title = w.Title,
                        Description = w.Description,
                        StartDate = w.StartDate,
                        EndDate = w.EndDate,
                        WorkPlaceName = workplace.Name,
                    };

                    viewWorkExperiences.Add(workExperienceFileModel);
                }

                CvFileModel fileModel = new CvFileModel()
                {
                    Name = user.Name,
                    Description = cv.Description,
                    WorkExperiences = viewWorkExperiences,
                    Competences = cv.Competences,
                    Educations = cv.Educations,
                };

                XmlSerializer serializer = new XmlSerializer(typeof(CvFileModel));

                using var memoryStream = new MemoryStream();

                serializer.Serialize(memoryStream, fileModel);

                memoryStream.Position = 0;

                return File(memoryStream.ToArray(), "application/xml", $"{Guid.NewGuid()}.xml");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while exporting the CV. Please try again later." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCV(CvEditModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var allProjects = await _context.Projects.ToListAsync();
                    model.AvailableProjects = allProjects.Select(p => new ProjectOptionViewModel
                    {
                        Id = p.Id,
                        Title = p.Titel,
                        IsSelected = model.SelectedProjects?.Any(sp => sp.ProjectId == p.Id && sp.IsSelected) == true
                    }).ToList();

                    ViewBag.ProfileViews = 0;
                    return View("~/Views/CvView/Edit.cshtml", model);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Users
                    .Include(u => u.Cv)
                        .ThenInclude(cv => cv!.Competences)
                    .Include(u => u.Cv)
                        .ThenInclude(cv => cv!.Educations)
                    .Include(u => u.Cv)
                        .ThenInclude(cv => cv!.WorkExperiences)
                    .Include(u => u.ProjectUsers)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound();
                }

                if (user.Cv == null)
                {
                    user.Cv = new CV
                    {
                        UserId = userId,
                        User = user,
                        Description = model.Description,
                        ProfileImage = string.IsNullOrEmpty(model.ProfileImageBase64)
                            ? "/pfp/standard.png"
                            : await SaveBase64ImageAsync(model.ProfileImageBase64)
                    };
                    _context.CVs.Add(user.Cv);
                }
                else
                {
                    user.Cv.Description = model.Description;

                    if (!string.IsNullOrEmpty(model.ProfileImageBase64))
                    {
                        user.Cv.ProfileImage = await SaveBase64ImageAsync(model.ProfileImageBase64);
                    }
                }

                user.Cv.Competences.Clear();
                foreach (var comp in model.Competences.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
                {
                    user.Cv.Competences.Add(new Competence
                    {
                        Name = comp.Name.Trim(),
                        CvId = user.Cv.Id,
                        Cv = user.Cv
                    });
                }

                user.Cv.Educations.Clear();
                foreach (var edu in model.Educations.Where(e => !string.IsNullOrWhiteSpace(e.ProgramOrCourseName)))
                {
                    Institution? institution = null;
                    if (!string.IsNullOrWhiteSpace(edu.InstitutionName))
                    {
                        institution = await _context.Institutions
                            .FirstOrDefaultAsync(i => i.Name.ToLower() == edu.InstitutionName.Trim().ToLower());

                        if (institution == null)
                        {
                            institution = new Institution { Name = edu.InstitutionName.Trim() };
                            _context.Institutions.Add(institution);
                        }
                    }

                    user.Cv.Educations.Add(new Education
                    {
                        ProgramOrCourseName = edu.ProgramOrCourseName.Trim(),
                        Institution = institution,
                        Description = edu.Description?.Trim(),
                        StartDate = edu.StartDate,
                        EndDate = edu.EndDate,
                        CvId = user.Cv.Id,
                        Cv = user.Cv
                    });
                }

                user.Cv.WorkExperiences.Clear();
                foreach (var work in model.WorkExperiences.Where(w => !string.IsNullOrWhiteSpace(w.Title)))
                {
                    WorkPlace? workPlace = null;
                    if (!string.IsNullOrWhiteSpace(work.CompanyName))
                    {
                        workPlace = await _context.WorkPlaces
                            .FirstOrDefaultAsync(wp => wp.Name.ToLower() == work.CompanyName.Trim().ToLower());

                        if (workPlace == null)
                        {
                            workPlace = new WorkPlace { Name = work.CompanyName.Trim() };
                            _context.WorkPlaces.Add(workPlace);
                        }
                    }

                    user.Cv.WorkExperiences.Add(new WorkExperience
                    {
                        Title = work.Title.Trim(),
                        WorkPlace = workPlace,
                        Description = work.Description?.Trim(),
                        StartDate = work.StartDate,
                        EndDate = work.EndDate,
                        CvId = user.Cv.Id,
                        Cv = user.Cv
                    });
                }

                user.ProjectUsers.Clear();
                if (model.SelectedProjects != null)
                {
                    foreach (var selectedProject in model.SelectedProjects.Where(sp => sp.IsSelected))
                    {
                        user.ProjectUsers.Add(new ProjectUser
                        {
                            UserId = userId,
                            ProjectId = selectedProject.ProjectId,
                            User = user,
                            Project = await _context.Projects.FindAsync(selectedProject.ProjectId)
                        });
                    }
                }
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "CV har uppdaterats!";
                return RedirectToAction("MyCV");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating the CV. Please try again later." });
            }
        }

        private async Task<string> SaveBase64ImageAsync(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return "/pfp/standard.png";

            try
            {
                var base64Data = base64String.Contains(',')
                    ? base64String.Split(',')[1]
                    : base64String;

                var bytes = Convert.FromBase64String(base64Data);

                string fileExtension = GetImageExtension(bytes);

                var fileName = $"{Guid.NewGuid()}{fileExtension}";

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pfp", fileName);

                var directory = Path.GetDirectoryName(filePath);
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory!);
                }

                await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                return $"/pfp/{fileName}";
            }
            catch
            {
                return "/pfp/standard.png";
            }
        }

        private string GetImageExtension(byte[] bytes)
        {
            try
            {
                if (bytes.Length > 2 && bytes[0] == 0xFF && bytes[1] == 0xD8)
                    return ".jpg";
                if (bytes.Length > 3 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                    return ".png";
                if (bytes.Length > 4 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
                    return ".gif";

                return ".jpg";
            }
            catch
            {
                return ".jpg";
            }
        }

        public async Task<IActionResult> EditProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                var viewModel = new ProfileViewModel
                {
                    Name = user.Name,
                    Address = user.Address,
                    IsPrivate = user.IsPrivate ?? false,
                    Email = user.Email
                };

                return View("~/Views/CvView/EditProfile.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while loading profile settings. Please try again later." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("~/Views/CvView/EditProfile.cshtml", model);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                user.Name = model.Name;
                user.Address = model.Address;
                user.IsPrivate = model.IsPrivate;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Profil har uppdaterats!";
                    return RedirectToAction("EditProfile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View("~/Views/CvView/EditProfile.cshtml", model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating profile. Please try again later." });
            }
        }

        [HttpPost]
        public IActionResult AddCompetenceRow([FromBody] List<CompetenceViewModel> currentCompetences)
        {
            try
            {
                currentCompetences.Add(new CompetenceViewModel());
                return PartialView("_CompetenceRow", currentCompetences.Last());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while adding competence row." });
            }
        }

        [HttpPost]
        public IActionResult AddEducationRow([FromBody] List<EducationViewModel> currentEducations)
        {
            try
            {
                currentEducations.Add(new EducationViewModel());
                return PartialView("_EducationRow", currentEducations.Last());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while adding education row." });
            }
        }

        [HttpPost]
        public IActionResult AddWorkExperienceRow([FromBody] List<WorkExperienceViewModel> currentWorkExperiences)
        {
            try
            {
                currentWorkExperiences.Add(new WorkExperienceViewModel());
                return PartialView("_WorkExperienceRow", currentWorkExperiences.Last());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while adding work experience row." });
            }
        }
    }
}