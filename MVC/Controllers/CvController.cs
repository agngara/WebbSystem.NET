using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using DAL;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace MVC.Controllers
{
    [Authorize]
    public class CvController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public CvController(AppDbContext context, UserManager<User> userManager,
                            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: CV-sida för inloggad användare
        public async Task<IActionResult> MyCv()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return NotFound();

            // Hämta CV med relaterad data
            var cv = await _context.CVs
                .Include(c => c.User)
                .Include(c => c.WorkExperiences)
                    .ThenInclude(we => we.WorkPlace)
                .Include(c => c.Educations)
                    .ThenInclude(e => e.Institution)
                .Include(c => c.Competences)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            // Skapa nytt CV om det inte finns
            if (cv == null)
            {
                cv = new CV
                {
                    UserId = user.Id,
                    User = user,
                    ProfileImage = null,
                    ProfileImageType = null,
                    WorkExperiences = new List<WorkExperience>(),
                    Educations = new List<Education>(),
                    Competences = new List<Competence>()
                };
                _context.CVs.Add(cv);
                await _context.SaveChangesAsync();
            }

            return View(cv);
        }

        // GET: Publik CV-sida för alla användare
        [AllowAnonymous]
        public async Task<IActionResult> PublicCv(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsPrivate == true)
            {
                TempData["ErrorMessage"] = "CV:t är inte tillgängligt eller användaren har privata inställningar.";
                return RedirectToAction("Index", "Home");
            }

            var cv = await _context.CVs
                .Include(c => c.User)
                .Include(c => c.WorkExperiences)
                    .ThenInclude(we => we.WorkPlace)
                .Include(c => c.Educations)
                    .ThenInclude(e => e.Institution)
                .Include(c => c.Competences)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cv == null)
            {
                TempData["ErrorMessage"] = "CV:t hittades inte.";
                return RedirectToAction("Index", "Home");
            }

            return View(cv);
        }

        // GET: Redigera CV (öppnar modal)
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await GetCurrentUserAsync();
            var cv = await GetOrCreateCvForUserAsync(user.Id);

            return View(cv);
        }

        // POST: Redigera CV (hanterar modal-submit med bilduppladdning)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CV model, IFormFile? newProfileImage)
        {
            if (!ModelState.IsValid)
            {
                // Om AJAX-förfrågan, returnera JSON-fel
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors) });
                }
                return View(model);
            }

            try
            {
                var user = await GetCurrentUserAsync();
                var existingCv = await GetOrCreateCvForUserAsync(user.Id);

                // 1. HANTERA PROFILBILD
                if (newProfileImage != null && newProfileImage.Length > 0)
                {
                    // Validera filstorlek (max 5MB)
                    if (newProfileImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "Filen är för stor. Max 5MB.");
                        return View(model);
                    }

                    // Validera filtyp
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(newProfileImage.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Endast JPG, PNG och GIF tillåtna.");
                        return View(model);
                    }

                    // Konvertera bilden till byte array
                    using (var memoryStream = new MemoryStream())
                    {
                        await newProfileImage.CopyToAsync(memoryStream);
                        existingCv.ProfileImage = memoryStream.ToArray();
                        existingCv.ProfileImageType = newProfileImage.ContentType;
                    }
                }
                else if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    // Behåll befintlig bild från dolda fält
                    existingCv.ProfileImage = model.ProfileImage;
                    existingCv.ProfileImageType = model.ProfileImageType;
                }

                // 2. HANTERA ARBETSERFARENHETER
                if (model.WorkExperiences != null)
                {
                    await UpdateWorkExperiencesAsync(existingCv, model.WorkExperiences);
                }

                // 3. HANTERA UTBILDNINGAR
                if (model.Educations != null)
                {
                    await UpdateEducationsAsync(existingCv, model.Educations);
                }

                // 4. HANTERA KOMPETENSER
                if (model.Competences != null)
                {
                    await UpdateCompetencesAsync(existingCv, model.Competences);
                }

                await _context.SaveChangesAsync();

                // Om AJAX-förfrågan, returnera JSON-svar
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // Skapa bild-URL för respons
                    string imageUrl = null;
                    if (existingCv.ProfileImage != null && existingCv.ProfileImage.Length > 0)
                    {
                        imageUrl = $"data:{existingCv.ProfileImageType};base64,{Convert.ToBase64String(existingCv.ProfileImage)}";
                    }

                    return Json(new
                    {
                        success = true,
                        message = "CV uppdaterat framgångsrikt!",
                        imageUrl = imageUrl,
                        redirectUrl = Url.Action("MyCv", "Cv")
                    });
                }

                TempData["SuccessMessage"] = "Ditt CV har uppdaterats!";
                return RedirectToAction(nameof(MyCv));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception i Edit: {ex.Message}\n{ex.StackTrace}");
                ModelState.AddModelError("", $"Ett fel uppstod: {ex.Message}");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = ex.Message });
                }
                return View(model);
            }
        }

        // POST: Separat bilduppladdning (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
            {
                return Json(new { success = false, error = "Ingen fil vald." });
            }

            // Validera filstorlek (max 5MB)
            if (profileImage.Length > 5 * 1024 * 1024)
            {
                return Json(new { success = false, error = "Filen är för stor. Max 5MB." });
            }

            // Validera filtyp
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, error = "Endast JPG, PNG och GIF tillåtna." });
            }

            try
            {
                var user = await GetCurrentUserAsync();
                var cv = await GetOrCreateCvForUserAsync(user.Id);

                // Konvertera bilden till byte array
                using (var memoryStream = new MemoryStream())
                {
                    await profileImage.CopyToAsync(memoryStream);
                    cv.ProfileImage = memoryStream.ToArray();
                    cv.ProfileImageType = profileImage.ContentType;
                }

                await _context.SaveChangesAsync();

                // Skapa en data URL för att visa bilden direkt
                var imageDataUrl = $"data:{cv.ProfileImageType};base64,{Convert.ToBase64String(cv.ProfileImage)}";

                return Json(new
                {
                    success = true,
                    imageUrl = imageDataUrl,
                    message = "Profilbild uppdaterad!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Ett fel uppstod: {ex.Message}" });
            }
        }

        // GET: Ta bort arbetserfarenhet (AJAX)
        [HttpDelete]
        public async Task<IActionResult> DeleteWorkExperience(int id)
        {
            var experience = await _context.WorkExperiences.FindAsync(id);
            if (experience == null)
                return Json(new { success = false, error = "Arbetserfarenheten hittades inte." });

            // Kontrollera att användaren äger denna erfarenhet
            var cv = await _context.CVs.FirstOrDefaultAsync(c => c.Id == experience.CvId);
            var user = await GetCurrentUserAsync();

            if (cv == null || cv.UserId != user.Id)
                return Json(new { success = false, error = "Du har inte behörighet att ta bort detta." });

            _context.WorkExperiences.Remove(experience);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // GET: Ta bort utbildning (AJAX)
        [HttpDelete]
        public async Task<IActionResult> DeleteEducation(int id)
        {
            var education = await _context.Educations.FindAsync(id);
            if (education == null)
                return Json(new { success = false, error = "Utbildningen hittades inte." });

            // Kontrollera att användaren äger denna utbildning
            var cv = await _context.CVs.FirstOrDefaultAsync(c => c.Id == education.CvId);
            var user = await GetCurrentUserAsync();

            if (cv == null || cv.UserId != user.Id)
                return Json(new { success = false, error = "Du har inte behörighet att ta bort detta." });

            _context.Educations.Remove(education);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // Hjälpmetod: Hämta aktuell användare
        private async Task<User> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User);
        }

        // Hjälpmetod: Hämta eller skapa CV för användare
        private async Task<CV> GetOrCreateCvForUserAsync(string userId)
        {
            var cv = await _context.CVs
                .Include(c => c.WorkExperiences)
                .Include(c => c.Educations)
                .Include(c => c.Competences)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cv == null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                cv = new CV
                {
                    UserId = userId,
                    User = user,
                    ProfileImage = null,
                    ProfileImageType = null,
                    WorkExperiences = new List<WorkExperience>(),
                    Educations = new List<Education>(),
                    Competences = new List<Competence>()
                };
                _context.CVs.Add(cv);
                await _context.SaveChangesAsync();
            }

            return cv;
        }

        // Hjälpmetod: Uppdatera arbetserfarenheter
        private async Task UpdateWorkExperiencesAsync(CV existingCv, ICollection<WorkExperience> newExperiences)
        {
            if (newExperiences == null) return;

            // Ta bort befintliga
            _context.WorkExperiences.RemoveRange(existingCv.WorkExperiences);
            existingCv.WorkExperiences.Clear();

            // Lägg till nya (med WorkPlace-hantering)
            foreach (var exp in newExperiences.Where(e => !string.IsNullOrWhiteSpace(e.Title)))
            {
                // Hantera WorkPlace om det finns
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

        // Hjälpmetod: Uppdatera utbildningar
        private async Task UpdateEducationsAsync(CV existingCv, ICollection<Education> newEducations)
        {
            if (newEducations == null) return;

            // Ta bort befintliga
            _context.Educations.RemoveRange(existingCv.Educations);
            existingCv.Educations.Clear();

            // Lägg till nya (med Institution-hantering)
            foreach (var edu in newEducations.Where(e => !string.IsNullOrWhiteSpace(e.ProgramOrCourseName)))
            {
                // Hantera Institution om det finns
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

        // Hjälpmetod: Uppdatera kompetenser
        private Task UpdateCompetencesAsync(CV existingCv, ICollection<Competence> newCompetences)
        {
            if (newCompetences == null) return Task.CompletedTask;

            // Ta bort befintliga
            _context.Competences.RemoveRange(existingCv.Competences);
            existingCv.Competences.Clear();

            // Lägg till nya
            foreach (var comp in newCompetences.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
            {
                comp.Cv = existingCv;
                comp.CvId = existingCv.Id;
                existingCv.Competences.Add(comp);
            }

            return Task.CompletedTask;
        }
    }
}