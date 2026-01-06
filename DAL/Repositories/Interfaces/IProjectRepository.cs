using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Repositories.Interfaces
{
    public interface IProjectRepository
    {
        bool AddProject(Project project);

        Task<List<Project>> GetAllAsync();

        List<Project> GetAmount(int amountOfProjects);

        Task<List<Project>> GetProjectsByMembers(string userId);

        Task<List<Project>> GetProjectsByCreator(string userId);


        Task<Project> GetProjectAsync(int projectId);

        bool UpdateProject(Project project);

        bool DeleteProject(int id);



    }
}
