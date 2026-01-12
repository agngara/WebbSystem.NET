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

        public bool AddProject(Project project)
        {
            _context.Add(project);
            _context.SaveChanges();
            return true;
        }

        public Task<List<Project>> GetAllAsync()
        {

            return _context.Projects                            
                            .Include(p => p.ProjectUsers)
                            .ThenInclude(pu => pu.User)
                            .Include(p => p.Creator)
                            .ToListAsync();
        }

        /// <summary>
        /// Gets projects, limited to the amount specificed in method arguments.
        /// </summary>
        /// <param name="amountOfProjects"></param>
        /// <returns>A list of projects.</returns>
        public async Task<List<Project>> GetAmount(int amountOfProjects)
        {
            return await _context.Projects
                   .Include(p => p.ProjectUsers)
                   .ThenInclude(pu => pu.User)
                   .Include(p => p.Creator)
                   .Take(amountOfProjects)
                   .ToListAsync();
        }

        /// <summary>
        /// Gets the projects where the entered userId is listed as member of the project.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>A list of projects</returns>
        public async Task<List<Project>> GetProjectsByMember(string userId)
        {

            return await _context.Projects
                            .Include(p => p.ProjectUsers)
                            .ThenInclude(pu => pu.User)
                            .Where(p => p.ProjectUsers
                            .Any(pu => pu.UserId == userId))
                            .ToListAsync();


        }

        /// <summary>
        /// Gets projects by a certain member
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>A list of projects</returns>
        public async Task<List<Project>> GetProjectsByCreator(string userId)
        {
            return await _context.Projects
                            .Include(p => p.ProjectUsers)
                            .ThenInclude(pu => pu.User)
                            .Where(p => p.CreatorId == userId)
                            .ToListAsync();
        }

        public async Task<Project> GetProjectAsync(int projectId)
        {           
            return await _context.Projects.Where(p => p.Id == projectId).FirstOrDefaultAsync();
        }

        public bool UpdateProject(Project project)
        {
            _context.Projects.Update(project);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteProject(int id)
        {
            Project? project = _context.Projects.Find(id);

            if (project != null)
            {
                _context.Remove(project);
                _context.SaveChanges();
                return true;

            }
            else
            {
                return false;
            }
        }
    }
}
