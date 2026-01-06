using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Models;
using System;
using System.Collections.Generic;
using System.Text;


namespace DAL.Repositories
{
    public class ProjectUserRepository : IProjectUserRepository

    {
        private readonly AppDbContext _context;


        public ProjectUserRepository(AppDbContext context)
        {
            _context = context;
        }

        public ProjectUser? GetProjectUser(string userId, int projectId)
        {
            return _context.ProjectUsers.FirstOrDefault(up => up.UserId == userId && up.ProjectId == projectId);
        }

        public User? GetUserFromProject(string userId, int projectId)
        {
            ProjectUser? projectUser = _context.ProjectUsers.FirstOrDefault(up => up.UserId == userId && up.ProjectId == projectId);
            if (projectUser == null) {
                return null;
            }
            else
            {
                return _context.Users.FirstOrDefault(u => u.Id == projectUser.UserId);
            }

        }
        public bool AddUserToProject(string userId, int projectId)
        {

            if (GetUserFromProject(userId, projectId) != null)
            {
                return false;
            }
            else 
            {
                ProjectUser projectUser = new ProjectUser
                {
                    UserId = userId,
                    ProjectId = projectId
                };

                _context.ProjectUsers.Add(projectUser);
                _context.SaveChanges();
                return true;
            }
               
        }

        public bool RemoveUserFromProject(string userId, int projectId)
        {
            ProjectUser? projectUser = GetProjectUser(userId, projectId);

            if (projectUser == null)
            {
                return false; 
            }
            else 
            {
                _context.ProjectUsers.Remove(projectUser);
                _context.SaveChanges();
                return true;
            }
        }


    }
}
