using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context) {

            _context = context;
        }
        
        public Task<List<Project>> GetAllAsync()
        {

            return _context.Projects.ToListAsync();
        }

        /// <summary>
        /// Gets projects, limited to the amount specificed in method arguments.
        /// </summary>
        /// <param name="amountOfProjects"></param>
        /// <returns>A list of projects.</returns>
        public List<Project> GetAmount(int amountOfProjects)
        {
            return _context.Projects.Take(amountOfProjects).ToList();
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            return await _context.Projects
                            .Include(p => p.ProjectUsers)
                            .ThenInclude(pu => pu.User)
                            .Where(p => p.ProjectUsers
                            .Any(pu => pu.UserId == userId))
                            .ToListAsync();
        }
    }
}
