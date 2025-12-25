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

        // GET: CV-sida för inloggad användare
        public async Task<IActionResult> MyCv()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                _logger.LogInformation($"Hämtar CV för användare: {user.Id}");

                // ANVÄND ENKLARE QUERY - hämta bara grunddata först
                var cv = await _context.CVs
                    .AsNoTracking() // Snabbare, ingen change tracking
                    .Where(c => c.UserId == user.Id)
                    .Select(c => new // Anonymt objekt med bara fält som finns
                    {
                        c.Id,
                        c.UserId,
                        c.ProfileImage
                    })
                    .FirstOrDefaultAsync();

                if (cv == null)
                {
                    _logger.LogInformation("CV finns inte, skapar nytt...");

                    // Skapa nytt CV med minimal data
                    var newCv = new CV
                    {
                        UserId = user.Id,
                        User = user,
                        ProfileImage = "/images/default-avatar.png"
                        // Andra listor kommer vara tomma automatiskt
                    };

                    _context.CVs.Add(newCv);
                    await _context.SaveChangesAsync();

                    // Använd det nya CV:t
                    return await LoadFullCvAsync(newCv.Id, user);
                }

                // Hämta fullständigt CV med alla relationer
                return await LoadFullCvAsync(cv.Id, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel i MyCv");

                // Fallback: Visa CV med grunddata
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var fallbackCv = new CV
                    {
                        Id = 0,
                        UserId = user.Id,
                        User = user,
                        ProfileImage = "/images/default-avatar.png",
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

        // Hjälpmetod för att ladda fullständigt CV
        private async Task<IActionResult> LoadFullCvAsync(int cvId, User user)
        {
            // Hämta CV med grunddata
            var cv = await _context.CVs
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == cvId);

            if (cv == null) return NotFound();

            // Hämta relationer separat (mindre risk för fel)
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
                // Fortsätt med tomma listor
                cv.WorkExperiences ??= new List<WorkExperience>();
                cv.Educations ??= new List<Education>();
                cv.Competences ??= new List<Competence>();
            }

            return View(cv);
        }

        // GET: Publik CV-sida
        [AllowAnonymous]
        public async Task<IActionResult> PublicCv(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || user.IsPrivate == true)
                    return NotFound();

                // Hitta CV för användaren
                var cvId = await _context.CVs
                    .Where(c => c.UserId == userId)
                    .Select(c => (int?)c.Id)
                    .FirstOrDefaultAsync();

                if (cvId == null)
                    return NotFound();

                // Hämta CV med all data
                var cv = await _context.CVs
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == cvId);

                if (cv == null) return NotFound();

                // Ladda relationer
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

        // GET: Redigera CV
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
                // Skapa nytt CV om det inte finns
                var newCv = new CV
                {
                    UserId = user.Id,
                    User = user,
                    ProfileImage = "/images/default-avatar.png"
                };

                _context.CVs.Add(newCv);
                await _context.SaveChangesAsync();
                cvId = newCv.Id;
            }

            // Hämta CV med all data
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

        // POST: Redigera CV
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

                // Uppdatera WorkExperiences
                await UpdateWorkExperiencesAsync(existingCv, model.WorkExperiences);

                // Uppdatera Educations
                await UpdateEducationsAsync(existingCv, model.Educations);

                // Uppdatera Competences
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

        // POST: Ladda upp profilbild
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                // Hitta CV
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

                // Validera filtyp
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest("Endast .jpg, .jpeg och .png filer är tillåtna");

                // Validera filstorlek (max 5MB)
                if (profileImage.Length > 5 * 1024 * 1024)
                    return BadRequest("Filen är för stor. Max 5MB");

                // Skapa unikt filnamn
                var fileName = $"profile_{user.Id}_{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");

                // Skapa mapp om den inte finns
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Spara filen
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                // Spara bara URL:en i ProfileImage (vi sparar inte filnamn/content type än)
                cv.ProfileImage = $"/uploads/profiles/{fileName}";
                cv.ProfileImageFileName = fileName;
                cv.ProfileImageContentType = profileImage.ContentType;

                // Radera gamla bilden om den finns
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

        // Hjälpmetoder för att uppdatera data
        private async Task UpdateWorkExperiencesAsync(CV existingCv, ICollection<WorkExperience> newExperiences)
        {
            if (newExperiences == null) return;

            // Ta bort gamla
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
        // Lägg till denna metod i din befintliga CvController.cs
        [HttpGet]
        [Route("/cv/setup")]
        public async Task<IActionResult> SetupDatabase()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial; padding: 20px; background: #f5f5f5; }");
            sb.AppendLine(".success { color: green; }");
            sb.AppendLine(".error { color: red; }");
            sb.AppendLine(".box { background: white; padding: 20px; border-radius: 10px; margin: 10px 0; }");
            sb.AppendLine("</style>");
            sb.AppendLine("<div class='box'>");
            sb.AppendLine("<h1>🛠️ Databas Setup för CV</h1>");

            try
            {
                // 1. Testa databasanslutning
                sb.AppendLine("<h3>1. Database Connection</h3>");
                if (await _context.Database.CanConnectAsync())
                {
                    sb.AppendLine("<p class='success'>✅ Ansluten till databasen</p>");
                }
                else
                {
                    sb.AppendLine("<p class='error'>❌ Kan inte ansluta till databasen</p>");
                    sb.AppendLine("</div>");
                    return Content(sb.ToString(), "text/html");
                }

                // 2. Lägg till saknade kolumner
                sb.AppendLine("<h3>2. Fixar CVs-tabellen</h3>");

                try
                {
                    // SQL för att lägga till de nya kolumnerna
                    var sql = @"
                -- Lägg till ProfileImageFileName om den saknas
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                              WHERE TABLE_NAME = 'CVs' AND COLUMN_NAME = 'ProfileImageFileName')
                BEGIN
                    ALTER TABLE CVs ADD ProfileImageFileName NVARCHAR(MAX) NULL;
                    PRINT 'Lade till ProfileImageFileName';
                END
                ELSE
                BEGIN
                    PRINT 'ProfileImageFileName finns redan';
                END
                
                -- Lägg till ProfileImageContentType om den saknas
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                              WHERE TABLE_NAME = 'CVs' AND COLUMN_NAME = 'ProfileImageContentType')
                BEGIN
                    ALTER TABLE CVs ADD ProfileImageContentType NVARCHAR(MAX) NULL;
                    PRINT 'Lade till ProfileImageContentType';
                END
                ELSE
                BEGIN
                    PRINT 'ProfileImageContentType finns redan';
                END
            ";

                    await _context.Database.ExecuteSqlRawAsync(sql);
                    sb.AppendLine("<p class='success'>✅ Kolumner fixade i CVs-tabellen</p>");
                }
                catch (Exception sqlEx)
                {
                    sb.AppendLine($"<p class='error'>❌ SQL-fel: {sqlEx.Message}</p>");
                }

                // 3. Skapa CV för alla användare som inte har ett
                sb.AppendLine("<h3>3. Skapar CV för användare</h3>");

                var allUsers = await _userManager.Users.ToListAsync();
                var createdCount = 0;

                foreach (var user in allUsers)
                {
                    try
                    {
                        var existingCv = await _context.CVs
                            .FirstOrDefaultAsync(c => c.UserId == user.Id);

                        if (existingCv == null)
                        {
                            var cv = new CV
                            {
                                UserId = user.Id,
                                User = user,
                                ProfileImage = "/images/default-avatar.png",
                                WorkExperiences = new List<WorkExperience>(),
                                Educations = new List<Education>(),
                                Competences = new List<Competence>()
                            };

                            _context.CVs.Add(cv);
                            createdCount++;
                            sb.AppendLine($"<p>✅ Skapade CV för: {user.UserName}</p>");
                        }
                        else
                        {
                            sb.AppendLine($"<p>ℹ️ CV finns redan för: {user.UserName}</p>");
                        }
                    }
                    catch (Exception userEx)
                    {
                        sb.AppendLine($"<p class='error'>❌ Fel för {user.UserName}: {userEx.Message}</p>");
                    }
                }

                await _context.SaveChangesAsync();
                sb.AppendLine($"<p class='success'>✅ Totalt skapade/uppdaterade CV: {createdCount}</p>");

                // 4. Testa att hämta något CV
                sb.AppendLine("<h3>4. Testar CV-hämtning</h3>");
                try
                {
                    var testCv = await _context.CVs
                        .Include(c => c.User)
                        .FirstOrDefaultAsync();

                    if (testCv != null)
                    {
                        sb.AppendLine($"<p class='success'>✅ Testlyckat! Hittade CV för: {testCv.User?.UserName}</p>");
                    }
                    else
                    {
                        sb.AppendLine("<p>ℹ️ Inga CV:n i databasen än</p>");
                    }
                }
                catch (Exception testEx)
                {
                    sb.AppendLine($"<p class='error'>❌ Testfel: {testEx.Message}</p>");
                }

                sb.AppendLine("<h2 class='success'>🎉 Setup klar!</h2>");
                sb.AppendLine("<p>Databasen är nu redo för CV-systemet.</p>");
                sb.AppendLine("<div style='margin-top: 20px;'>");
                sb.AppendLine("<a href='/cv/mycv' class='btn' style='background: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin-right: 10px;'>Testa mitt CV</a>");
                sb.AppendLine("<a href='/Account/Register' class='btn' style='background: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Skapa ny användare</a>");
                sb.AppendLine("</div>");

            }
            catch (Exception ex)
            {
                sb.AppendLine($"<h3 class='error'>❌ Allvarligt fel:</h3>");
                sb.AppendLine($"<pre style='background: #ffe6e6; padding: 10px; border-radius: 5px;'>{ex.Message}</pre>");
                sb.AppendLine($"<pre style='background: #fff0f0; padding: 10px; border-radius: 5px;'>{ex.StackTrace}</pre>");
            }

            sb.AppendLine("</div>");
            return Content(sb.ToString(), "text/html");
        }
    }
}