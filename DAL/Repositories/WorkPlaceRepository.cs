using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Models;
using DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class WorkPlaceRepository : IWorkPlaceRepository
    {
        private readonly AppDbContext _context;

        public WorkPlaceRepository(AppDbContext context) 
        { 
            _context = context; 
        }

        public async Task<WorkPlace?> GetWorkPlaceFromIdAsync(int? workplaceId)
        {
            return await _context.WorkPlaces.FindAsync(workplaceId);
        }

        public async Task<WorkPlace?> GetWorkPlaceFromNameAsync(string workplaceName)
        {
            return await _context.WorkPlaces.FirstOrDefaultAsync(wp => wp.Name.ToLower() == workplaceName.Trim().ToLower());
        }

        public async Task AddWorkPlaceAsync(WorkPlace workPlace)
        {
            await _context.WorkPlaces.AddAsync(workPlace);
            await _context.SaveChangesAsync();
        }


    }
}
