using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using DAL;

namespace MVC.Controllers
{
    [Authorize]
    public class CvController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public CvController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: CV-sida för inloggad användare
        public async Task<IActionResult> MyCv()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // SIMPLIFIERA - ta bort ThenInclude
            var cv = await _context.CVs
                .Include(c => c.User)
                .Include(c => c.WorkExperiences)
                .Include(c => c.Educations)
                .Include(c => c.Competences)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            // Skapa nytt CV om det inte finns
            if (cv == null)
            {
                cv = new CV
                {
                    UserId = user.Id,
                    User = user,
                    ProfileImage = "/images/default-avatar.png",
                    WorkExperiences = new List<WorkExperience>(),
                    Educations = new List<Education>(),
                    Competences = new List<Competence>()
                };
                _context.CVs.Add(cv);
                await _context.SaveChangesAsync();
            }

            return View(cv);
        }

        // GET: Publik CV-sida
        [AllowAnonymous]
        public async Task<IActionResult> PublicCv(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsPrivate == true)
                return NotFound();

            var cv = await _context.CVs
                .Include(c => c.User)
                .Include(c => c.WorkExperiences)
                .Include(c => c.Educations)
                .Include(c => c.Competences)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cv == null) return NotFound();
            return View(cv);
        }

        // GET: Redigera CV
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            var cv = await GetOrCreateCvForUserAsync(user.Id);
            return View(cv);
        }

        // POST: Redigera CV
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CV model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var existingCv = await GetOrCreateCvForUserAsync(user.Id);

                // Uppdatera grundläggande properties
                existingCv.ProfileImage = model.ProfileImage;

                // Hantera WorkExperiences
                await UpdateWorkExperiencesAsync(existingCv, model.WorkExperiences);

                // Hantera Educations
                await UpdateEducationsAsync(existingCv, model.Educations);

                // Hantera Competences
                await UpdateCompetencesAsync(existingCv, model.Competences);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyCv));
            }
            return View(model);
        }

        // Hjälpmetod: Hämta eller skapa CV
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
                    ProfileImage = "/images/default-avatar.png",
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

        // Hjälpmetod: Uppdatera utbildningar
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

        // Hjälpmetod: Uppdatera kompetenser
        private Task UpdateCompetencesAsync(CV existingCv, ICollection<Competence> newCompetences)
        {
            if (newCompetences == null) return Task.CompletedTask;

            _context.Competences.RemoveRange(existingCv.Competences);
            existingCv.Competences.Clear();

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