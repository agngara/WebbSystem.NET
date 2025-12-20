using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DAL
{
    public class CVRepository
    {
        private readonly AppDbContext _context;

        public CVRepository(AppDbContext appDbContext) {
            _context = appDbContext;
        }
        /// <summary>
        /// Gets all rows from the CV-table with related data joined in.
        /// </summary>
        /// <returns>A list of CV:s</returns>
        public async Task<List<CV>> GetAllExtendedCVs()
        {
            var query = _context.CVs.Include(c => c.Competences).Include(c => c.WorkExperiences).Include(c => c.Educations).ToList();
            return query;
        }
        // TO BE IMPLEMENTED
        public async Task<List<CV>> GetTop50CVs()
        {
            return new List<CV>();
        }
    }


    


}
