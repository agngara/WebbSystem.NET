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

        Task<List<Project>> GetUserProjectsAsync(string userId);


    }
}
