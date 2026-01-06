using System;
using System.Collections.Generic;
using System.Text;
using Models;

namespace DAL.Repositories.Interfaces
{
    public interface IProjectUserRepository
    {
        ProjectUser GetProjectUser(string userId, int projectId);

        User GetUserFromProject(string userId, int ProjectId);

        bool AddUserToProject(string userId, int projectId);

        bool RemoveUserFromProject(string userId, int ProjectId);


    }
}
