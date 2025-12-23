using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL
{
    public class ProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context) {

            _context = context;
        }

        public List<Project> GetAll()
        {

            return _context.Projects.ToList();
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


    }
}
