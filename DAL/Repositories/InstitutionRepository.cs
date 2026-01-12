using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories
{

    public class InstitutionRepository : IInstitutionRepository
    {
        private readonly AppDbContext _context;

        public InstitutionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Institution> GetInstitutionByNameAsync(string institutionName)
        {
            return await _context.Institutions
                            .FirstOrDefaultAsync(i => i.Name.ToLower() == institutionName.Trim().ToLower());
        }

        public async Task AddInstitutionAsync(Institution institution)
        {
            _context.Institutions.Add(institution);
            _context.SaveChanges();
        }
    }
}
