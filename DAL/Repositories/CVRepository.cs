using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DAL.Repositories
{
    public class CVRepository : SqlServerRepository<CV>, ICVRepository
    {
        private readonly AppDbContext _context;

        public CVRepository(AppDbContext appDbContext) : base(appDbContext)
        {
            _context = appDbContext;
        }

        public async Task<CV> Add(CV entity)
        {
            await _context.CVs.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Gets all rows from the CV-table with related data joined in.
        /// </summary>
        /// <returns>A list of CV:s</returns>
        public async Task<List<CV>> GetAllExtendedCVs()
        {
            var query = _context.CVs.Include(c => c.Competences)
                                    .Include(c => c.WorkExperiences)
                                    .Include(c => c.Educations)
                                    .Include(c => c.User)
                                    .ToList();
            return query;
        }
        /// <summary>
        /// Getter limited to 20 rows and ordered descending by profile views. 
        /// </summary>
        /// <returns></returns>
        public async Task<List<CV>> GetTop20CVs()
        {
            return await _context.CVs.Include(c => c.Competences)
                        .Include(c => c.WorkExperiences)
                        .Include(c => c.Educations)
                        .Include(c => c.User)
                        .OrderByDescending(c => c.ProfileViews)
                        .Take(20)
                        .ToListAsync();

        }

        public async Task<List<CV>> GetTop20PublicCVs()
        {
            return await _context.CVs.Include(c => c.Competences)
            .Include(c => c.WorkExperiences)
            .Include(c => c.Educations)
            .Include(c => c.User)
            .Where(c => c.User.IsActive == true && c.User.IsPrivate == false)
            .OrderByDescending(c => c.ProfileViews)
            .Take(20)
            .ToListAsync();
        }


        public async Task<CV> GetCVForCV(int cvId)
        {
            return _context.CVs
                            .Include(c => c.Competences)
                            .Include(c => c.WorkExperiences)
                            .ThenInclude(w => w.WorkPlace)
                            .Include(c => c.Educations)
                            .ThenInclude(e => e.Institution)
                            .Include(c => c.User)
                            .FirstOrDefault(c => c.Id == cvId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
