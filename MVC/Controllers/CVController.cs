using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Security.Claims;
using DAL;
using System.IO;

namespace MVC.Controllers
{
    [Authorize]
    public class CVController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public CVController(AppDbContext context, UserManager<User> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // Visa CV för en specifik användare
        [AllowAnonymous]
        public async Task<IActionResult> View(string id)
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

            // Kontrollera om profilen är privat
            if (user.IsPrivate == true)
            {
                if (!User.Identity!.IsAuthenticated)
                {
                    return Unauthorized("Den här profilen är privat. Logga in för att se den.");
                }

                // Kontrollera om den aktuella användaren är samma som profilen
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId != user.Id)
                {
                    return Forbid("Du har inte behörighet att se denna privata profil");
                }
            }

            // Öka profilvisningar om det är en annan användare
            if (User.Identity!.IsAuthenticated)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId != user.Id && user.Cv != null)
                {
                    user.Cv.ProfileViews++;
                    await _context.SaveChangesAsync();
                }
            }

            return View("~/Views/CvView/View.cshtml", user);
        }

        // Visa och redigera eget CV
        public async Task<IActionResult> MyCV()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users
                .Include(u => u.Cv)
                    .ThenInclude(cv => cv!.Competences)
                .Include(u => u.Cv)
                    .ThenInclude(cv => cv!.Educations)
                .Include(u => u.Cv)
                    .ThenInclude(cv => cv!.WorkExperiences)
                .Include(u => u.ProjectUsers)
                    .ThenInclude(pu => pu.Project)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            // Hämta alla tillgängliga projekt
            var allProjects = await _context.Projects.ToListAsync();

            var viewModel = new CvEditModel
            {
                Id = user.Cv?.Id,
                Description = user.Cv?.Description ?? "",
                ExistingProfileImage = user.Cv?.ProfileImage,
                ProfileImageUrl = user.Cv?.ProfileImage?.StartsWith("http") == true ? user.Cv.ProfileImage : null,
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

        // Uppdatera CV
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCV(CvEditModel model)
        {
            if (!ModelState.IsValid)
            {
                // Hämta projekt igen om validering misslyckas
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

            // Skapa eller uppdatera CV
            if (user.Cv == null)
            {
                user.Cv = new CV
                {
                    UserId = userId,
                    User = user,
                    Description = model.Description
                };
                _context.CVs.Add(user.Cv);
            }
            else
            {
                user.Cv.Description = model.Description;
            }

            // Hantera profilbild (Prioritet: URL > Base64 > Behåll befintlig)
            if (!string.IsNullOrEmpty(model.ProfileImageUrl))
            {
                // Använd URL
                user.Cv.ProfileImage = model.ProfileImageUrl;
            }
            else if (!string.IsNullOrEmpty(model.ProfileImageBase64))
            {
                // Spara base64 som fil på servern
                var fileName = await SaveBase64Image(model.ProfileImageBase64);
                if (!string.IsNullOrEmpty(fileName))
                {
                    user.Cv.ProfileImage = $"/uploads/profile-images/{fileName}";
                }
            }
            // Om ingen ny bild anges, behåll den befintliga

            // Uppdatera kompetenser
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

            // Uppdatera utbildningar
            user.Cv.Educations.Clear();
            foreach (var edu in model.Educations.Where(e => !string.IsNullOrWhiteSpace(e.ProgramOrCourseName)))
            {
                // Hitta eller skapa institution
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

            // Uppdatera arbetserfarenheter
            user.Cv.WorkExperiences.Clear();
            foreach (var work in model.WorkExperiences.Where(w => !string.IsNullOrWhiteSpace(w.Title)))
            {
                // Hitta eller skapa arbetsplats
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

            // Uppdatera valda projekt
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

        // Hjälpmetod för att spara base64 bild som fil
        private async Task<string?> SaveBase64Image(string base64Image)
        {
            if (string.IsNullOrEmpty(base64Image) || !base64Image.Contains(","))
                return null;

            try
            {
                // Ta bort "data:image/png;base64," delen
                var base64Data = base64Image.Substring(base64Image.IndexOf(",") + 1);

                // Kontrollera om det är giltig base64
                if (base64Data.Length % 4 != 0)
                    return null;

                byte[] imageBytes = Convert.FromBase64String(base64Data);

                // Kontrollera filstorlek (max 2MB)
                if (imageBytes.Length > 2 * 1024 * 1024)
                    return null;

                // Skapa uploads-mapp om den inte finns
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profile-images");
                Directory.CreateDirectory(uploadsFolder);

                // Bestäm filtyp från base64 header
                string fileExtension = ".png"; // default
                if (base64Image.Contains("image/jpeg"))
                    fileExtension = ".jpg";
                else if (base64Image.Contains("image/gif"))
                    fileExtension = ".gif";

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                return fileName;
            }
            catch
            {
                return null;
            }
        }

        // Redigera profilinformation
        public async Task<IActionResult> EditProfile()
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
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

        // Hjälpmetod för att lägga till tomma rader i formuläret (AJAX)
        [HttpPost]
        public IActionResult AddCompetenceRow([FromBody] List<CompetenceViewModel> currentCompetences)
        {
            currentCompetences.Add(new CompetenceViewModel());
            return PartialView("_CompetenceRow", currentCompetences.Last());
        }

        [HttpPost]
        public IActionResult AddEducationRow([FromBody] List<EducationViewModel> currentEducations)
        {
            currentEducations.Add(new EducationViewModel());
            return PartialView("_EducationRow", currentEducations.Last());
        }

        [HttpPost]
        public IActionResult AddWorkExperienceRow([FromBody] List<WorkExperienceViewModel> currentWorkExperiences)
        {
            currentWorkExperiences.Add(new WorkExperienceViewModel());
            return PartialView("_WorkExperienceRow", currentWorkExperiences.Last());
        }
    }
}