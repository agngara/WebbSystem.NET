using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories.Interfaces
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllAsync();

        List<Project> GetAmount(int amountOfProjects);

        Task<List<Project>> GetProjectsByMembers(string userId);

        Task<List<Project>> GetProjectsByCreator(string userId);

        bool AddProject(Project project);

        Task<Project> GetProjectAsync(int projectId);


    }
}
