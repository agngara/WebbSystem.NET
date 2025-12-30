using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Security.Claims;
using DAL;

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

            return View(user);
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
                    Title = p.Titel, // Ändra från p.Title till p.Titel
                    IsSelected = model.SelectedProjects?.Any(sp => sp.ProjectId == p.Id && sp.IsSelected) == true
                }).ToList();

                ViewBag.ProfileViews = 0; // Temporärt värde
                return View("Edit", model);
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
                user.Cv = new Models.CV // Lägg till Models. framför
                {
                    UserId = userId,
                    Description = model.Description
                };
                _context.CVs.Add(user.Cv);
            }
            else
            {
                user.Cv.Description = model.Description;
            }

            // Hantera profilbild
            if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profile-images");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImageFile.CopyToAsync(stream);
                }

                user.Cv.ProfileImage = $"/uploads/profile-images/{uniqueFileName}";
            }

            // Uppdatera kompetenser
            user.Cv.Competences.Clear();
            foreach (var comp in model.Competences.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
            {
                user.Cv.Competences.Add(new Models.Competence // Lägg till Models. framför
                {
                    Name = comp.Name,
                    Cv = user.Cv
                });
            }

            // Uppdatera utbildningar
            user.Cv.Educations.Clear();
            foreach (var edu in model.Educations.Where(e => !string.IsNullOrWhiteSpace(e.ProgramOrCourseName)))
            {
                var institution = await _context.Institutions
                    .FirstOrDefaultAsync(i => i.Name == edu.InstitutionName)
                    ?? new Models.Institution { Name = edu.InstitutionName }; // Lägg till Models. framför

                user.Cv.Educations.Add(new Models.Education // Lägg till Models. framför
                {
                    ProgramOrCourseName = edu.ProgramOrCourseName,
                    Institution = institution,
                    Description = edu.Description,
                    StartDate = edu.StartDate,
                    EndDate = edu.EndDate,
                    Cv = user.Cv
                });
            }

            // Uppdatera arbetserfarenheter
            user.Cv.WorkExperiences.Clear();
            foreach (var work in model.WorkExperiences.Where(w => !string.IsNullOrWhiteSpace(w.Title)))
            {
                var workPlace = await _context.WorkPlaces
                    .FirstOrDefaultAsync(wp => wp.Name == work.CompanyName)
                    ?? new Models.WorkPlace { Name = work.CompanyName }; // Lägg till Models. framför

                user.Cv.WorkExperiences.Add(new Models.WorkExperience // Lägg till Models. framför
                {
                    Title = work.Title,
                    WorkPlace = workPlace,
                    Description = work.Description,
                    StartDate = work.StartDate,
                    EndDate = work.EndDate,
                    Cv = user.Cv
                });
            }

            // Uppdatera valda projekt
            user.ProjectUsers.Clear();
            if (model.SelectedProjects != null)
            {
                foreach (var selectedProject in model.SelectedProjects.Where(sp => sp.IsSelected))
                {
                    user.ProjectUsers.Add(new Models.ProjectUser // Lägg till Models. framför
                    {
                        UserId = userId,
                        ProjectId = selectedProject.ProjectId
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "CV har uppdaterats!";
            return RedirectToAction("MyCV");
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

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditProfile", model);
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

            return View("EditProfile", model);
        }

        // Hjälpmetod för att lägga till tomma rader i formuläret
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