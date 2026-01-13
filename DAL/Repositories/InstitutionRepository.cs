using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DAL.Repositories
{
    public class InstitutionRepository : IInstitutionRepository
    {
        private readonly AppDbContext _context;

        public InstitutionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Institution?> GetByNameAsync(string name)
        {
            return await _context.Institutions
                .FirstOrDefaultAsync(i => i.Name.ToLower() == name.Trim().ToLower());
        }

        public async Task<Institution> AddAsync(Institution institution)
        {
            await _context.Institutions.AddAsync(institution);
            return institution;
        }
    }
}
