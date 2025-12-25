using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Text;
using System.Text.Json;

namespace MVC.Controllers
{
    [Authorize]
    public class CvController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CvController> _logger;

        public CvController(AppDbContext context, UserManager<User> userManager,
                           IWebHostEnvironment environment, ILogger<CvController> logger)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IActionResult> MyCv()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                _logger.LogInformation($"Hämtar CV för användare: {user.Id}");

                var cv = await _context.CVs
                    .AsNoTracking()
                    .Where(c => c.UserId == user.Id)
                    .Select(c => new
                    {
                        c.Id,
                        c.UserId,
                        c.ProfileImage
                    })
                    .FirstOrDefaultAsync();

                if (cv == null)
                {
                    _logger.LogInformation("CV finns inte, skapar nytt...");

                    var newCv = new CV
                    {
                        UserId = user.Id,
                        User = user,
                        ProfileImage = "/images/standard.png"
                    };

                    _context.CVs.Add(newCv);
                    await _context.SaveChangesAsync();

                    return await LoadFullCvAsync(newCv.Id, user);
                }

                return await LoadFullCvAsync(cv.Id, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel i MyCv");

                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var fallbackCv = new CV
                    {
                        Id = 0,
                        UserId = user.Id,
                        User = user,
                        ProfileImage = "/images/standard.png",
                        WorkExperiences = new List<WorkExperience>(),
                        Educations = new List<Education>(),
                        Competences = new List<Competence>()
                    };

                    ViewData["Warning"] = "Databasfel. CV-data kanske inte sparas.";
                    return View(fallbackCv);
                }

                return RedirectToAction("Index", "Home");
            }
        }

        private async Task<IActionResult> LoadFullCvAsync(int cvId, User user)
        {
            var cv = await _context.CVs
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == cvId);

            if (cv == null) return NotFound();

            try
            {
                cv.WorkExperiences = await _context.WorkExperiences
                    .Where(we => we.CvId == cvId)
                    .Include(we => we.WorkPlace)
                    .ToListAsync();

                cv.Educations = await _context.Educations
                    .Where(e => e.CvId == cvId)
                    .Include(e => e.Institution)
                    .ToListAsync();

                cv.Competences = await _context.Competences
                    .Where(c => c.CvId == cvId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kunde inte ladda alla relationer");
                cv.WorkExperiences ??= new List<WorkExperience>();
                cv.Educations ??= new List<Education>();
                cv.Competences ??= new List<Competence>();
            }

            return View(cv);
        }

        [AllowAnonymous]
        public async Task<IActionResult> PublicCv(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.IsPrivate == true)
                    return NotFound();

                var cvId = await _context.CVs
                    .Where(c => c.UserId == userId)
                    .Select(c => (int?)c.Id)
                    .FirstOrDefaultAsync();

                if (cvId == null)
                    return NotFound();

                var cv = await _context.CVs
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == cvId);

                if (cv == null) return NotFound();

                cv.WorkExperiences = await _context.WorkExperiences
                    .Where(we => we.CvId == cvId)
                    .Include(we => we.WorkPlace)
                    .ToListAsync();

                cv.Educations = await _context.Educations
                    .Where(e => e.CvId == cvId)
                    .Include(e => e.Institution)
                    .ToListAsync();

                cv.Competences = await _context.Competences
                    .Where(c => c.CvId == cvId)
                    .ToListAsync();

                return View(cv);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            var cvId = await _context.CVs
                .Where(c => c.UserId == user.Id)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (cvId == 0)
            {
                var newCv = new CV
                {
                    UserId = user.Id,
                    User = user,
                    ProfileImage = "/images/standard.png"
                };

                _context.CVs.Add(newCv);
                await _context.SaveChangesAsync();
                cvId = newCv.Id;
            }

            var cv = await _context.CVs
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == cvId);

            cv.WorkExperiences = await _context.WorkExperiences
                .Where(we => we.CvId == cvId)
                .Include(we => we.WorkPlace)
                .ToListAsync();

            cv.Educations = await _context.Educations
                .Where(e => e.CvId == cvId)
                .Include(e => e.Institution)
                .ToListAsync();

            cv.Competences = await _context.Competences
                .Where(c => c.CvId == cvId)
                .ToListAsync();

            return View(cv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CV model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var existingCv = await _context.CVs
                    .Include(c => c.WorkExperiences)
                    .Include(c => c.Educations)
                    .Include(c => c.Competences)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (existingCv == null)
                {
                    existingCv = new CV
                    {
                        UserId = user.Id,
                        User = user
                    };
                    _context.CVs.Add(existingCv);
                }

                await UpdateWorkExperiencesAsync(existingCv, model.WorkExperiences);
                await UpdateEducationsAsync(existingCv, model.Educations);
                UpdateCompetences(existingCv, model.Competences);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "CV sparades!";
                return RedirectToAction(nameof(MyCv));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid CV-redigering");
                ModelState.AddModelError("", "Ett fel uppstod vid sparande.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var cv = await _context.CVs
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (cv == null)
                {
                    cv = new CV
                    {
                        UserId = user.Id,
                        User = user
                    };
                    _context.CVs.Add(cv);
                }

                if (profileImage == null || profileImage.Length == 0)
                    return BadRequest("Ingen bild vald");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest("Endast .jpg, .jpeg och .png filer är tillåtna");

                if (profileImage.Length > 5 * 1024 * 1024)
                    return BadRequest("Filen är för stor. Max 5MB");

                var fileName = $"profile_{user.Id}_{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                cv.ProfileImage = $"/uploads/profiles/{fileName}";
                cv.ProfileImageFileName = fileName;
                cv.ProfileImageContentType = profileImage.ContentType;

                if (!string.IsNullOrEmpty(cv.ProfileImage) && cv.ProfileImage.StartsWith("/uploads/profiles/"))
                {
                    var oldFileName = Path.GetFileName(cv.ProfileImage);
                    var oldFilePath = Path.Combine(uploadsFolder, oldFileName);
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    imageUrl = cv.ProfileImage,
                    message = "Profilbild uppdaterad!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid bilduppladdning");
                return StatusCode(500, $"Ett fel uppstod: {ex.Message}");
            }
        }

        private async Task UpdateWorkExperiencesAsync(CV existingCv, ICollection<WorkExperience> newExperiences)
        {
            if (newExperiences == null) return;

            _context.WorkExperiences.RemoveRange(existingCv.WorkExperiences);
            existingCv.WorkExperiences.Clear();

            foreach (var exp in newExperiences.Where(e => !string.IsNullOrWhiteSpace(e.Title)))
            {
                if (!string.IsNullOrWhiteSpace(exp.WorkPlace?.Name))
                {
                    var workPlace = await _context.WorkPlaces
                        .FirstOrDefaultAsync(wp => wp.Name == exp.WorkPlace.Name)
                        ?? new WorkPlace { Name = exp.WorkPlace.Name };

                    exp.WorkPlace = workPlace;
                }

                exp.Cv = existingCv;
                exp.CvId = existingCv.Id;
                existingCv.WorkExperiences.Add(exp);
            }
        }

        private async Task UpdateEducationsAsync(CV existingCv, ICollection<Education> newEducations)
        {
            if (newEducations == null) return;

            _context.Educations.RemoveRange(existingCv.Educations);
            existingCv.Educations.Clear();

            foreach (var edu in newEducations.Where(e => !string.IsNullOrWhiteSpace(e.ProgramOrCourseName)))
            {
                if (!string.IsNullOrWhiteSpace(edu.Institution?.Name))
                {
                    var institution = await _context.Institutions
                        .FirstOrDefaultAsync(i => i.Name == edu.Institution.Name)
                        ?? new Institution { Name = edu.Institution.Name };

                    edu.Institution = institution;
                }

                edu.Cv = existingCv;
                edu.CvId = existingCv.Id;
                existingCv.Educations.Add(edu);
            }
        }

        private void UpdateCompetences(CV existingCv, ICollection<Competence> newCompetences)
        {
            if (newCompetences == null) return;

            _context.Competences.RemoveRange(existingCv.Competences);
            existingCv.Competences.Clear();

            foreach (var comp in newCompetences.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
            {
                comp.Cv = existingCv;
                comp.CvId = existingCv.Id;
                existingCv.Competences.Add(comp);
            }
        }
    }
}
