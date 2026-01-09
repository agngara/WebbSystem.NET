using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Models;
using DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
